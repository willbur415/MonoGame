// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    [CLSCompliant (false)]
    public class MonoGameAndroidGameView : SurfaceView, ISurfaceHolderCallback, View.IOnTouchListener
    {
        // What is the state of the app, for tracking surface recreation inside this class.
        // This acts as a replacement for the all-out monitor wait approach which caused code to be quite fragile.
        enum InternalState
        {          
            Pausing_UIThread,  // set by android UI thread and the game thread process it and transitions into 'Paused' state
            Resuming_UIThread, // set by android UI thread and the game thread process it and transitions into 'Running' state
            Exiting,           // set either by game or android UI thread and the game thread process it and transitions into 'Exited' state          

            Paused_GameThread,  // set by game thread after processing 'Pausing' state
            Running_GameThread, // set by game thread after processing 'Resuming' state
            Exited_GameThread,  // set by game thread after processing 'Exiting' state

            ForceRecreateSurface, // also used to create the surface the 1st time or when screen orientation changes
        }

        bool disposed = false;
        ISurfaceHolder mHolder;
        Size size;


        object _lock = new object ();
        volatile InternalState _internalState = InternalState.Exited_GameThread;


        bool androidSurfaceAvailable;

        bool glSurfaceAvailable;
        bool glContextAvailable;
        bool lostglContext;  
        System.Diagnostics.Stopwatch stopWatch;
        double tick = 0;

        bool loaded = false;

        Task renderTask;
        CancellationTokenSource cts = null;
        private readonly AndroidTouchEventManager _touchManager;
        private readonly AndroidGameWindow _gameWindow;
        private readonly Game _game;

        const int EglContextClientVersion = 0x3098;

        // Events that are triggered on the game thread
        public static event EventHandler OnPauseGameThread;
        public static event EventHandler OnResumeGameThread;

        public bool TouchEnabled {
            get { return _touchManager.Enabled; }
            set {
                _touchManager.Enabled = value;
                SetOnTouchListener (value ? this : null);
            }
        }

        public bool IsResuming { get; private set; }

        public MonoGameAndroidGameView (Context context, AndroidGameWindow gameWindow, Game game)
            : base (context)
        {
            _gameWindow = gameWindow;
            _game = game;
            _touchManager = new AndroidTouchEventManager (gameWindow);
            Init ();
        }

        private void Init ()
        {
            Log.Verbose ("AndroidGameView", "Init: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            // default
            mHolder = Holder;
            // Add callback to get the SurfaceCreated etc events
            mHolder.AddCallback (this);
            mHolder.SetType (SurfaceType.Gpu);
            OpenGL.GL.LoadEntryPoints();

            Log.Verbose ("AndroidGameView", "Init end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        public void SurfaceChanged (ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            Log.Verbose ("AndroidGameView", "SurfaceChanged 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (_lock)
            {
                Log.Verbose ("AndroidGameView", "SurfaceChanged 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                // Set flag to recreate gl surface or rendering can be bad on orienation change or if app 
                // is closed in one orientation and re-opened in another.
                if(_internalState != InternalState.Resuming_UIThread) // must not overwrite UI thread resume command at app resume
                    _internalState = InternalState.ForceRecreateSurface;
               
                Log.Verbose ("AndroidGameView", "SurfaceChanged end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            }
        }

        public void SurfaceCreated (ISurfaceHolder holder)
        {
            Log.Verbose ("AndroidGameView", "SurfaceCreated 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (_lock)
            {
                Log.Verbose ("AndroidGameView", "SurfaceCreated 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                androidSurfaceAvailable = true;            
            }
            Log.Verbose ("AndroidGameView", "SurfaceCreated end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        public void SurfaceDestroyed (ISurfaceHolder holder)
        {
            Log.Verbose ("AndroidGameView", "SurfaceDestroyed 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (_lock)
            {
                Log.Verbose ("AndroidGameView", "SurfaceDestroyed 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                androidSurfaceAvailable = false;
                Log.Verbose ("AndroidGameView", "SurfaceDestroyed 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
               /* while (glSurfaceAvailable)
                {
                  //  Monitor.Wait (lockObject);
                  // todo: add sleep a bit or something to release cpu cycles?
                }*/
                Log.Verbose ("AndroidGameView", "SurfaceDestroyed 4: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            }
            Log.Verbose ("AndroidGameView", "SurfaceDestroyed end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
        }

        public bool OnTouch (View v, MotionEvent e)
        {
            _touchManager.OnTouchEvent (e);
            return true;
        }

        public virtual void SwapBuffers ()
        {
            EnsureUndisposed ();
            if (!egl.EglSwapBuffers (eglDisplay, eglSurface)) {
                if (egl.EglGetError () == 0) {
                    if (lostglContext)
                        System.Diagnostics.Debug.WriteLine ("Lost EGL context" + GetErrorAsString ());
                    lostglContext = true;
                }
            }

        }

        public virtual void MakeCurrent ()
        {
            EnsureUndisposed ();
            if (!egl.EglMakeCurrent (eglDisplay, eglSurface,
                    eglSurface, eglContext)) {
                System.Diagnostics.Debug.WriteLine ("Error Make Current" + GetErrorAsString ());
            }

        }

        public virtual void ClearCurrent ()
        {
            EnsureUndisposed ();
            if (!egl.EglMakeCurrent (eglDisplay, EGL10.EglNoSurface,
                EGL10.EglNoSurface, EGL10.EglNoContext)) {
                System.Diagnostics.Debug.WriteLine ("Error Clearing Current" + GetErrorAsString ());
            }
        }

        double updates;

        public bool LogFPS { get; set; }
        public bool RenderOnUIThread { get; set; }

        public virtual void Run ()
        {
            cts = new CancellationTokenSource ();
            if (LogFPS) {
                targetFps = currentFps = 0;
                avgFps = 1;
            }
            updates = 0;
            var syncContext = new SynchronizationContext ();
            renderTask = Task.Factory.StartNew (() => 
            {
                if (RenderOnUIThread)
                {
                    syncContext.Send ((s) =>
                    {
                        RenderLoop (cts.Token);
                    }, null);
                } else
                    RenderLoop (cts.Token);
            }, cts.Token)
                .ContinueWith ((t) => 
                {
                    OnStopped (EventArgs.Empty);
                });

        }

        public virtual void Run (double updatesPerSecond)
        {
            cts = new CancellationTokenSource ();
            if (LogFPS) {
                avgFps = targetFps = currentFps = updatesPerSecond;
            }
            updates = 1000 / updatesPerSecond;
            var syncContext = new SynchronizationContext ();
            renderTask = Task.Factory.StartNew (() => {
                if (RenderOnUIThread) {
                    syncContext.Send ((s) => {
                        RenderLoop (cts.Token);
                    }, null);
                } else
                    RenderLoop (cts.Token);
            }, cts.Token);
        }

        public virtual void Pause ()
        {

            Log.Verbose ("AndroidGameView", "Pause 0: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
         
            EnsureUndisposed ();

            Log.Verbose ("AndroidGameView", "Pause 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (_lock)
            {
                Log.Verbose ("AndroidGameView", "Pause 11: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                _internalState = InternalState.Pausing_UIThread;
            }

            Log.Verbose ("AndroidGameView", "Pause 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            // wait until game thread transitions to Paused state so we know its finished processing everything
            while(true)
            {
                lock(_lock)
                {
                    if (_internalState == InternalState.Paused_GameThread)
                    {
                        Log.Verbose ("AndroidGameView", "Pause 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                        break;
                    }
                }
            }

            Log.Verbose ("AndroidGameView", "Pause end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        public virtual void Resume ()
        {
            EnsureUndisposed ();
            Log.Verbose ("AndroidGameView", "Resume 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            Log.Verbose ("AndroidGameView", "Resume 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (_lock)
            {
                _internalState = InternalState.Resuming_UIThread;
                Monitor.PulseAll (_lock); // restart main loop if it was paused previously

                try
                {
                    if (!IsFocused)
                        RequestFocus ();
                }
                catch {  }
            }

            // do not wait for state transition here since surface creation must be triggered first

            Log.Verbose ("AndroidGameView", "Resume 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
        

            Log.Verbose ("AndroidGameView", "Resume end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        protected override void Dispose (bool disposing)
        {
            if (disposing) {
                Stop ();
            }
            base.Dispose (disposing);
        }

        public async void Stop ()
        {
            Log.Verbose ("AndroidGameView", "Stop 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            EnsureUndisposed ();
            if (cts != null) {
                Log.Verbose ("AndroidGameView", "Stop 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                // todo: try triggering this to see if we need an additional 'Stopping' state
                lock (_lock)
                {
                    _internalState = InternalState.Exiting;
                }
                Log.Verbose ("AndroidGameView", "Stop 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                cts.Cancel ();

                // wait for game thread to process existing
                while(true)
                {
                    lock(_lock)
                    {
                        if(_internalState == InternalState.Exited_GameThread)
                        {
                            break;
                        }
                    }

                    await Task.Delay (100); // todo sleep?                    
                }
               
                Log.Verbose ("AndroidGameView", "Stop 4: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                //renderTask.Wait ();
                Log.Verbose ("AndroidGameView", "Stop 5: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            }
            Log.Verbose ("AndroidGameView", "Stop end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        FrameEventArgs renderEventArgs = new FrameEventArgs ();

        public volatile static int s = -1;
        protected void RenderLoop (CancellationToken token)
        {
            s = 0;
            Threading.ResetThread (Thread.CurrentThread.ManagedThreadId);
            try
            {
                stopWatch = System.Diagnostics.Stopwatch.StartNew ();
                tick = 0;
                prevUpdateTime = DateTime.Now;
                s = 1;
                while (!cts.IsCancellationRequested)
                {
                    s = 101;
                    // set main game thread global ID
                    Threading.ResetThread (Thread.CurrentThread.ManagedThreadId);
                    s = 102;
                    InternalState currentState = InternalState.Exited_GameThread;
                    lock(_lock)
                    {
                        s = 103;
                        currentState = _internalState;
                    }

                    // THIS CAN ONLY BE SET BY THE EXISTING STATE OR IN CASE OF ERROR, NO OTHER STATE MUST SET THIS OR STATES CAN BE MISSED!
                    bool exitGameLoop = false;
                    s = 104;
                    switch (currentState)
                    {
                        // exit states
                        case InternalState.Exiting: // when ui thread wants to exit
                            s = 105;
                            processStateExiting ();
                            break;

                        case InternalState.Exited_GameThread: // when game thread processed exiting event
                            s = 106;
                            exitGameLoop = true;
                            break;

                        // pause states
                        case InternalState.Pausing_UIThread: // when ui thread wants to pause
                            s = 107;
                            processStatePausing ();
                            s = 108;
                            break;

                        case InternalState.Paused_GameThread: // when game thread processed pausing event
                            s = 108;
                            lock (_lock)
                            {
                                s = 109;
                                Monitor.Wait (_lock); // stop this thread until resume is triggered 
                            }
                            break;

                        // other states
                        case InternalState.Resuming_UIThread: // when ui thread wants to resume
                            s = 110;
                            processStateResuming ();
                            break;

                        case InternalState.Running_GameThread: // when we are running game 
                            s = 111;
                            processStateRunning (token);                                                   
                            break;

                        case InternalState.ForceRecreateSurface:
                            s = 112;
                            processStateForceSurfaceRecreation ();
                            break;

                        // default case, error
                        default:
                            processStateDefault ();
                            exitGameLoop = true;
                            break;
                    }

                    // if game wants to exit OR if error happens so that we rather exit than hang app
                    if(exitGameLoop)
                    {
                        break;
                    }

                    s = 2;
                    // MUST BE CALLED BEFORE IsGLSurfaceAvailable! because the latter blocks.
                    s = 3;
                    // this waits when android onPause callback is triggered, resumes when onResume is called (by pinging Monitor)
                   /* if (!IsGLSurfaceAvailableWaitIfNot ())
                    {
                        s = 302;

                        continue;
                    }*/
                    s = 303;
                  

                    s = 4;
                    
                    s = 5;
                   
                    s = 7;
                }
                Log.Verbose ("AndroidGameView", "RenderLoop exited: w: "+w+", s: "+s + ", tid: " + Environment.CurrentManagedThreadId);
            }
            catch (Exception ex)
            {
                Log.Error ("AndroidGameView", ex.ToString ());
            }
            finally
            {
                bool c = cts.IsCancellationRequested;
                s = 8;
                lock (_lock)
                {
                    s = 9;
                    
                    cts = null;

                    if (glSurfaceAvailable)
                        DestroyGLSurface ();
                    s = 10;
                    if (glContextAvailable)
                    {
                        s = 11;
                        DestroyGLContext ();
                        s = 12;
                        ContextLostInternal ();
                        s = 13;
                    }

                    _internalState = InternalState.Exited_GameThread;
                }
            }
        }

        DateTime prevUpdateTime;
        DateTime prevRenderTime;
        DateTime curUpdateTime;
        DateTime curRenderTime;
        FrameEventArgs updateEventArgs = new FrameEventArgs ();

        void processStateDefault()
        {
            Log.Error ("AndroidGameView", "Default case for switch on InternalState in main game loop, exiting");
            lock (_lock)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }

        void processStateRunning(CancellationToken  token)
        {
            s = 112;
            // do not run game if surface is not avalible
            lock (_lock)
            {
                s = 113;
                if ( !androidSurfaceAvailable )
                {
                    return;
                }
            }

            // check if app wants to exit
            if (token.IsCancellationRequested)
            {
                s = 114;
                // change state to exit and skip game loop
                lock (_lock)
                {
                    s = 115;
                    _internalState = InternalState.Exiting;
                }
                return;
            }
            s = 116;
            try
            {
                RunIteration ();
            }
            catch (MonoGameGLException ex)
            {
                Log.Error ("AndroidGameView", "GL Exception occured during RunIteration {0}", ex.Message);
            }
            s = 6;

            if (updates > 0)
            {
                var t = updates - (stopWatch.Elapsed.TotalMilliseconds - tick);
                if (t > 0)
                {
                    if (LogFPS)
                    {
                        Log.Verbose ("AndroidGameView", "took {0:F2}ms, should take {1:F2}ms, sleeping for {2:F2}", stopWatch.Elapsed.TotalMilliseconds - tick, updates, t);
                    }
                   
                }
            }

        }

        void processStatePausing ()
        {
            if (glSurfaceAvailable)
            {
                // Surface we are using needs to go away
                DestroyGLSurface ();

                w = 4;

                if (loaded)
                    OnUnload (EventArgs.Empty);

                w = 5;
            }

            // trigger callbacks, must pause openAL device here
            OnPauseGameThread (this, EventArgs.Empty);

            // go to next state
            lock (_lock)
            {
                _internalState = InternalState.Paused_GameThread;
            }
        }

        void processStateResuming ()
        {
            w = 6;
 
            // do not execute yet, must wait for android callbacks that surface was created
            lock (_lock)
            {
                if (!androidSurfaceAvailable)
                    return;
            }

            // create surface if context is avalible
            if ( glContextAvailable && !lostglContext)
            {
                try
                {
                    w = 7;
                    CreateGLSurface ();
                    w = 8;
                }
                catch (Exception ex)
                {
                    // We failed to create the surface for some reason
                    Log.Verbose ("AndroidGameView", ex.ToString ());
                }
                w = 81;
            }

            // create context if not avalible
            if ( (!glContextAvailable || lostglContext))
            {
                // Start or Restart due to context loss
                bool contextLost = false;
                if (lostglContext || glContextAvailable)
                {
                    // we actually lost the context
                    // so we need to free up our existing 
                    // objects and re-create one.
                    w = 9;
                    DestroyGLContext ();
                    contextLost = true;
                    w = 10;
                    Log.Verbose ("AndroidGameView", "ContentLostInternal");
                    ContextLostInternal ();
                }
                w = 11;
                CreateGLContext ();
                w = 12;
                CreateGLSurface ();
                w = 13;
                if (!loaded && glContextAvailable)
                    OnLoad (EventArgs.Empty);
                w = 14;
                if (contextLost && glContextAvailable)
                {
                    Log.Verbose ("AndroidGameView", "ContentSetInternal");
                    // we lost the gl context, we need to let the programmer
                    // know so they can re-create textures etc.
                    ContextSetInternal ();
                    w = 15;
                }
            }
            else if( glSurfaceAvailable ) // finish state if surface created, may take a frame or two until the android UI thread callbacks fire
            {
                w = 81;

                // trigger callbacks, must resume openAL device here
                OnResumeGameThread (this, EventArgs.Empty);

                w = 82;

                // go to next state
                lock (_lock)
                {
                    w = 83;
                    _internalState = InternalState.Running_GameThread;
                }
                w = 84;
            }
            w = 85;
        }

        void processStateExiting ()
        {
            // go to next state
            lock (_lock)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }

        void processStateForceSurfaceRecreation ()
        {
            lock (_lock)
            {
                // needed at app start
                if( !androidSurfaceAvailable || !glContextAvailable)
                {
                    return;
                }
            }

                w = 16;
         
            DestroyGLSurface ();
            w = 17;
            CreateGLSurface ();
            w = 18;

            // go to next state
            lock (_lock)
            {
                _internalState = InternalState.Running_GameThread;
            }
        }

        void UpdateFrameInternal (FrameEventArgs e)
        {
            s = 11801;
            OnUpdateFrame (e);
            s = 11802;
            if (UpdateFrame != null)
            {
                s = 11803;
                UpdateFrame (this, e);
                s = 11804;
            }
              
        }

        protected virtual void OnUpdateFrame (FrameEventArgs e)
        {

        }

        // this method is called on the main thread
        void RunIteration ()
        {
            //  if (token.IsCancellationRequested) // todo, no problem right if this is not used I presume, since 
            //  return;
            s = 117;
            curUpdateTime = DateTime.Now;
            if (prevUpdateTime.Ticks != 0) {
                var t = (curUpdateTime - prevUpdateTime).TotalMilliseconds;
                updateEventArgs.Time = t < 0 ? 0 : t;
            }
            s = 118;
            try {
                UpdateFrameInternal (updateEventArgs);
            } catch (Content.ContentLoadException) {
                // ignore it..
            }
            s = 119;
            prevUpdateTime = curUpdateTime;

            curRenderTime = DateTime.Now;
            if (prevRenderTime.Ticks == 0) {
                var t = (curRenderTime - prevRenderTime).TotalMilliseconds;
                renderEventArgs.Time = t < 0 ? 0 : t;
            }
            s = 120;
            RenderFrameInternal (renderEventArgs);
            s = 121;
            prevRenderTime = curRenderTime;
            s = 122;
        }

        void RenderFrameInternal (FrameEventArgs e)
        {
            if (LogFPS) {
                Mark ();
            }
            OnRenderFrame (e);
            if (RenderFrame != null)
                RenderFrame (this, e);
        }

        protected virtual void OnRenderFrame (FrameEventArgs e)
        {

        }

        int frames = 0;
        double prev = 0;
        double avgFps = 0;
        double currentFps = 0;
        double targetFps = 0;

        void Mark ()
        {
            double cur = stopWatch.Elapsed.TotalMilliseconds;
            if (cur < 2000) {
                return;
            }
            frames++;

            if (cur - prev >= 995) {
                avgFps = 0.8 * avgFps + 0.2 * frames;

                Log.Verbose ("AndroidGameView", "frames {0} elapsed {1}ms {2:F2} fps",
                    frames,
                    cur - prev,
                    avgFps);

                frames = 0;
                prev = cur;
            }
        }

        protected void EnsureUndisposed ()
        {
            if (disposed)
                throw new ObjectDisposedException ("");
        }

        protected void DestroyGLContext ()
        {
            Log.Verbose ("AndroidGameView", "DestroyGLContext 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            if (eglContext != null) {
                if (!egl.EglDestroyContext (eglDisplay, eglContext))
                    throw new Exception ("Could not destroy EGL context" + GetErrorAsString ());
                eglContext = null;
            }
            if (eglDisplay != null) {
                if (!egl.EglTerminate (eglDisplay))
                    throw new Exception ("Could not terminate EGL connection" + GetErrorAsString ());
                eglDisplay = null;
            }

            glContextAvailable = false;
            Log.Verbose ("AndroidGameView", "DestroyGLContext end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        void DestroyGLSurfaceInternal ()
        {
            Log.Verbose ("AndroidGameView", "DestroyGLSurfaceInternal 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            if (!(eglSurface == null || eglSurface == EGL10.EglNoSurface)) {
                if (!egl.EglMakeCurrent (eglDisplay, EGL10.EglNoSurface,
                        EGL10.EglNoSurface, EGL10.EglNoContext)) {
                    Log.Verbose ("AndroidGameView", "Could not unbind EGL surface" + GetErrorAsString ());
                }

                if (!egl.EglDestroySurface (eglDisplay, eglSurface)) {
                    Log.Verbose ("AndroidGameView", "Could not destroy EGL surface" + GetErrorAsString ());
                }
            }
            eglSurface = null;
            glSurfaceAvailable = false;
            Log.Verbose ("AndroidGameView", "DestroyGLSurfaceInternal end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        protected virtual void DestroyGLSurface ()
        {
            Log.Verbose ("AndroidGameView", "DestroyGLSurface 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            DestroyGLSurfaceInternal ();
            Log.Verbose ("AndroidGameView", "DestroyGLSurface end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        internal struct SurfaceConfig
        {
            public int Red;
            public int Green;
            public int Blue;
            public int Alpha;
            public int Depth;
            public int Stencil;

            public int [] ToConfigAttribs ()
            {

                return new int [] {
                    EGL11.EglRedSize, Red,
                    EGL11.EglGreenSize, Green,
                    EGL11.EglBlueSize, Blue,
                    EGL11.EglAlphaSize, Alpha,
                    EGL11.EglDepthSize, Depth,
                    EGL11.EglStencilSize, Stencil,
                    EGL11.EglRenderableType, 4,
                    EGL11.EglNone
                };
            }

            public override string ToString ()
            {
                return string.Format ("Red:{0} Green:{1} Blue:{2} Alpha:{3} Depth:{4} Stencil:{5}", Red, Green, Blue, Alpha, Depth, Stencil);
            }
        }

        protected void CreateGLContext ()
        {
            Log.Verbose ("AndroidGameView", "CreateGLContext");
            lostglContext = false;

            egl = EGLContext.EGL.JavaCast<IEGL10> ();

            eglDisplay = egl.EglGetDisplay (EGL10.EglDefaultDisplay);
            if (eglDisplay == EGL10.EglNoDisplay)
                throw new Exception ("Could not get EGL display" + GetErrorAsString ());

            int [] version = new int [2];
            if (!egl.EglInitialize (eglDisplay, version))
                throw new Exception ("Could not initialize EGL display" + GetErrorAsString ());

            int depth = 0;
            int stencil = 0;
            switch (_game.graphicsDeviceManager.PreferredDepthStencilFormat) {
            case DepthFormat.Depth16:
                depth = 16;
                break;
            case DepthFormat.Depth24:
                depth = 24;
                break;
            case DepthFormat.Depth24Stencil8:
                depth = 24;
                stencil = 8;
                break;
            case DepthFormat.None:
                break;
            }

            List<SurfaceConfig> configs = new List<SurfaceConfig> ();
            if (depth > 0) {
                configs.Add (new SurfaceConfig () { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil });
                configs.Add (new SurfaceConfig () { Red = 5, Green = 6, Blue = 5, Depth = depth, Stencil = stencil });
                configs.Add (new SurfaceConfig () { Depth = depth, Stencil = stencil });
                if (depth > 16) {
                    configs.Add (new SurfaceConfig () { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = 16 });
                    configs.Add (new SurfaceConfig () { Red = 5, Green = 6, Blue = 5, Depth = 16 });
                    configs.Add (new SurfaceConfig () { Depth = 16 });
                }
            } else {
                configs.Add (new SurfaceConfig () { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add (new SurfaceConfig () { Red = 5, Green = 6, Blue = 5 });
            }
            configs.Add (new SurfaceConfig () { Red = 4, Green = 4, Blue = 4, Alpha = 0, Depth = 0, Stencil = 0 });
            int [] numConfigs = new int [1];
            EGLConfig [] results = new EGLConfig [1];


            foreach (var config in configs) {

                if (!egl.EglChooseConfig (eglDisplay, config.ToConfigAttribs (), results, 1, numConfigs)) {
                    continue;
                }
                Log.Verbose ("AndroidGameView", string.Format ("Selected Config : {0}", config));
                break;
            }

            if (numConfigs [0] == 0)
                throw new Exception ("No valid EGL configs found" + GetErrorAsString ());
            eglConfig = results [0];

            int [] contextAttribs = new int [] { EglContextClientVersion, 2, EGL10.EglNone };
            eglContext = egl.EglCreateContext (eglDisplay, eglConfig, EGL10.EglNoContext, contextAttribs);
            if (eglContext == null || eglContext == EGL10.EglNoContext) {
                eglContext = null;
                throw new Exception ("Could not create EGL context" + GetErrorAsString ());
            }

            glContextAvailable = true;
        }

        private string GetErrorAsString ()
        {
            switch (egl.EglGetError ()) {
            case EGL10.EglSuccess:
                return "Success";

            case EGL10.EglNotInitialized:
                return "Not Initialized";

            case EGL10.EglBadAccess:
                return "Bad Access";
            case EGL10.EglBadAlloc:
                return "Bad Allocation";
            case EGL10.EglBadAttribute:
                return "Bad Attribute";
            case EGL10.EglBadConfig:
                return "Bad Config";
            case EGL10.EglBadContext:
                return "Bad Context";
            case EGL10.EglBadCurrentSurface:
                return "Bad Current Surface";
            case EGL10.EglBadDisplay:
                return "Bad Display";
            case EGL10.EglBadMatch:
                return "Bad Match";
            case EGL10.EglBadNativePixmap:
                return "Bad Native Pixmap";
            case EGL10.EglBadNativeWindow:
                return "Bad Native Window";
            case EGL10.EglBadParameter:
                return "Bad Parameter";
            case EGL10.EglBadSurface:
                return "Bad Surface";

            default:
                return "Unknown Error";
            }
        }

        protected void CreateGLSurface ()
        {
            w = 701;
            Log.Verbose ("AndroidGameView", "CreateGLSurface 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock(_lock)
            {
                // todo: remove this lock
                if( !androidSurfaceAvailable)
                {
                    Log.Error ("AndroidGameView", "CreateGLSurface isSurfaceAvalible == false: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                }
            }

            if ( !glSurfaceAvailable)
            {
                w = 702;
                try
                {
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                    // If there is an existing surface, destroy the old one
                    DestroyGLSurfaceInternal ();
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                    w = 703;
                    eglSurface = egl.EglCreateWindowSurface (eglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);
                    if (eglSurface == null || eglSurface == EGL10.EglNoSurface)
                        throw new Exception ("Could not create EGL window surface" + GetErrorAsString ());
                    w = 704;
                    if (!egl.EglMakeCurrent (eglDisplay, eglSurface, eglSurface, eglContext))
                        throw new Exception ("Could not make EGL current" + GetErrorAsString ());
                    w = 705;
                    glSurfaceAvailable = true;
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 4: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                    // Must set viewport after creation, the viewport has correct values in it already as we call it, but
                    // the surface is created after the correct viewport is already applied so we must do it again.
                    if (_game.GraphicsDevice != null)
                        _game.graphicsDeviceManager.ResetClientBounds ();
                    w = 705;
                    /*   if (_game.GraphicsDevice != null)
                       {
                           _game.GraphicsDevice.ApplyCurrentViewport ();
                       }*/
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 5: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                }
                catch (Exception ex)
                {
                    Log.Error ("AndroidGameView", ex.ToString ());
                    glSurfaceAvailable = false;
                }
            }
            w = 706;
            Log.Verbose ("AndroidGameView", "CreateGLSurface end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        protected EGLSurface CreatePBufferSurface (EGLConfig config, int [] attribList)
        {
            IEGL10 egl = EGLContext.EGL.JavaCast<IEGL10> ();
            EGLSurface result = egl.EglCreatePbufferSurface (eglDisplay, config, attribList);
            if (result == null || result == EGL10.EglNoSurface)
                throw new Exception ("EglCreatePBufferSurface");
            return result;
        }

        protected void ContextSetInternal ()
        {
            if (lostglContext)
            {
                if (_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Initialize ();

                    IsResuming = true;
                    if (_gameWindow.Resumer != null)
                    {
                        _gameWindow.Resumer.LoadContent ();
                    }

                    // Reload textures on a different thread so the resumer can be drawn
                    System.Threading.Thread bgThread = new System.Threading.Thread (
                        o => {
                            Android.Util.Log.Debug ("MonoGame", "Begin reloading graphics content");
                            Microsoft.Xna.Framework.Content.ContentManager.ReloadGraphicsContent ();
                            Android.Util.Log.Debug ("MonoGame", "End reloading graphics content");

                            // DeviceReset events
                            _game.graphicsDeviceManager.OnDeviceReset (EventArgs.Empty);
                            _game.GraphicsDevice.OnDeviceReset ();

                            IsResuming = false;
                        });

                    bgThread.Start ();
                }
            }
            OnContextSet (EventArgs.Empty);
        }

        protected void ContextLostInternal ()
        {
            OnContextLost (EventArgs.Empty);
            _game.graphicsDeviceManager.OnDeviceResetting (EventArgs.Empty);
            if (_game.GraphicsDevice != null)
                _game.GraphicsDevice.OnDeviceResetting ();
        }

        protected virtual void OnContextLost (EventArgs eventArgs)
        {

        }

        volatile int w = -1;
       /* protected bool IsGLSurfaceAvailableWaitIfNot ()
        {
            // todo: why is this lock here needed??
            w = 1;
            lock (lockObject)
            {
                w = 2;
                // we want to wait until we have a valid surface
                // this is not called from the UI thread but on
                // the background rendering thread
                while (!cts.IsCancellationRequested)
                {
                    //Log.Verbose ("AndroidGameView", "IsGLSurfaceAvailable {0} IsPaused {1} lostcontext {2} surfaceAvailable {3} contextAvailable {4} ThreadID {5}",
                    //  glSurfaceAvailable, isPaused, lostglContext, surfaceAvailable, glContextAvailable,Thread.CurrentThread.ManagedThreadId);
                    if ( glSurfaceAvailable && (isPaused || !surfaceAvailable) )
                    {
                        w = 3;
                        forceGLSurfaceRecreation = false;

                        // Surface we are using needs to go away
                        DestroyGLSurface ();
                        w = 4;
                        if (loaded)
                            OnUnload (EventArgs.Empty);
                        w = 5;
                    }
                    else if ((!glSurfaceAvailable && !isPaused && surfaceAvailable ) || lostglContext )
                    {
                        forceGLSurfaceRecreation = false;
                        w = 6;
                        // We can (re)create the EGL surface (not paused, surface available)
                        if (glContextAvailable && !lostglContext)
                        {
                            try
                            {
                                w = 7;
                                CreateGLSurface ();
                            } catch (Exception ex)
                            {
                                // We failed to create the surface for some reason
                                Log.Verbose ("AndroidGameView", ex.ToString ());
                            }
                            w = 8;
                        }

                        if (!glSurfaceAvailable || lostglContext) { // Start or Restart due to context loss
                            bool contextLost = false;
                            if (lostglContext || glContextAvailable) {
                                // we actually lost the context
                                // so we need to free up our existing 
                                // objects and re-create one.
                                w = 9;
                                DestroyGLContext ();
                                contextLost = true;
                                w = 10;
                                Log.Verbose ("AndroidGameView", "ContentLostInternal");
                                ContextLostInternal ();
                            }
                            w = 11;
                            CreateGLContext ();
                            w = 12;
                            CreateGLSurface ();
                            w = 13;
                            if (!loaded && glContextAvailable)
                                OnLoad (EventArgs.Empty);
                            w = 14;
                            if (contextLost && glContextAvailable) {
                                Log.Verbose ("AndroidGameView", "ContentSetInternal");
                                // we lost the gl context, we need to let the programmer
                                // know so they can re-create textures etc.
                                ContextSetInternal ();
                                w = 15;
                            }
                        }
                    }
                    else if( glSurfaceAvailable && surfaceAvailable && forceGLSurfaceRecreation)
                    {
                        w = 16;
                        // when rotation happens we must recreate surface
                        forceGLSurfaceRecreation = false;

                        DestroyGLSurface ();
                        w = 17;
                        CreateGLSurface ();
                        w = 18;
                    }

                    w = 19;

                    // If we have a GL surface we can continue 
                    // rednering
                    if (glSurfaceAvailable)
                    {
                        w = 20;
                        return true;
                    }
                    else
                    {
                        w = 21;
                        // if we dont we need to wait until we get
                        // a surfaceCreated event or some other 
                        // event from the ISurfaceHolderCallback
                        // so we can create a new GL surface.
                        if (cts.IsCancellationRequested)
                        {
                            w = 22;
                            break;
                        }

                        w = 23;

                        if (Game.Activity.IsFinishing)
                        {
                            w = 24;
                            return false;
                        }
                        w = 25;
                        Log.Verbose ("AndroidGameView", "IsGLSurfaceAvailable entering wait state: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                        w = 26;
                        triggerOpenALPauseAndResumeEvents ();
                        Monitor.Wait (lockObject);
                        //Monitor.PulseAll (lockObject);
                        w = 27;
                        Log.Verbose ("AndroidGameView", "IsGLSurfaceAvailable exiting wait state: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                        return false;
                    }
                }
                Log.Verbose ("AndroidGameView", "IsGLSurfaceAvailable exited!!!!!: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                return false;
            }
        }*/

        protected virtual void OnContextSet (EventArgs eventArgs)
        {

        }

        protected virtual void OnUnload (EventArgs eventArgs)
        {

        }

        protected virtual void OnLoad (EventArgs eventArgs)
        {

        }

        protected virtual void OnStopped (EventArgs eventArgs)
        {

        }

        #region Key and Motion

        public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
        {
            if (GamePad.OnKeyDown (keyCode, e))
                return true;

            Keyboard.KeyDown (keyCode);
            #if !OUYA
            // we need to handle the Back key here because it doesnt work any other way
            if (keyCode == Keycode.Back)
                GamePad.Back = true;
            #endif
            if (keyCode == Keycode.VolumeUp) {
                AudioManager audioManager = (AudioManager)Context.GetSystemService (Context.AudioService);
                audioManager.AdjustStreamVolume (Stream.Music, Adjust.Raise, VolumeNotificationFlags.ShowUi);
                return true;
            }

            if (keyCode == Keycode.VolumeDown) {
                AudioManager audioManager = (AudioManager)Context.GetSystemService (Context.AudioService);
                audioManager.AdjustStreamVolume (Stream.Music, Adjust.Lower, VolumeNotificationFlags.ShowUi);
                return true;
            }

            return true;
        }

        public override bool OnKeyUp (Keycode keyCode, KeyEvent e)
        {
            if (GamePad.OnKeyUp (keyCode, e))
                return true;
            Keyboard.KeyUp (keyCode);
            return true;
        }

        public override bool OnGenericMotionEvent (MotionEvent e)
        {
            if (GamePad.OnGenericMotionEvent (e))
                return true;

            return base.OnGenericMotionEvent (e);
        }

        #endregion

        #region Properties

        private IEGL10 egl;
        private EGLDisplay eglDisplay;
        private EGLConfig eglConfig;
        private EGLContext eglContext;
        private EGLSurface eglSurface;

        /// <summary>The visibility of the window. Always returns true.</summary>
        /// <value></value>
        /// <exception cref="T:System.ObjectDisposed">The instance has been disposed</exception>
        public virtual bool Visible {
            get {
                EnsureUndisposed ();
                return true;
            }
            set {
                EnsureUndisposed ();
            }
        }

        /// <summary>The size of the current view.</summary>
        /// <value>A <see cref="T:System.Drawing.Size" /> which is the size of the current view.</value>
        /// <exception cref="T:System.ObjectDisposed">The instance has been disposed</exception>
        public virtual Size Size {
            get {
                EnsureUndisposed ();
                return size;
            }
            set {
                EnsureUndisposed ();
                if (size != value) {
                    size = value;
                    OnResize (EventArgs.Empty);
                }
            }
        }

        private void OnResize (EventArgs eventArgs)
        {

        }

        #endregion

        public event FrameEvent RenderFrame;
        public event FrameEvent UpdateFrame;

        public delegate void FrameEvent (object sender, FrameEventArgs e);

        public class FrameEventArgs : EventArgs
        {
            double elapsed;

            /// <summary>
            /// Constructs a new FrameEventArgs instance.
            /// </summary>
            public FrameEventArgs ()
            {
            }

            /// <summary>
            /// Constructs a new FrameEventArgs instance.
            /// </summary>
            /// <param name="elapsed">The amount of time that has elapsed since the previous event, in seconds.</param>
            public FrameEventArgs (double elapsed)
            {
                Time = elapsed;
            }

            /// <summary>
            /// Gets a <see cref="System.Double"/> that indicates how many seconds of time elapsed since the previous event.
            /// </summary>
            public double Time {
                get { return elapsed; }
                internal set {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException ();
                    elapsed = value;
                }
            }
        }

        public BackgroundContext CreateBackgroundContext ()
        {
            return new BackgroundContext (this);
        }

        public class BackgroundContext
        {

            EGLContext eglContext;
            MonoGameAndroidGameView view;
            EGLSurface surface;

            public BackgroundContext (MonoGameAndroidGameView view)
            {
                this.view = view;
                int [] contextAttribs = new int [] { EglContextClientVersion, 2, EGL10.EglNone };
                eglContext = view.egl.EglCreateContext (view.eglDisplay, view.eglConfig, view.eglContext, contextAttribs);
                if (eglContext == null || eglContext == EGL10.EglNoContext) {
                    eglContext = null;
                    throw new Exception ("Could not create EGL context" + view.GetErrorAsString ());
                }
                int [] pbufferAttribList = new int [] { EGL10.EglWidth, 64, EGL10.EglHeight, 64, EGL10.EglNone };
                surface = view.CreatePBufferSurface (view.eglConfig, pbufferAttribList);
                if (surface == EGL10.EglNoSurface)
                    throw new Exception ("Could not create Pbuffer Surface" + view.GetErrorAsString ());
            }

            public void MakeCurrent ()
            {
                view.ClearCurrent ();
                view.egl.EglMakeCurrent (view.eglDisplay, surface, surface, eglContext);
            }
        }
    }
}
