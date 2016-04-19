// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    class CellText : CellBase
    {
        public CellText(string category, string name, object value, bool editable) : base(category, name, value)
        {
            Editable = editable && value is string;
        }

        public override void Edit(Control control)
        {
            var dialog = new DialogBase();
            var editText = new TextBox();
            editText.Text = Value.ToString();

            dialog.CreateContent(editText);
            dialog.Run(control);
        }
    }
}
