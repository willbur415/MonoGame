// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    class SDLGameWindow : GameWindow, IDisposable
    {
        public override bool AllowUserResizing
        {
            get
            {
                return !this.IsBorderless && _resizable;
            }
            set
            {
                if (_init)
                    throw new Exception("SDL does not support changing resizable parameter of the window after it's already been created.");
                
                _resizable = value;
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                int x = 0, y = 0, w, h;

                SDL.Window.GetSize(Handle, out w, out h);

                if (!_isFullScreen)
                {
                    SDL.Window.GetPosition(Handle, out x, out y);

                    if (!IsBorderless)
                    {
                        x += BorderX;
                        y += BorderY;
                    }
                }

                return new Rectangle(x, y, w, h);
            }
        }

        public override Point Position
        {
            get
            {
                int x = 0, y = 0;

                if (!_isFullScreen)
                    SDL.Window.GetPosition(Handle, out x, out y);
                
                return new Point(x, y);
            }
            set
            {
                SDL.Window.SetPosition(Handle, value.X, value.Y);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return DisplayOrientation.LandscapeLeft;
            }
        }

        public override IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }

        public override string ScreenDeviceName
        {
            get
            {
                return _screenDeviceName;
            }
        }

        public override bool IsBorderless
        {
            get
            {
                return _borderless;
            }
            set
            {
                SDL.Window.SetBordered(this._handle, value ? 1 : 0);
                _borderless = value;
            }
        }

        internal static GameWindow Instance;

        internal int BorderX, BorderY;
        internal bool _isFullScreen;

        private Game _game;
        private IntPtr _handle;
        private bool _init, _disposed;
        private bool _resizable, _borderless, _willBeFullScreen, _mouseVisible;
        private string _screenDeviceName;
        private SDL.Rectangle _display;

        public SDLGameWindow(Game game)
        {
            this._game = game;
            this._screenDeviceName = "";

            Instance = this;

            _display = GetMouseDiaplay ();

            // We need a dummy handle for GraphicDevice until our window gets created
            this._handle = SDL.Window.Create("", _display.X + _display.Width / 4, _display.Y + _display.Height / 4, GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight, SDL.Window.State.Hidden);
        }

        internal void CreateWindow()
        {
            var width = GraphicsDeviceManager.DefaultBackBufferWidth;
            var height = GraphicsDeviceManager.DefaultBackBufferHeight;
            var title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();

            var initflags = 
                SDL.Window.State.OpenGL |
                SDL.Window.State.Hidden |
                SDL.Window.State.InputFocus |
                SDL.Window.State.MouseFocus;

            if (_resizable)
                initflags |= SDL.Window.State.Resizable;

            SDL.Window.Destroy(_handle);
            this._handle = SDL.Window.Create(title, 
                _display.X + _display.Width / 2 - width / 2, 
                _display.Y + _display.Height / 2 - height / 2, 
                width, height, initflags);

            SDL.SetHint ("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");

            using (var stream = Assembly.GetEntryAssembly ().GetManifestResourceStream (Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace + ".Icon.bmp") ??
                   Assembly.GetEntryAssembly ().GetManifestResourceStream ("Icon.bmp") ??
                   Assembly.GetExecutingAssembly ().GetManifestResourceStream ("MonoGame.bmp")) {

                using (BinaryReader br = new BinaryReader (stream)) {
                    var src = SDL.RWFromMem (br.ReadBytes ((int)stream.Length), (int)stream.Length);
                    var icon = SDL.LoadBMP_RW (src, 1);
                    SDL.SetWindowIcon (_handle, icon);
                }
            }

            SetCursorVisible(_mouseVisible);

            // TODO, per platform border size detection

            _init = true;
        }

        ~SDLGameWindow()
        {
            Dispose(false);
        }

        private SDL.Rectangle GetMouseDiaplay()
        {
            SDL.Rectangle rect = new SDL.Rectangle();

            int x, y;
            SDL.Mouse.GetGlobalState (out x, out y);

            var displayCount = SDL.Display.GetNumVideoDisplays();
            for (int i = 0; i < displayCount; i++) 
            {
                SDL.Display.GetBounds(i, out rect);

                if (x >= rect.X && x < rect.X + rect.Width && 
                    y >= rect.Y && y < rect.Y + rect.Height) {
                    return rect;
                }
            }

            return rect;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            SDL.Mouse.ShowCursor(visible ? 1 : 0);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            this._screenDeviceName = screenDeviceName;

            var prevBounds = ClientBounds;
            var displayIndex = SDL.Window.GetDisplayIndex(Handle);

            SDL.Rectangle displayRect;
            SDL.Display.GetBounds(displayIndex, out displayRect);
            
            if (_willBeFullScreen != _isFullScreen)
            {
                var fullscreenFlag = _game.graphicsDeviceManager.HardwareModeSwitch ? SDL.Window.State.Fullscreen : SDL.Window.State.FullscreenDesktop;
                SDL.Window.SetFullscreen(Handle, (_willBeFullScreen) ? fullscreenFlag : 0);
            }

            if (!_willBeFullScreen)
                SDL.Window.SetSize(Handle, clientWidth, clientHeight);

            var centerX = Math.Max(prevBounds.X - ((IsBorderless || _isFullScreen) ? 0 : BorderX) + ((prevBounds.Width - clientWidth) / 2), 0);
            var centerY = Math.Max(prevBounds.Y - ((IsBorderless || _isFullScreen) ? 0 : BorderY) + ((prevBounds.Height - clientHeight) / 2), 0);

            if (_isFullScreen && !_willBeFullScreen)
            {
                centerX += displayRect.X;
                centerY += displayRect.Y;
            }

            // If this window is resizable, there is a bug in SDL where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is 
            // to not try and set the window position because it will be wrong.
            if (!AllowUserResizing)
                SDL.Window.SetPosition (Handle, centerX, centerY);

            _isFullScreen = _willBeFullScreen;
            OnClientSizeChanged();
        }

        public void ClientResize(int width, int height)
        {
            _game.GraphicsDevice.PresentationParameters.BackBufferWidth = width;
            _game.GraphicsDevice.PresentationParameters.BackBufferHeight = height;

            _game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

            OnClientSizeChanged ();
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Nothing to do here
        }

        protected override void SetTitle(string title)
        {
            SDL.Window.SetTitle(this._handle, title);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                SDL.Window.Destroy(_handle);
                _handle = IntPtr.Zero;

                _disposed = true;
            }
        }
    }
}

