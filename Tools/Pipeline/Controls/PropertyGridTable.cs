// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    static class PropInfo
    {
        public static int TextHeight;
        public static Color TextColor;
        public static Color BackColor;
        public static Color HoverTextColor;
        public static Color HoverBackColor;
        public static Color DisabledTextColor;
        public static Color BorderColor;

        static PropInfo()
        {
            TextHeight = (int)SystemFonts.Default().LineHeight;
            TextColor = SystemColors.ControlText;
            BackColor = SystemColors.ControlBackground;
            HoverTextColor = SystemColors.HighlightText;
            HoverBackColor = SystemColors.Highlight;
            DisabledTextColor = SystemColors.ControlText;
            DisabledTextColor.A = 0.4f;
            BorderColor = SystemColors.WindowBackground;
        }

        public static Color GetTextColor(bool selected, bool disabled)
        {
            if (disabled)
                return DisabledTextColor;

            return selected ? HoverTextColor : TextColor;
        }

        public static Color GetBackgroundColor(bool selected)
        {
            return selected ? HoverBackColor : BackColor;
        }
    }

    public partial class PropertyGridTable
    {
        public bool Group { get; set; }

        CellBase selectedCell;
        List<CellBase> cells;
        private int spacing = 12;
        PointF location = new PointF(-1, -1);

        public PropertyGridTable()
        {
            InitializeComponent();

            cells = new List<CellBase>();

            Group = true;
        }

        private void DrawGroup(Graphics g, Rectangle rec, string text)
        {
            var font = SystemFonts.Default();
            font = new Font(font.Family, font.Size, FontStyle.Bold);

            g.FillRectangle(PropInfo.BorderColor, rec);
            g.DrawText(SystemFonts.Default(), PropInfo.TextColor, rec.X + 1, rec.Y + (rec.Height - font.LineHeight) / 2, text);
        }

        private void Drawable_Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var rec = new Rectangle(0, 0, drawable.Width - 1, PropInfo.TextHeight + spacing);
            var separatorPos = rec.Width / 2;

            graphics.Clear(PropInfo.BackColor);

            string prevCategory = null;

            selectedCell = null;
            foreach (var c in cells)
            {
                if (prevCategory != c.Category)
                {
                    if (c.Category.Contains("Proc") || Group)
                    {
                        DrawGroup(graphics, rec, c.Category);
                        prevCategory = c.Category;
                        rec.Y += PropInfo.TextHeight + spacing;
                    }
                }

                var sel = rec.Contains((int)location.X, (int)location.Y);
                c.Draw(graphics, rec, separatorPos, sel);
                if (sel)
                    selectedCell = c;
                rec.Y += PropInfo.TextHeight + spacing;
            }

            drawable.Height = rec.Y + 1;
            graphics.FillRectangle(PropInfo.BackColor, separatorPos - 1, 0, 1, drawable.Height);
            graphics.FillRectangle(PropInfo.BorderColor, separatorPos - 1, 0, 1, drawable.Height);
        }

        private void Drawable_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var c = selectedCell;
            if (c != null && c.Editable)
                c.Edit(this);
        }

        private void Drawable_MouseMove(object sender, MouseEventArgs e)
        {
            location = e.Location;
            drawable.Invalidate();
        }

        private void Drawable_MouseLeave(object sender, MouseEventArgs e)
        {
            location = new PointF(-1, -1);
            drawable.Invalidate();
        }

        public void Clear()
        {
            cells.Clear();
        }

        public void AddEntry(string category, string name, object value, EventHandler eventHandler = null, bool editable = true)
        {
            if (value is Enum || value is Boolean || value is ImporterTypeDescription || value is ProcessorTypeDescription)
                cells.Add(new CellCombo(category, name, value, eventHandler));
            else if (name.Contains("Dir"))
                cells.Add(new CellPath(category, name, value, eventHandler));
            else if (value is IList)
                cells.Add(new CellRefs(category, name, value, eventHandler));
            else if (value is Microsoft.Xna.Framework.Color)
                cells.Add(new CellColor(category, name, value, eventHandler));
            else
                cells.Add(new CellText(category, name, value, eventHandler, editable));
        }

        public void Update()
        {
            if (Group)
                cells.Sort((x, y) => string.Compare(x.Category + x.Text, y.Category + y.Text) + (x.Category.Contains("Proc") ? 100 : 0) + (y.Category.Contains("Proc") ? -100 : 0));
            else
                cells.Sort((x, y) => string.Compare(x.Text, y.Text) + (x.Category.Contains("Proc") ? 100 : 0) + (y.Category.Contains("Proc") ? -100 : 0));

            drawable.Invalidate();
        }
    }
}
