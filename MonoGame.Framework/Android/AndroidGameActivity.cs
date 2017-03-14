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
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "Run 1");

            RequestWindowFeature (WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "Run 2");

            IntentFilter filter = new IntentFilter();
		    filter.AddAction(Intent.ActionScreenOff);
		    filter.AddAction(Intent.ActionScreenOn);
		    filter.AddAction(Intent.ActionUserPresent);

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "Run 3");

            screenReceiver = new ScreenReceiver();
		    RegisterReceiver(screenReceiver, filter);

            _orientationListener = new OrientationListener(this);
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "Run 4");

            Game.Activity = this;
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "Run end");

        }

        public static event EventHandler Paused;

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			// we need to refresh the viewport here.
			base.OnConfigurationChanged (newConfig);
		}

        protected override void OnPause()
        {
            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnPause 1");

            base.OnPause();
            if (Paused != null)
                Paused(this, EventArgs.Empty);

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

            if (Game != null)
            {
                var deviceManager = (IGraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
                if (deviceManager == null)
                    return;
                ((GraphicsDeviceManager)deviceManager).ForceSetFullScreen();
                ((AndroidGameWindow)Game.Window).GameView.RequestFocus();
                if (_orientationListener.CanDetectOrientation())
                    _orientationListener.Enable();
            }

            Android.Util.Log.Verbose ("AndroidGameView_AndroidGameActivity", "OnResume end");


        }

        protected override void OnDestroy ()
		{
            UnregisterReceiver(screenReceiver);
            ScreenReceiver.ScreenLocked = false;
            _orientationListener = null;
            if (Game != null)
                Game.Dispose();
            Game = null;
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
