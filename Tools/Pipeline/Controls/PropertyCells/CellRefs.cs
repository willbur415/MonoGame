// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    class CellRefs : CellBase
    {
        public CellRefs(string category, string name, object value) : base(category, name, value)
        {
            DisplayValue = (Value as List<string>).Count > 0 ? "Collection" : "None";
        }

        public override void Edit(Control control)
        {
            var dialog = new ReferenceDialog(MainWindow.Controller, (Value as List<string>).ToArray());
            dialog.Run(control);
        }
    }
}
