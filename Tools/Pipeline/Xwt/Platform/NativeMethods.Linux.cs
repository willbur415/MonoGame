using System.Runtime.InteropServices;
using System;
using GLib;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public static class NativeMethods
    {
        public const string giolibpath = "libgio-2.0.so.0";

        [DllImport (giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_new_for_path (string path);

        [DllImport (giolibpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr g_file_query_info (IntPtr gfile, string attributes, int flag, IntPtr cancelable, IntPtr error);

        private static IconTheme theme = IconTheme.Default;
        private static Xwt.Drawing.Image folder = Xwt.Drawing.Image.FromFile(theme.LookupIcon("folder", 16, (IconLookupFlags)0).Filename);

        public static Xwt.Drawing.Image GetFolderImage()
        {
            return folder;
        }

        public static Xwt.Drawing.Image GetFileImage(string path)
        {
            FileInfo info = new FileInfo(g_file_query_info(g_file_new_for_path(path), "standard::*", 0, new IntPtr(), new IntPtr()));

            try
            {
                string[] sicon = info.Icon.ToString().Split(' ');

                for(int i = sicon.Length - 1;i >= 1;i--)
                {
                    try
                    {
                        var ii = theme.LookupIcon(sicon[i], 16, (IconLookupFlags)0);

                        if(ii != null)
                        {
                            return Xwt.Drawing.Image.FromFile(ii.Filename);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return Xwt.Drawing.Image.FromFile(theme.LookupIcon("text-x-generic", 16, (IconLookupFlags)0).Filename);
        }
    }
}

