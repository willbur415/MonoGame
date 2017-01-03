// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using Xwt.Drawing;

namespace MonoGame.Tools.Pipeline
{
    static partial class Global
    {
        private static Dictionary<string, Image> _xwtFiles;
        private static Image _xwtFileMissing, _xwtFolder, _xwtFolderMissing;

        private static void InitXwt()
        {
            _xwtFiles = new Dictionary<string, Image>();
            _xwtFiles.Add(".", Image.FromResource("TreeView.File.png"));
            _xwtFileMissing = Image.FromResource("TreeView.FileMissing.png");
            _xwtFolder = Image.FromResource("TreeView.Folder.png");
            _xwtFolderMissing = Image.FromResource("TreeView.FolderMissing.png");
        }

        public static Image GetXwtDirectoryIcon(bool exists)
        {
            return exists ? _xwtFolder : _xwtFolderMissing;
        }

        public static Image GetXwtFileIcon(string path, bool exists)
        {
            if (!exists)
                return _xwtFileMissing;

            var ext = Path.GetExtension(path);
            if (_xwtFiles.ContainsKey(ext))
                return _xwtFiles[ext];

            Image icon;

            try
            {
                icon = ToXwtImage(PlatformGetFileIcon(path));
            }
            catch
            {
                icon = _xwtFiles["."];
            }

            _xwtFiles.Add(ext, icon);
            return icon;
        }

        public static Image GetXwtIcon(string resource)
        {
#if LINUX
            var nativeicon = PlatformGetIcon(resource);

            if (nativeicon != null)
                return ToXwtImage(nativeicon);
#endif

            return Image.FromResource(resource);
        }
    }
}
