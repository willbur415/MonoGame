// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    public partial class PropertyGridTable
    {
        List<string> values;
        private int spacing = 16;

        public PropertyGridTable()
        {
            InitializeComponent();

            values = new List<string>();

            drawable.BackgroundColor = SystemColors.ControlBackground;
            drawable.Paint += Drawable_Paint;
        }

        private void Drawable_Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(SystemColors.ControlBackground);
            var y = 3;

            foreach (var v in values)
            {
                graphics.DrawText(SystemFonts.Default(), SystemColors.ControlText, 3, y, v);
                y += (int)SystemFonts.Default().Size + spacing;
            }

        }

        public void Clear()
        {
            values.Clear();
        }

        public void AddEntry(string category, PropertyInfo property, object value)
        {
            Console.WriteLine(category + " " + property.Name);
            values.Add(property.Name);
        }

        public void Update()
        {
            drawable.Update(new Rectangle(0, 0, drawable.Width, drawable.Height));
        }
    }
}

