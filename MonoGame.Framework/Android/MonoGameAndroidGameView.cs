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

        bool disposed = false;
        ISurfaceHolder mHolder;
        Size size;
        object lockObject = new object ();

        bool surfaceAvailable;

        int surfaceWidth;
        int surfaceHeight;

        bool forceGLSurfaceRecreation = false;
        bool glSurfaceAvailable;
        bool glContextAvailable;
        bool lostglContext;
        private bool isPaused;
        private bool isExited = false;
        System.Diagnostics.Stopwatch stopWatch;
        double tick = 0;

        volatile bool executeOpenALOnPause = false;
        volatile bool executeOpenALOnResume = false;

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

            lock (lockObject) {
                Log.Verbose ("AndroidGameView", "SurfaceChanged 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                surfaceWidth = Width;
                surfaceHeight = Height;

                // Set flag to recreate gl surface or rendering can be bad on orienation change or if app 
                // is closed in one orientation and re-opened in another.
                forceGLSurfaceRecreation = true;
               
                Log.Verbose ("AndroidGameView", "SurfaceChanged end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            }
        }

        public void SurfaceCreated (ISurfaceHolder holder)
        {
            Log.Verbose ("AndroidGameView", "SurfaceCreated 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (lockObject)
            {
                Log.Verbose ("AndroidGameView", "SurfaceCreated 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                surfaceWidth = Width;
                surfaceHeight = Height;
                surfaceAvailable = true;
                Monitor.PulseAll (lockObject);
            }
            Log.Verbose ("AndroidGameView", "SurfaceCreated end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        public void SurfaceDestroyed (ISurfaceHolder holder)
        {
            Log.Verbose ("AndroidGameView", "SurfaceDestroyed 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (lockObject)
            {
                Log.Verbose ("AndroidGameView", "SurfaceDestroyed 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                surfaceAvailable = false;
                Monitor.PulseAll (lockObject);
                Log.Verbose ("AndroidGameView", "SurfaceDestroyed 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                while (glSurfaceAvailable)
                {
                    Monitor.Wait (lockObject);
                }
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
            EnsureUndisposed ();

            Log.Verbose ("AndroidGameView", "Pause 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (lockObject) {
                isPaused = true;
                Monitor.PulseAll (lockObject);
            }

            Log.Verbose ("AndroidGameView", "Pause 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            // MUST BE CALLED AFTER 'isPaused = true;'
            pauseOpenALDeviceBlocking ();

            Log.Verbose ("AndroidGameView", "Pause end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        public virtual void Resume ()
        {
            EnsureUndisposed ();
            Log.Verbose ("AndroidGameView", "Resume 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            resumeOpenALDeviceNonBlocking ();
            Log.Verbose ("AndroidGameView", "Resume 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            lock (lockObject) {
                if (isPaused) {
                    isPaused = false;
                    Monitor.PulseAll (lockObject);
                }
                try {
                    if (!IsFocused)
                        RequestFocus ();
                } catch {
                }
            }
            Log.Verbose ("AndroidGameView", "Resume end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);


        }

        private void resumeOpenALDeviceNonBlocking()
        {
            // we cannot wait here because the renderTask has not started yet ( status == WaitingForActivation, which is an internal wait state)
            executeOpenALOnResume = true;
        }

        private void pauseOpenALDeviceBlocking()
        {
            Log.Verbose ("AndroidGameView", "pauseOpenALDeviceBlocking 1: w: " + w + ", s: " + s+", tid: "+Environment.CurrentManagedThreadId);
            // set flag and wait for game thread to finish executing it it, must wait so that game thread isn't stopped before that.
            lock (lockObject)
            {
                Log.Verbose ("AndroidGameView", "pauseOpenALDeviceBlocking 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                executeOpenALOnPause = true;
                Monitor.PulseAll (lockObject);
                while (executeOpenALOnPause)
                {
                    Log.Verbose ("AndroidGameView", "pauseOpenALDeviceBlocking 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                    Monitor.Wait (lockObject);
                    Log.Verbose ("AndroidGameView", "pauseOpenALDeviceBlocking 4: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                }
            }
           
            Log.Verbose ("AndroidGameView", "pauseOpenALDeviceBlocking end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
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

                lock (lockObject) {
                    Monitor.PulseAll (lockObject);
                }
                Log.Verbose ("AndroidGameView", "Stop 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                cts.Cancel ();
                while (!isExited) {
                    lock (lockObject) {
                        Monitor.PulseAll (lockObject);

                    }
                    await Task.Delay (100);
                }
                Log.Verbose ("AndroidGameView", "Stop 4: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                //renderTask.Wait ();
                Log.Verbose ("AndroidGameView", "Stop 5: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            }
            Log.Verbose ("AndroidGameView", "Stop end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        FrameEventArgs renderEventArgs = new FrameEventArgs ();

        int s = -1;
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
                    s = 2;
                    // MUST BE CALLED BEFORE IsGLSurfaceAvailable! because the latter blocks.
                    triggerOpenALPauseAndResumeEvents ();
                    s = 3;
                    // this waits when android onPause callback is triggered, resumes when onResume is called (by pinging Monitor)
                    if (!IsGLSurfaceAvailableWaitIfNot ())
                    {
                        s = 302;

                        continue;
                    }
                    s = 303;
                    if (isPaused)
                    {
                        s = 304;

                        continue;
                    }

                    s = 4;
                    Threading.ResetThread (Thread.CurrentThread.ManagedThreadId);
                    s = 5;
                    try
                    {
                        RunIteration (token);
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
                            if (token.IsCancellationRequested)
                                return;
                        }
                    }
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
                s = 8;
                lock (lockObject)
                {
                    s = 9;
                    isExited = true;
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
                }
            }
        }

        DateTime prevUpdateTime;
        DateTime prevRenderTime;
        DateTime curUpdateTime;
        DateTime curRenderTime;
        FrameEventArgs updateEventArgs = new FrameEventArgs ();

        void UpdateFrameInternal (FrameEventArgs e)
        {
            OnUpdateFrame (e);
            if (UpdateFrame != null)
                UpdateFrame (this, e);
        }

        protected virtual void OnUpdateFrame (FrameEventArgs e)
        {

        }

        void triggerOpenALPauseAndResumeEvents()
        {
            Log.Verbose ("AndroidGameView", "triggerOpenALPauseAndResumeEvents 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            // we must execute this event on the game thread so that the openAL device pause/resume get called on the same thread as other openAL API calls.
            if (executeOpenALOnPause)
            {
                Log.Verbose ("AndroidGameView", "triggerOpenALPauseAndResumeEvents Pause 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                executeOpenALOnPause = false;
                OnPauseGameThread (this, EventArgs.Empty);
                Log.Verbose ("AndroidGameView", "triggerOpenALPauseAndResumeEvents Pause end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                // must notify any waiting threads
                lock (lockObject)
                {
                    Monitor.PulseAll (lockObject);
                }
            }

            if (executeOpenALOnResume)
            {
                Log.Verbose ("AndroidGameView", "triggerOpenALPauseAndResumeEvents Resume: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                executeOpenALOnResume = false;
                OnResumeGameThread (this, EventArgs.Empty);
                Log.Verbose ("AndroidGameView", "triggerOpenALPauseAndResumeEvents Resume end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                // must notify any waiting threads
                lock (lockObject)
                {
                    Monitor.PulseAll (lockObject);
                }
            }

          

            Log.Verbose ("AndroidGameView", "triggerOpenALPauseAndResumeEvents end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        // this method is called on the main thread
        void RunIteration (CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            curUpdateTime = DateTime.Now;
            if (prevUpdateTime.Ticks != 0) {
                var t = (curUpdateTime - prevUpdateTime).TotalMilliseconds;
                updateEventArgs.Time = t < 0 ? 0 : t;
            }
            try {
                UpdateFrameInternal (updateEventArgs);
            } catch (Content.ContentLoadException) {
                // ignore it..
            }

            prevUpdateTime = curUpdateTime;

            curRenderTime = DateTime.Now;
            if (prevRenderTime.Ticks == 0) {
                var t = (curRenderTime - prevRenderTime).TotalMilliseconds;
                renderEventArgs.Time = t < 0 ? 0 : t;
            }

            RenderFrameInternal (renderEventArgs);
            prevRenderTime = curRenderTime;

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
            Log.Verbose ("AndroidGameView", "DestroyGLSurfaceInternal end: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

        }

        protected virtual void DestroyGLSurface ()
        {
            Log.Verbose ("AndroidGameView", "DestroyGLSurface 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
            DestroyGLSurfaceInternal ();
            glSurfaceAvailable = false;
            Monitor.PulseAll (lockObject);
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
            Log.Verbose ("AndroidGameView", "CreateGLSurface 1: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

            if (!glSurfaceAvailable)
                try {
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 2: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);
                    // If there is an existing surface, destroy the old one
                    DestroyGLSurfaceInternal ();
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 3: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                    eglSurface = egl.EglCreateWindowSurface (eglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);
                    if (eglSurface == null || eglSurface == EGL10.EglNoSurface)
                        throw new Exception ("Could not create EGL window surface" + GetErrorAsString ());

                    if (!egl.EglMakeCurrent (eglDisplay, eglSurface, eglSurface, eglContext))
                        throw new Exception ("Could not make EGL current" + GetErrorAsString ());

                    glSurfaceAvailable = true;
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 4: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                    // Must set viewport after creation, the viewport has correct values in it already as we call it, but
                    // the surface is created after the correct viewport is already applied so we must do it again.
                    if (_game.GraphicsDevice != null)
                        _game.graphicsDeviceManager.ResetClientBounds ();

                 /*   if (_game.GraphicsDevice != null)
                    {
                        _game.GraphicsDevice.ApplyCurrentViewport ();
                    }*/
                    Log.Verbose ("AndroidGameView", "CreateGLSurface 5: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

                }
                catch (Exception ex) {
                    Log.Error ("AndroidGameView", ex.ToString ());
                    glSurfaceAvailable = false;
                }
            Log.Verbose ("AndroidGameView", "CreateGLSurface 6: w: " + w + ", s: " + s + ", tid: " + Environment.CurrentManagedThreadId);

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
            if (lostglContext) {
                if (_game.GraphicsDevice != null) {
                    _game.GraphicsDevice.Initialize ();

                    IsResuming = true;
                    if (_gameWindow.Resumer != null) {
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

        int w = -1;
        protected bool IsGLSurfaceAvailableWaitIfNot ()
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
        }

        protected virtual void OnUnload (EventArgs eventArgs)
        {

        }

        protected virtual void OnContextSet (EventArgs eventArgs)
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
