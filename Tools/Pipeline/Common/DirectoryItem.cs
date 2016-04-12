// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Tools.Pipeline
{
    public class DirectoryItem : IProjectItem
    {
        public DirectoryItem(string name, string location)
        {
            Name = name;
            Location = location;
            Exists = true;
        }

        #region IProjectItem

        public string OriginalPath { get; set; }

        public string Name { set; get; }

        public string Location { set; get; }

        public string Icon
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Exists { get; set; }

        #endregion
    }
}

