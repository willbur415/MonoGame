// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

﻿using System;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.GtkSharp.Drawing;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    static partial class Gtk3Wrapper
    {
        public const string giolibpath = "libgio-2.0.so.0";

        [DllImport(giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_query_info(IntPtr gfile, string attributes, int flag, IntPtr cancelable, IntPtr error);

        [DllImport(giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_new_for_path(string path);
    }

    static partial class Global
    {
        static IconTheme theme = IconTheme.Default;

        static void PlatformShowOpenWithDialog(string filePath)
        {
            
        }

        static Bitmap PlatformGetDirectoryIcon(bool exists)
        {
            if (!exists)
                throw new NotImplementedException();
            
            return new Bitmap(new BitmapHandler(theme.LoadIcon("folder", 16, 0)));
        }

        static Bitmap PlatformGetFileIcon(string path, bool exists)
        {
            Gdk.Pixbuf icon = null;

            if (!exists)
                throw new NotImplementedException();

            try
            {
                var info = new GLib.FileInfo(Gtk3Wrapper.g_file_query_info(Gtk3Wrapper.g_file_new_for_path(path), "standard::*", 0, new IntPtr(), new IntPtr()));
                var sicon = info.Icon.ToString().Split(' ');

                for (int i = sicon.Length - 1; i >= 1; i--)
                {
                    try
                    {
                        icon = theme.LoadIcon(sicon[i], 16, 0);
                        if (icon != null)
                            break;
                    }
                    catch { }
                }
            }
            catch { }

            if (icon == null)
                icon = theme.LoadIcon("text-x-generic", 16, 0);

            return new Bitmap(new BitmapHandler(icon));
        }
    }
}

