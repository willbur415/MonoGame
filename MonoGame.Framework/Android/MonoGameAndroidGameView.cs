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

        // we use signals to increase responsiveness of the app instead of using blocking while loops
       /* AutoResetEvent _waitForPauseProcessingToFinish = new AutoResetEvent (false); // more reliable than monitor, the set (==pulse) doesn't get lost if wait is not called yet
        AutoResetEvent _waitForResumeToFinishProcessing = new AutoResetEvent (false); // we cannot nicely wait inside resume as we can in pause because surface creation doesn't get triggered so we deadlock
        AutoResetEvent _waitForExitProcessingToFinish = new AutoResetEvent (false); 
        //AutoResetEvent _waitForAndroidSurfaceCreation = new AutoResetEvent (false);
        AutoResetEvent _waitForMainGameLoop = new AutoResetEvent (false);*/
       // int _interlockedSlowGameThread = 0; // if 1 game thread starts waiting 50 ms per frame, needed to increase responsiveness on some older phones

        bool _wasPausedStateProcessed = false; // more reliable than monitor, the set (==pulse) doesn't get lost if wait is not called yet
        bool _wasResumeStateProcessed = false; // we cannot nicely wait inside resume as we can in pause because surface creation doesn't get triggered so we deadlock
        bool _wasExitStateProcessed = false;

        AutoResetEvent _waitForMainGameLoop2 = new AutoResetEvent (false);
        AutoResetEvent _workerThreadUIRenderingWait = new AutoResetEvent (false);

        object _lockObject = new object ();

        volatile InternalState _internalState = InternalState.Exited_GameThread;

        bool androidSurfaceAvailable = false;

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
            // default
            mHolder = Holder;
            // Add callback to get the SurfaceCreated etc events
            mHolder.AddCallback (this);
            mHolder.SetType (SurfaceType.Gpu);
            OpenGL.GL.LoadEntryPoints();
        }

        public void SurfaceChanged (ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceChanged 1, " + _internalState);

            // make app more reponsive by making locks easier to aqcuire by the UI thread
           // Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

            // Set flag to recreate gl surface or rendering can be bad on orienation change or if app 
            // is closed in one orientation and re-opened in another.
            lock (_lockObject)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceChanged 2, " + _internalState);

                // can only be triggered when main loop is running, is unsafe to overwrite other states
                if (_internalState == InternalState.Running_GameThread)
                {
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceChanged 3, " + _internalState);

                    _internalState = InternalState.ForceRecreateSurface;
                }
                    
            }

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceChanged end, " + _internalState);

        }

        public void SurfaceCreated (ISurfaceHolder holder)
        {
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceCreated 1, " + _internalState);

            // make app more reponsive by making locks easier to aqcuire by the UI thread
           // Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

            lock (_lockObject)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceCreated 2, " + _internalState);

                androidSurfaceAvailable = true;
            }
            //_waitForAndroidSurfaceCreation.Set ();

            // if main thread is waiting in resuming state send signal to wake it because it goes to sleep if no surface, otherwise not responsive enough
            /*  if (_internalState == InternalState.Resuming_UIThread)
              {

                  Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceCreated pulse, " + _internalState);

                  _pauseSignal.Set ();
              }*/


            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceCreated end, " + _internalState);

        }

        public void SurfaceDestroyed (ISurfaceHolder holder)
        {
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceDestroyed 1, " + _internalState);

            // make app more reponsive by making locks easier to aqcuire by the UI thread
            //Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

            lock (_lockObject)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceDestroyed 2, " + _internalState);

                androidSurfaceAvailable = false;
            }
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: SurfaceDestroyed end, " + _internalState);

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
            Run (0.0);
        }

        public virtual void Run (double updatesPerSecond)
        {
            cts = new CancellationTokenSource ();
            if (LogFPS)
            {
                targetFps = currentFps = 0;
                avgFps = 1;
            }
            updates = 1000 / updatesPerSecond;

            //var syncContext = new SynchronizationContext ();
            var syncContext = SynchronizationContext.Current;

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Run 1, " + Environment.CurrentManagedThreadId + ", " + System.Threading.Thread.CurrentThread.Name + ", " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            assertThreadPriority ();
       
            // We always start a new task, regardless if we render on UI thread or not.
            renderTask = Task.Factory.StartNew (() =>
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Run 2, " + Environment.CurrentManagedThreadId + ", " + System.Threading.Thread.CurrentThread.Name + ", " + System.Threading.Thread.CurrentThread.ManagedThreadId);
               
                WorkerThreadFrameDispatcher (syncContext);

            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith ((t) =>
                {
                    OnStopped (EventArgs.Empty);
                });
        }

        volatile int numResume = 0;
        public virtual void Pause ()
        {
            --numResume;
            // lock the entire block if you are doing anything otherwise deadlocks will creep on you because game thread will call "pulse" before you reach "wait" here.

            EnsureUndisposed ();
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 1, " + _internalState+", p: "+ numResume);

            // if triggered in quick succession and blocked by graphics device creation, 
            // pause can be triggered twice, without resume in between on some phones.
            if (_internalState != InternalState.Running_GameThread)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 11, " + _internalState + ", p: " + numResume);
                return;
            }


            // this guarantees that resume finished processing, since we cannot wait inside resume because we deadlock as surface wouldn't get created
            if (RenderOnUIThread == false)
            {
                while (true)
                {
                    lock (_lockObject)
                    {
                        if (_wasResumeStateProcessed)
                            break;
                    }
                }
            }

            _waitForMainGameLoop2.Reset ();  // in case it was enabled

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 2, " + _internalState + ", p: " + numResume);

            // make app more reponsive by making locks easier to aqcuire by the UI thread
         //   Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

            // happens if pause is called immediately after resume so that the surfaceCreated callback was not called yet.

            bool isAndroidSurfaceAvalible = false; // use local because the wait below must be outside lock
            lock (_lockObject)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 3, " + _internalState + ", p: " + numResume);
                isAndroidSurfaceAvalible = androidSurfaceAvailable;

                if (!isAndroidSurfaceAvalible)
                {
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 4, " + _internalState + ", p: " + numResume);
                    _internalState = InternalState.Paused_GameThread; // prepare for next game loop iteration
                }
            }

            // must be outside lock
           /* if( !isAndroidSurfaceAvalible )
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 5, " + _internalState + ", xx: " + xx);

                _waitForPauseProcessingToFinish.WaitOne (); // now wait for the game thread to finish processing paused state
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause return, " + _internalState + ", xx: " + xx);
                return;
            }*/

             Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 6, " + _internalState + ", p: " + numResume);

            // make app more reponsive by making locks easier to aqcuire by the UI thread
           // Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

            lock (_lockObject)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 7, " + _internalState + ", p: " + numResume);

                // processing the pausing state only if the surface was created already
                if (androidSurfaceAvailable)
                {
                    _wasPausedStateProcessed = false;
                    _internalState = InternalState.Pausing_UIThread;
                }              
            }

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause 8, " + _internalState + ", p: " + numResume);

            if(RenderOnUIThread == false)
            {
                while (true)
                {
                    lock (_lockObject)
                    {
                        if (_wasPausedStateProcessed)
                            break;
                    }
                }
            }

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Pause end, " + _internalState + ", p: " + numResume);

        }

        public virtual void Resume ()
        {
            numResume++;
            // lock the entire block if you are doing anything otherwise deadlocks will creep on you because game thread will call "pulse" before you reach "wait" here.
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume 1, " + _internalState + ", p: " + numResume);

            EnsureUndisposed ();
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume 2, " + _internalState + ", p: " + numResume);

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume 21, " + _internalState + ", p: " + numResume);


            // make app more reponsive by making locks easier to aqcuire by the UI thread
           // Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

            lock (_lockObject)
            {
               Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume 4, " + _internalState + ", p: " + numResume);

                _wasResumeStateProcessed = false;
                _internalState = InternalState.Resuming_UIThread;
            }

            // Monitor.PulseAll (_lockObject); // restart main loop if it was paused previously

                 _waitForMainGameLoop2.Set ();

                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume 5, " + _internalState + ", p: " + numResume);

                try
                {
                    if (!IsFocused)
                        RequestFocus ();
                }
                catch {  }
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume 6, " + _internalState + ", p: " + numResume);

          
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Resume end, " + _internalState + ", p: " + numResume);


            // do not wait for state transition here since surface creation must be triggered first
        }

        protected override void Dispose (bool disposing)
        {
            if (disposing) {
                Stop ();
            }
            base.Dispose (disposing);
        }

   
        public void Stop ()
        {
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Stop 1, " + _internalState);

            EnsureUndisposed ();
            if (cts != null)
            {
                // make app more reponsive by making locks easier to aqcuire by the UI thread
               // Interlocked.Exchange (ref _interlockedSlowGameThread, 1);

                lock (_lockObject)
                {
                    _internalState = InternalState.Exiting;
                }
               
                cts.Cancel ();

                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: WAITALL stop, " + _internalState);

                if( RenderOnUIThread == false)
                {
                    while (true)
                    {
                        lock (_lockObject)
                        {
                            if (_wasExitStateProcessed)
                                break;
                        }
                    }
                }

            }

            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Stop end, " + _internalState);

        }

        FrameEventArgs renderEventArgs = new FrameEventArgs ();
  
        protected void WorkerThreadFrameDispatcher (SynchronizationContext uiThreadSyncContext)
        {
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: RenderLoop 1, " + _internalState);

            Threading.ResetThread (Thread.CurrentThread.ManagedThreadId);
            try
            {
                stopWatch = System.Diagnostics.Stopwatch.StartNew ();
                tick = 0;
                prevUpdateTime = DateTime.Now;
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: RenderLoop 2, " + _internalState);
               
                while (!cts.IsCancellationRequested)
                {
                    // todo comment 
                   /* if (!RenderOnUIThread)
                    {
                        uiThreadSyncContext.Send ((s) =>
                        {

                        }, null);
                    }*/

                    assertThreadPriority (); // in case users try change thread priority

                    // either use UI thread to render one frame or this worker thread
                    bool pauseThread = false;
                    if(RenderOnUIThread)
                    {
                        uiThreadSyncContext.Send ((s) =>
                        {
                            pauseThread = RunIteration (cts.Token);
                        }, null);
                    }
                    else
                    {
                        pauseThread = RunIteration (cts.Token);

                        // allows CPU to switch to some other thread of same priority if any is waiting. If none is waiting it does nothing.
                        // (UI thread is what we are interested in giving CPU cycles to), needed for one phone (droid razr M),
                        // otherwise pause/resume doesn't get called. As if it has bad scheduling??
                       // Thread.Sleep (1); 
                    }


                    if(pauseThread)
                    {
                        Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: RenderLoop pausing, " + _internalState);

                        lock (_lockObject)
                        {
                            _wasPausedStateProcessed = true;
                        }
                       
                        _waitForMainGameLoop2.WaitOne (); // pause this thread
                    }
                }


                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: RenderLoop 3, " + _internalState);

            }
            catch (Exception ex)
            {
                Log.Error ("AndroidGameView", ex.ToString ());
            }
            finally
            {
                bool c = cts.IsCancellationRequested;
             
                cts = null;

                if (glSurfaceAvailable)
                    DestroyGLSurface ();

                if (glContextAvailable)
                {
                    DestroyGLContext ();
                    ContextLostInternal ();
                }

                lock (_lockObject)
                {
                    _internalState = InternalState.Exited_GameThread;
                }
            }
            Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: RenderLoop end, " + _internalState);

        }

        void assertThreadPriority()
        {
            if (Thread.CurrentThread.Priority != ThreadPriority.Normal)
            {
                throw new Exception ("Error: MonoGameAndroidGameView.cs: UI thread and worker thread priority must be Normal. If you set it some other value some phones may not trigger pause or resume anymore. Remove at your own peril.");
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

            lock (_lockObject)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }

        void processStateRunning(CancellationToken  token)
        {
            if (runs>0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateRunning 1, " + _internalState);
            }

            // do not run game if surface is not avalible
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                {

                    return;
                }
            }


            // check if app wants to exit
            if (token.IsCancellationRequested)
            {
                // change state to exit and skip game loop
                lock (_lockObject)
                {
                    _internalState = InternalState.Exiting;
                }
 
                return;
            }
            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateRunning 2, " + _internalState);
            }
            try
            {
                UpdateAndRenderFrame ();
            }
            catch (MonoGameGLException ex)
            {
                Log.Error ("AndroidGameView", "GL Exception occured during RunIteration {0}", ex.Message);
            }
           
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
            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateRunning end, " + _internalState);
            }
            --runs;
        }

        void processStatePausing ()
        {        
            if (glSurfaceAvailable)
            {            
                // Surface we are using needs to go away
                DestroyGLSurface ();

                if (loaded)
                    OnUnload (EventArgs.Empty);
            }
         
            // trigger callbacks, must pause openAL device here
            OnPauseGameThread (this, EventArgs.Empty);

            // go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Paused_GameThread;
            }
        }

        int runs = 0;
        void processStateResuming ()
        {
           // Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming 1, " + _internalState);

            // do not execute yet, must wait for android callbacks that surface was created,
            // need signaling to increase responsiveness
           
            bool isSurfaceAvalible = false;
            lock (_lockObject)
            {
               // Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming 2, " + _internalState);

                isSurfaceAvalible = androidSurfaceAvailable;
            }
           // Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming 3, " + _internalState);

            // must sleep outside lock!
            if (!isSurfaceAvalible)
            {
                //  _waitForAndroidSurfaceCreation.WaitOne ();
              //  Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming no surf, " + _internalState);

                Thread.Sleep (50); // sleep so UI thread easier acquires lock
                return;
            }

            //    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming 4, " + _internalState);


            // this can happen if pause is triggered immediately after resume so that SurfaceCreated callback doesn't get called yet,
            // in this case we skip the resume process and pause sets a new state.   
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                    return;

             //   Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming 5, " + _internalState);

                // create surface if context is avalible
                if (glContextAvailable && !lostglContext)
                {
                    try
                    {
                        CreateGLSurface ();
                    }
                    catch (Exception ex)
                    {
                        // We failed to create the surface for some reason
                        Log.Verbose ("AndroidGameView", ex.ToString ());
                    }
                }
             //   Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming 6, " + _internalState);

                // create context if not avalible
                if ((!glContextAvailable || lostglContext))
                {
                    // Start or Restart due to context loss
                    bool contextLost = false;
                    if (lostglContext || glContextAvailable)
                    {
                        // we actually lost the context
                        // so we need to free up our existing 
                        // objects and re-create one.
                        DestroyGLContext ();
                        contextLost = true;

                        ContextLostInternal ();
                    }

                    CreateGLContext ();
                    CreateGLSurface ();

                    if (!loaded && glContextAvailable)
                        OnLoad (EventArgs.Empty);

                    if (contextLost && glContextAvailable)
                    {
                        // we lost the gl context, we need to let the programmer
                        // know so they can re-create textures etc.
                        ContextSetInternal ();
                    }

                }
                else if (glSurfaceAvailable) // finish state if surface created, may take a frame or two until the android UI thread callbacks fire
                {
                    // trigger callbacks, must resume openAL device here
                    OnResumeGameThread (this, EventArgs.Empty);

                    // go to next state
                    _internalState = InternalState.Running_GameThread;
                }
            }

            //Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: processStateResuming end, " + _internalState);

        }

        void processStateExiting ()
        {     
            // go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }

        void processStateForceSurfaceRecreation ()
        {       
            // needed at app start
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable || !glContextAvailable)
                {
                    return;
                }
            }
         
            DestroyGLSurface ();         
            CreateGLSurface ();
   
            // go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Running_GameThread;
            }
        }

        // Return true to trigger worker thread pause
        bool RunIteration(CancellationToken token)
        {

            //Thread.Sleep (100); this seems to fix it

            // set main game thread global ID
            Threading.ResetThread (Thread.CurrentThread.ManagedThreadId);

            InternalState currentState = InternalState.Exited_GameThread;
         
            // make app more reponsive by making locks easier to aqcuire by the UI thread
            /* int shouldSlowDownGameThread = Interlocked.CompareExchange (ref _interlockedSlowGameThread, 1, 1);
             if (shouldSlowDownGameThread == 1)
             {
                 Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: shouldSlowDownGameThread, " + _internalState);
                 xx = 3;
                 Thread.Sleep (100);
             }*/
    
            // THIS CAN ONLY BE SET BY THE EXISTING STATE OR IN CASE OF ERROR, NO OTHER STATE MUST SET THIS OR STATES CAN BE MISSED!
            bool exitGameLoop = false;

            lock (_lockObject)
            {
                currentState = _internalState; // todo: remove now that no lock?
            }
       
            switch (currentState)
            {
                // exit states
                case InternalState.Exiting: // when ui thread wants to exit
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStateExiting 1 out, " + _internalState);

                    processStateExiting ();
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStateExiting end out, " + _internalState);

                    break;

                case InternalState.Exited_GameThread: // when game thread processed exiting event
                    exitGameLoop = true;
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: Exited_GameThread  pulse, " + _internalState);

                    lock (_lockObject)
                    {
                        _wasExitStateProcessed = true;
                    }

                    //  Monitor.PulseAll (_lockObject); // continue UI thread Stop function execution
                    break;

                // pause states
                case InternalState.Pausing_UIThread: // when ui thread wants to pause
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStatePausing 1 out, " + _internalState);

                    processStatePausing ();
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStatePausing end out, " + _internalState);

                    break;

                case InternalState.Paused_GameThread: // when game thread processed pausing event
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: WAITALL Paused_GameThread, " + _internalState);

                    // this must be processed outside of this loop, in the new task thread!
                    return true; // trigger pause of worker thread

                    break; // for sanity in case of future fixes

                // other states
                case InternalState.Resuming_UIThread: // when ui thread wants to resume
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStateResuming 1 out, " + _internalState);

                    processStateResuming ();

                    // pause must wait for resume in case pause/resume is called in very quick succession
                    lock (_lockObject)
                    {
                        _wasResumeStateProcessed = true;
                    }

                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStateResuming end out, " + _internalState);

                    runs = 1;

                    break;

                case InternalState.Running_GameThread: // when we are running game 
                  
                    // disable slowed-down game loop
                    //  Interlocked.Exchange (ref _interlockedSlowGameThread, 0);

                    processStateRunning (token);
                   
                    break;

                case InternalState.ForceRecreateSurface:
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStateForceSurfaceRecreation 1 out, " + _internalState);

                    processStateForceSurfaceRecreation ();
                    Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: processStateForceSurfaceRecreation end out, " + _internalState);

                    break;

                // default case, error
                default:
                    processStateDefault ();
                    exitGameLoop = true;
                    break;
            }
         
            // if game wants to exit OR if error happens so that we rather exit than hang app
            if (exitGameLoop)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderLoop: exitGameLoop, " + _internalState);
            }

            return false;
        }

        void UpdateFrameInternal (FrameEventArgs e)
        {
            OnUpdateFrame (e);
            if (UpdateFrame != null)
            {
                UpdateFrame (this, e);
            }
              
        }
       
        protected virtual void OnUpdateFrame (FrameEventArgs e)
        {

        }

        // this method is called on the main thread
        void UpdateAndRenderFrame ()
        {
            //  if (token.IsCancellationRequested) // todo, no problem right if this is not used I presume, since 
            //  return;
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

            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: UpdateAndRenderFrame 1, " + _internalState);
            }

            RenderFrameInternal (renderEventArgs);


            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: UpdateAndRenderFrame 2, " + _internalState);
            }

            prevRenderTime = curRenderTime;
        }

        void RenderFrameInternal (FrameEventArgs e)
        {
            if (LogFPS) {
                Mark ();
            }
            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderFrameInternal 1, " + _internalState);
            }
            OnRenderFrame (e);
            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderFrameInternal 2, " + _internalState);
            }
            if (RenderFrame != null)
                RenderFrame (this, e);
            if (runs > 0)
            {
                Android.Util.Log.Verbose ("AndroidGameView", "MnGAndGameView: RenderFrameInternal end, " + _internalState);
            }
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
        }

        protected void DestroyGLSurface ()
        {
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
            // todo: remove this lock
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                {
                    Log.Error ("AndroidGameView", "CreateGLSurface isSurfaceAvalible == false");
                }
            }        

            if ( !glSurfaceAvailable)
            {
                try
                {
                    // If there is an existing surface, destroy the old one
                    DestroyGLSurface ();

                    eglSurface = egl.EglCreateWindowSurface (eglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);
                    if (eglSurface == null || eglSurface == EGL10.EglNoSurface)
                        throw new Exception ("Could not create EGL window surface" + GetErrorAsString ());
                   
                    if (!egl.EglMakeCurrent (eglDisplay, eglSurface, eglSurface, eglContext))
                        throw new Exception ("Could not make EGL current" + GetErrorAsString ());
                 
                    glSurfaceAvailable = true;
                   
                    // Must set viewport after creation, the viewport has correct values in it already as we call it, but
                    // the surface is created after the correct viewport is already applied so we must do it again.
                    if (_game.GraphicsDevice != null)
                        _game.graphicsDeviceManager.ResetClientBounds ();
                }
                catch (Exception ex)
                {
                    Log.Error ("AndroidGameView", ex.ToString ());
                    glSurfaceAvailable = false;
                }
            }         
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
