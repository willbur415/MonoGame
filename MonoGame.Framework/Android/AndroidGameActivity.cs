// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

namespace Microsoft.Xna.Framework
{
	[CLSCompliant(false)]
    public class AndroidGameActivity : Activity
    {
        internal Game Game { private get; set; }

        private ScreenReceiver screenReceiver;
        private OrientationListener _orientationListener;

        public bool AutoPauseAndResumeMediaPlayer = true;
        public bool RenderOnUIThread = true; 

		/// <summary>
		/// OnCreate called when the activity is launched from cold or after the app
		/// has been killed due to a higher priority app needing the memory
		/// </summary>
		/// <param name='savedInstanceState'>
		/// Saved instance state.
		/// </param>
		protected override void OnCreate (Bundle savedInstanceState)
		{
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

			IntentFilter filter = new IntentFilter();
		    filter.AddAction(Intent.ActionScreenOff);
		    filter.AddAction(Intent.ActionScreenOn);
		    filter.AddAction(Intent.ActionUserPresent);
		    
		    screenReceiver = new ScreenReceiver();
		    RegisterReceiver(screenReceiver, filter);

            _orientationListener = new OrientationListener(this);

			Game.Activity = this;
		}

        public static event EventHandler Paused;

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			// we need to refresh the viewport here.
			base.OnConfigurationChanged (newConfig);
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnConfigurationChanged end");

        }

        protected override void OnPause()
        {
            base.OnPause();

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnPause 1");

            if (Paused != null)
                Paused(this, EventArgs.Empty);

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnPause 2");

            if (_orientationListener.CanDetectOrientation())
                _orientationListener.Disable();

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnPause end");

        }

        public static event EventHandler Resumed;
        protected override void OnResume()
        {
           
            base.OnResume();
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume 1");

            if (Resumed != null)
                Resumed(this, EventArgs.Empty);

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume 2");

            if (Game != null)
            {
                Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume 3");

                var deviceManager = (IGraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
                if (deviceManager == null)
                    return;

                Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume 4");

                ((GraphicsDeviceManager)deviceManager).ForceSetFullScreen();
                ((AndroidGameWindow)Game.Window).GameView.RequestFocus();
                Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume 5");

                if (_orientationListener.CanDetectOrientation())
                    _orientationListener.Enable();
            }
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume end");

        }

        protected override void OnDestroy ()
		{
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnDestroy 1");

            UnregisterReceiver (screenReceiver);
            ScreenReceiver.ScreenLocked = false;
            _orientationListener = null;
            if (Game != null)
                Game.Dispose();
            Game = null;

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnDestroy end");

            base.OnDestroy ();
		}
    }

	[CLSCompliant(false)]
	public static class ActivityExtensions
    {
        public static ActivityAttribute GetActivityAttribute(this AndroidGameActivity obj)
        {			
            var attr = obj.GetType().GetCustomAttributes(typeof(ActivityAttribute), true);
			if (attr != null)
			{
            	return ((ActivityAttribute)attr[0]);
			}
			return null;
        }
    }

}
