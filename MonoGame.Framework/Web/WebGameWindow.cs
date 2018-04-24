// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Bridge.Html5;
using Bridge.WebGL;

#if WEB
using IntPtr = Microsoft.Xna.Framework.IntPtr;
#endif

namespace Microsoft.Xna.Framework
{
    class WebGameWindow : GameWindow
    {
        public static WebGLRenderingContext GL;

        HTMLCanvasElement _canvas;

        public WebGameWindow()
        {
            _canvas = Document.GetElementById("monogamecanvas") as HTMLCanvasElement;

            var possiblecontexts = new[] { "webgl", "experimental-webgl", "webkit-3d", "moz-webgl" };

            foreach(var context in possiblecontexts)
            {
                try
                {
                    GL = _canvas.GetContext(context).As<WebGLRenderingContext>();
                    if (GL != null)
                        break;
                }
                catch { }
            }

            if (GL == null)
                throw new Exception("Failed to get WebGL context :|");
        }

        public override bool AllowUserResizing
        {
            get => false;
            set { }
        }

        public override Rectangle ClientBounds => new Rectangle(0, 0, _canvas.Width, _canvas.Height);

        public override DisplayOrientation CurrentOrientation => DisplayOrientation.Default;

        public override IntPtr Handle => IntPtr.Zero;

        public override string ScreenDeviceName => string.Empty;

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            
        }

        protected override void SetTitle(string title)
        {
            
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            
        }
    }
}

