#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

using OpenTK;
using OpenTK.Graphics;

namespace Microsoft.Xna.Framework
{
    class OpenTKGamePlatform : GamePlatform
    {
        private OpenTKGameWindow _view;
		private OpenALSoundController soundControllerInstance = null;
        // stored the current screen state, so we can check if it has changed.
        private bool isCurrentlyFullScreen = false;
        private Toolkit toolkit;
        private DisplayDevice CurrentDisplay;

		public OpenTKGamePlatform(Game game)
            : base(game)
        {
            toolkit = Toolkit.Init();
            _view = new OpenTKGameWindow(game);
            this.Window = _view;

			// Setup our OpenALSoundController to handle our SoundBuffer pools
            try
            {
                soundControllerInstance = OpenALSoundController.GetInstance;
            }
            catch (DllNotFoundException ex)
            {
                throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
            }
            
#if LINUX
            // also set up SdlMixer to play background music. If one of these functions fails, we will not get any background music (but that should rarely happen)
            Tao.Sdl.Sdl.SDL_InitSubSystem(Tao.Sdl.Sdl.SDL_INIT_AUDIO);
            Tao.Sdl.SdlMixer.Mix_OpenAudio(44100, (short)Tao.Sdl.Sdl.AUDIO_S16SYS, 2, 1024);			

            //even though this method is called whenever IsMouseVisible is changed it needs to be called during startup
            //so that the cursor can be put in the correct inital state (hidden)
            OnIsMouseVisibleChanged();
#endif
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _view.SetMouseVisible(IsMouseVisible);
        }

        public override void RunLoop()
        {
            ResetWindowBounds();
            _view.Run();
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The desktop platform does not support asynchronous run loops");
        }
        
        public override void Exit()
        {
            if (_view.Window.Exists)
            {
                //(SJ) Why is this called here when it's not in any other project
                //Net.NetworkSession.Exit();
                _view.Window.Close();
            }
#if LINUX
            Tao.Sdl.SdlMixer.Mix_CloseAudio();
#endif
            if (CurrentDisplay != null)
            {
                CurrentDisplay.RestoreResolution();
            }
        }

        public override void BeforeInitialize()
        {
            _view.Window.Visible = true;
            base.BeforeInitialize();
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            IsActive = _view.Window.Focused;

            // Update our OpenAL sound buffer pools
            if (soundControllerInstance != null)
                soundControllerInstance.Update();
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
            ResetWindowBounds();
        }

        public override void ExitFullScreen()
        {
            ResetWindowBounds();
        }

        internal DisplayMode CurrentDisplayMode
        {
            get
            {
                if (CurrentDisplay != null)
                {
                    return new DisplayMode(
                        CurrentDisplay.Width, CurrentDisplay.Height,
                        (int)CurrentDisplay.RefreshRate, SurfaceFormat.Color);
                }
                // Unknown display mode (running on a system without a monitor?)
                // Few applications will expect this - return a reasonable default to avoid crashes.
                return new DisplayMode(800, 480, 60, SurfaceFormat.Color);
            }
        }

        internal DisplayModeCollection SupportedDisplayModes
        {
            get
            {
                DisplayModeCollection mode_collection;
                if (CurrentDisplay != null)
                {
                    var modes = new List<DisplayMode>(CurrentDisplay.AvailableResolutions.Count);
                    foreach (OpenTK.DisplayResolution resolution in CurrentDisplay.AvailableResolutions)
                    {
                        SurfaceFormat format = SurfaceFormat.Color;
                        switch (resolution.BitsPerPixel)
                        {
                            case 32: format = SurfaceFormat.Color; break;
                            case 16: format = SurfaceFormat.Bgr565; break;
                            case 8: format = SurfaceFormat.Alpha8; break;
                            default:
                                break;
                        }

                        // Just report the 32 bit surfaces for now
                        // Need to decide what to do about other surface formats
                        if (format == SurfaceFormat.Color)
                        {
                            modes.Add(new DisplayMode(resolution.Width, resolution.Height, (int)resolution.RefreshRate, format));
                        }
                    }
                    mode_collection = new DisplayModeCollection(modes);
                }
                else
                {
                    mode_collection = new DisplayModeCollection(new List<DisplayMode> { CurrentDisplayMode });
                }
                return mode_collection;
            }
        }

        internal void ResetWindowBounds()
        {
            //Changing window style forces a redraw. Some games
            //have fail-logic and toggle fullscreen in their draw function,
            //so temporarily become inactive so it won't execute.

            bool wasActive = IsActive;
            IsActive = false;

            var graphicsDeviceManager = (GraphicsDeviceManager)
                Game.Services.GetService(typeof(IGraphicsDeviceManager));

            var bounds = new Rectangle(
                0, 0,
                graphicsDeviceManager.PreferredBackBufferWidth,
                graphicsDeviceManager.PreferredBackBufferHeight);
            var center = _view.Window.PointToScreen(
                new System.Drawing.Point(
                    (int)bounds.Center.X,
                    (int)bounds.Center.Y));

            // Find which DisplayDevice contains the center of the GameWindow
            for (var i = DisplayIndex.First; i < DisplayIndex.Sixth; i++)
            {
                var d = DisplayDevice.GetDisplay(i);
                if (d != null && d.Bounds.Contains(center))
                {
                    CurrentDisplay = d;
                    break;
                }
            }

            if (graphicsDeviceManager.IsFullScreen)
            {
                // Change the resolution of CurrentDisplay to match the preferred
                // backbuffer size.
                if (CurrentDisplay != null)
                {
                    DisplayMode selected = null;
                    int score = Int32.MaxValue;
                    int width = graphicsDeviceManager.PreferredBackBufferWidth;
                    int height = graphicsDeviceManager.PreferredBackBufferHeight;
                    float ratio = width / (float) height;

                    if (CurrentDisplay.Width != width || CurrentDisplay.Height != height)
                    {
                        // Select the closest matching DisplayMode.
                        // Note: when a requested resolution is not supported, XNA appears to perform
                        // a mode switch to the same resolution as before. For example, on a portrait monitor:
                        // 1200x1920 (default), 800x480 (preferred) -> mode switch to 1200x1920.
                        // Should we do the same thing, or should we actually implement proper mode switching?
                        foreach (var mode in SupportedDisplayModes)
                        {
                            int distance = 0;
                            distance += Math.Abs(mode.Width * mode.Height - width * height);
                            distance += (int)(Math.Abs(mode.AspectRatio - ratio) * 1000); // avoid wrong aspect ratios
                            if (distance < score)
                            {
                                score = distance;
                                selected = mode;
                            }
                        }

                        if (selected != null)
                        {
                            CurrentDisplay.ChangeResolution(selected.Width, selected.Height, 32, selected.RefreshRate);
                        }
                    }
                }
            }
            else
            {
                if (CurrentDisplay != null)
                {
                    // Restore the origin resolution of CurrentDisplay
                    CurrentDisplay.RestoreResolution();
                    bounds.Width = graphicsDeviceManager.PreferredBackBufferWidth;
                    bounds.Height = graphicsDeviceManager.PreferredBackBufferHeight;
                }
            }
            

            // Now we set our Presentation Parameters
            var device = (GraphicsDevice)graphicsDeviceManager.GraphicsDevice;
            // FIXME: Eliminate the need for null checks by only calling
            //        ResetWindowBounds after the device is ready.  Or,
            //        possibly break this method into smaller methods.
            if (device != null)
            {
                PresentationParameters parms = device.PresentationParameters;
                parms.BackBufferHeight = (int)bounds.Height;
                parms.BackBufferWidth = (int)bounds.Width;
            }

            if (graphicsDeviceManager.IsFullScreen != isCurrentlyFullScreen)
            {                
                _view.ToggleFullScreen();
            }

            // we only change window bounds if we are not fullscreen
            // or if fullscreen mode was just entered
            if (!graphicsDeviceManager.IsFullScreen || (graphicsDeviceManager.IsFullScreen != isCurrentlyFullScreen))
                _view.ChangeClientBounds(bounds);

            // store the current fullscreen state
            isCurrentlyFullScreen = graphicsDeviceManager.IsFullScreen;

            IsActive = wasActive;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            
        }
        
        public override void Log(string Message)
        {
            Console.WriteLine(Message);
        }

        public override void Present()
        {
            base.Present();

            var device = Game.GraphicsDevice;
            if (device != null)
                device.Present();
        }
		
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (toolkit != null)
                {
                    toolkit.Dispose();
                    toolkit = null;
                }

                if (_view != null)
                {
                    _view.Dispose();
                    _view = null;
                }
            }

			base.Dispose(disposing);
        }
			
    }
}
