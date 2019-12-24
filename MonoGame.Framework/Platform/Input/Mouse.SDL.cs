// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        internal static int ScrollX;
        internal static int ScrollY;

        private static Dictionary<MouseCursor, IntPtr> _cursors;

        static Mouse()
        {
            _cursors = new Dictionary<MouseCursor, IntPtr>();
            _cursors[MouseCursor.Arrow] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.Arrow);
            _cursors[MouseCursor.IBeam] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.IBeam);
            _cursors[MouseCursor.Wait] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.Wait);
            _cursors[MouseCursor.Crosshair] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.Crosshair);
            _cursors[MouseCursor.WaitArrow] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.WaitArrow);
            _cursors[MouseCursor.SizeNWSE] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.SizeNWSE);
            _cursors[MouseCursor.SizeNESW] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.SizeNESW);
            _cursors[MouseCursor.SizeWE] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.SizeWE);
            _cursors[MouseCursor.SizeNS] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.SizeNS);
            _cursors[MouseCursor.SizeAll] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.SizeAll);
            _cursors[MouseCursor.No] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.No);
            _cursors[MouseCursor.Hand] = Sdl.Mouse.CreateSystemCursor(Sdl.Mouse.SystemCursor.Hand);
        }

        private static IntPtr PlatformGetWindowHandle()
        {
            return PrimaryWindow.Handle;
        }
        
        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            int x, y;
            var winFlags = Sdl.Window.GetWindowFlags(window.Handle);
            var state = Sdl.Mouse.GetGlobalState(out x, out y);

            if ((winFlags & Sdl.Window.State.MouseFocus) != 0)
            {
                // Window has mouse focus, position will be set from the motion event
                window.MouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

                window.MouseState.HorizontalScrollWheelValue = ScrollX;
                window.MouseState.ScrollWheelValue = ScrollY;
            }
            else
            {
                // Window does not have mouse focus, we need to manually get the position
                var clientBounds = window.ClientBounds;
                window.MouseState.X = x - clientBounds.X;
                window.MouseState.Y = y - clientBounds.Y;
            }

            return window.MouseState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
            
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        private static void PlatformSetCursor(MouseCursor cursor, Texture2D texture = null, int originx = 0, int originy = 0)
        {
            if (cursor != MouseCursor.Texture)
            {
                Sdl.Mouse.SetCursor(_cursors[cursor]);
                return;
            }

            var handle = IntPtr.Zero;
            var surface = IntPtr.Zero;

            if (_cursors.TryGetValue(cursor, out handle) && handle != IntPtr.Zero)
            {
                Sdl.Mouse.FreeCursor(handle);
                handle = IntPtr.Zero;
            }

            try
            {
                var bytes = new byte[texture.Width * texture.Height * 4];
                texture.GetData(bytes);
                surface = Sdl.CreateRGBSurfaceFrom(bytes, texture.Width, texture.Height, 32, texture.Width * 4, 0x000000ff, 0x0000FF00, 0x00FF0000, 0xFF000000);
                
                if (surface == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to create surface for mouse cursor: " + Sdl.GetError());

                handle = Sdl.Mouse.CreateColorCursor(surface, originx, originy);
            }
            finally
            {
                if (surface != IntPtr.Zero)
                    Sdl.FreeSurface(surface);

                if (handle != IntPtr.Zero)
                    Sdl.Mouse.SetCursor(handle);
            }

        }
    }
}
