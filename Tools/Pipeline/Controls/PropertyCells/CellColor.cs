// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    class CellColor : CellBase
    {
        Color color;

        public CellColor(string category, string name, object value) : base(category, name, value)
        {
            var tmp = (Microsoft.Xna.Framework.Color)value;
            color = new Color(tmp.R / 255f, tmp.G / 255f, tmp.B / 255f, tmp.A / 255f);
        }

        public override void Edit(Control control)
        {
            var dialog = new ColorDialog();
            dialog.Color = color;
            dialog.ShowDialog(control);
        }

        public override void DrawCell(Graphics g, Rectangle rec, int separatorPos, bool selected)
        {
            var border = rec.Height / 5;
            g.FillRectangle(color, separatorPos + border, rec.Y + border, rec.Width - separatorPos - 2 * border, rec.Height - 2 * border);
        }
    }
}
