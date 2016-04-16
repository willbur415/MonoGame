// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public class DirectoryItem : IProjectItem
    {
        public DirectoryItem(string path)
        {
            Name = Path.GetFileName(path);
            Location = Path.GetDirectoryName(path);
            OriginalPath = path;
            Exists = true;
        }

        public DirectoryItem(string name, string location)
        {
            Name = name;
            Location = location;
            OriginalPath = Path.Combine(location, name);
            Exists = true;
        }

        #region IProjectItem

        [Browsable(false)]
        public string OriginalPath { get; set; }

        [Category("Common")]
        [Description("The name of this folder.")]
        public string Name { set; get; }

        [Category("Common")]
        [Description("The file path to this folder.")]
        public string Location { set; get; }

        [Browsable(false)]
        public string Icon { get; set; }

        [Browsable(false)]
        public bool Exists { get; set; }

        #endregion
    }
}

