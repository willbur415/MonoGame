// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;
using Eto;

namespace MonoGame.Tools.Pipeline
{
    interface IProjectControl : Control.IHandler
    {
        void SetRoot(IProjectItem item);

        void AddItem(IProjectItem item);

        void RemoveItem(IProjectItem item);

        void UpdateItem(IProjectItem item);
    }

    [Handler(typeof(IProjectControl))]
    class ProjectControl : Control
    {
        public new IProjectControl Handler { get { return (IProjectControl)base.Handler; } }

        public void SetRoot(IProjectItem item)
        {
            Handler.SetRoot(item);
        }

        public void AddItem(IProjectItem item)
        {
            Handler.AddItem(item);
        }

        public void RemoveItem(IProjectItem item)
        {
            Handler.RemoveItem(item);
        }

        public void UpdateItem(IProjectItem item)
        {
            Handler.UpdateItem(item);
        }
    }
}
