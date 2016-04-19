// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    class CellPath : CellBase
    {
        public CellPath(string category, string name, object value) : base(category, name, value)
        {
            
        }

        public override void Edit(Control control)
        {
            var dialog = new PathDialog(MainWindow.Controller, Value.ToString());
            dialog.Run(control);
        }
    }
}
