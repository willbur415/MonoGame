// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Html5;
using Bridge.WebGL;
using Microsoft.Xna.Framework.Input;

#if WEB
using IntPtr = Microsoft.Xna.Framework.IntPtr;
#endif

static class Web
{
    public static WebGLRenderingContext GL;
}

namespace Microsoft.Xna.Framework
{
    class WebGameWindow : GameWindow
    {
        private HTMLCanvasElement _canvas;
        private bool _isFullscreen, _willBeFullScreen;
        private string _screenDeviceName;
        private Game _game;
        private List<Keys> _keys;

        public WebGameWindow(Game game)
        {
            _game = game;
            _keys = new List<Keys>();

            Keyboard.SetKeys(_keys);

            _canvas = Document.GetElementById("monogamecanvas") as HTMLCanvasElement;
            _canvas.Width = GraphicsDeviceManager.DefaultBackBufferWidth;
            _canvas.Height = GraphicsDeviceManager.DefaultBackBufferHeight;
            _canvas.TabIndex = 1000;

            // Disable selection
            _canvas.Style.SetProperty("-webkit-touch-callout", "none");
            _canvas.Style.SetProperty("-webkit-user-select", "none");
            _canvas.Style.SetProperty("-khtml-user-select", "none");
            _canvas.Style.SetProperty("-moz-user-select", "none");
            _canvas.Style.SetProperty("-ms-user-select", "none");
            _canvas.Style.SetProperty("user-select", "none");

            // TODO: Move "GL context" creation outside the game window
            var possiblecontexts = new[] { "webgl", "experimental-webgl", "webkit-3d", "moz-webgl" };
            foreach(var context in possiblecontexts)
            {
                try
                {
                    Web.GL = _canvas.GetContext(context).As<WebGLRenderingContext>();
                    if (Web.GL != null)
                        break;
                }
                catch { }
            }

            if (Web.GL == null)
                throw new Exception("Failed to get WebGL context :|");

            // Block context menu on the canvas element
            _canvas.OnContextMenu += (e) => e.PreventDefault();

            // Connect events
            _canvas.OnMouseMove += Canvas_MouseMove;
            _canvas.OnMouseDown += Canvas_MouseDown;
            _canvas.OnMouseUp += Canvas_MouseUp;
            _canvas.OnMouseWheel += Canvas_MouseWheel;
            _canvas.OnKeyDown += Canvas_KeyDown;
            _canvas.OnKeyUp += Canvas_KeyUp;

            Document.AddEventListener("webkitfullscreenchange", Document_FullscreenChange);
            Document.AddEventListener("mozfullscreenchange", Document_FullscreenChange);
            Document.AddEventListener("fullscreenchange", Document_FullscreenChange);
            Document.AddEventListener("MSFullscreenChange", Document_FullscreenChange);
        }

        // Fullscreen can only be interacted with on user interaction events
        // so make sure we connect this code to as many input events as possible
        private void EnsureFullscreen()
        {
            var isfull = Script.Eval<bool>("(document.fullScreenElement && document.fullScreenElement !== null) || document.mozFullScreen || document.webkitIsFullScreen");

            if (_isFullscreen != isfull)
            {
                if (_isFullscreen)
                {
                    Script.Write(@"
                    var f_elem = document.getElementById('monogamecanvas');
                    var f_method = f_elem.requestFullscreen || f_elem.msRequestFullscreen || f_elem.mozRequestFullScreen || f_elem.webkitRequestFullscreen;
                    f_method.call(f_elem);
                    ");
                }
                else
                {
                    Script.Write(@"
                    var f_method = document.exitFullscreen || document.msExitFullscreen || document.mozCancelFullScreen || document.webkitExitFullscreen;
                    f_method.call(document);
                    ");
                }
            }
        }

        private void Document_FullscreenChange()
        {
            _isFullscreen = Script.Eval<bool>("(document.fullScreenElement && document.fullScreenElement !== null) || document.mozFullScreen || document.webkitIsFullScreen");
            
            if (_isFullscreen)
            {
                _canvas.Width = Document.DocumentElement.ClientWidth;
                _canvas.Height = Document.DocumentElement.ClientHeight;
            }
            else
            {
                _canvas.Width = GraphicsDeviceManager.DefaultBackBufferWidth;
                _canvas.Height = GraphicsDeviceManager.DefaultBackBufferHeight;
            }

            _game.graphicsDeviceManager.IsFullScreen = _isFullscreen;
        }

        private void Canvas_MouseMove(MouseEvent e)
        {
            this.MouseState.X = e.ClientX - _canvas.OffsetLeft;
            this.MouseState.Y = e.ClientY - _canvas.OffsetTop;
        }

        private void Canvas_MouseDown(MouseEvent e)
        {
            switch(e.Button)
            {
                case 0:
                    this.MouseState.LeftButton = ButtonState.Pressed;
                    break;
                case 1:
                    this.MouseState.MiddleButton = ButtonState.Pressed;
                    break;
                case 2:
                    this.MouseState.RightButton = ButtonState.Pressed;
                    break;
            }

            EnsureFullscreen();
        }

        private void Canvas_MouseUp(MouseEvent e)
        {
            switch(e.Button)
            {
                case 0:
                    this.MouseState.LeftButton = ButtonState.Released;
                    break;
                case 1:
                    this.MouseState.MiddleButton = ButtonState.Released;
                    break;
                case 2:
                    this.MouseState.RightButton = ButtonState.Released;
                    break;
            }

            EnsureFullscreen();
        }

        private void Canvas_MouseWheel(MouseEvent e)
        {
            if (e.Detail < 0)
                this.MouseState.ScrollWheelValue += 120;
            else
                this.MouseState.ScrollWheelValue -= 120;
        }

        private void Canvas_KeyDown(KeyboardEvent e)
        {
            var xnaKey = KeyboardUtil.ToXna(e.KeyCode, int.Parse(e.Location.ToString()));

            if (!_keys.Contains(xnaKey))
                _keys.Add(xnaKey);

            Keyboard.CapsLock = (e.KeyCode == 20) ? !Keyboard.CapsLock : e.GetModifierState("CapsLock");
            Keyboard.NumLock = (e.KeyCode == 144) ? !Keyboard.NumLock : e.GetModifierState("NumLock");

            EnsureFullscreen();
        }

        private void Canvas_KeyUp(KeyboardEvent e)
        {
            _keys.Remove(KeyboardUtil.ToXna(e.KeyCode, int.Parse(e.Location.ToString())));

            EnsureFullscreen();
        }

        public override bool AllowUserResizing
        {
            get => false;
            set { }
        }

        public override Rectangle ClientBounds => new Rectangle(0, 0, _canvas.Width, _canvas.Height);

        public override DisplayOrientation CurrentOrientation => DisplayOrientation.Default;

        public override IntPtr Handle => IntPtr.Zero;

        public override string ScreenDeviceName => _screenDeviceName;

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _screenDeviceName = screenDeviceName;

            if (!_isFullscreen && !_willBeFullScreen)
            {
                _canvas.Width = clientWidth;
                _canvas.Height = clientHeight;
            }

            _isFullscreen = _willBeFullScreen;
        }

        protected override void SetTitle(string title)
        {
            Document.Title = title;
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            
        }
    }
}

