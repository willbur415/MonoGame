using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline
{
    public static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
            public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        }

        private static Icon GetFolderIcon()
        {
            IntPtr hImgSmall; //the handle to the system image list
            IntPtr hImgLarge; //the handle to the system image list
            string fName = "C:\\Windows"; //  'the file name to get icon from
            SHFILEINFO shinfo = new SHFILEINFO();

            hImgSmall = Win32.SHGetFileInfo(fName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);

            System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
            return myIcon;
        }

        private static Xwt.Drawing.Image FolderIcon;

        public static Xwt.Drawing.Image GetFolderImage()
        {
            if(FolderIcon == null)
            {
                /*var icon = GetFolderIcon();

                string s = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\IconData.ico";
                using (FileStream fs = new FileStream(s, FileMode.Create))
                    icon.Save(fs);

                

                var stream = new MemoryStream();
                icon.Save(stream);

                var ret = Xwt.Drawing.Image.FromStream(stream).WithSize(16);
                stream.Dispose();

                FolderIcon = ret;*/
                FolderIcon = Xwt.Drawing.Image.FromResource("MonoGame.Tools.Pipeline.Icons.folder_closed.png");
            }

            return FolderIcon;
        }

        public static Xwt.Drawing.Image GetFileImage(string path)
        {
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);

            var stream = new MemoryStream();
            icon.Save(stream);

            var ret = Xwt.Drawing.Image.FromStream(stream).WithSize(16);
            stream.Dispose();

            return ret;
        }
    }
}