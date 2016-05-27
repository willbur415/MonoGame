// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.WinForms.Forms.Controls;
using Eto.WinForms.Forms.Menu;
using Eto.WinForms.Forms.ToolBar;
using System.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<LabelHandler>("Wrap", h => h.Control.MaximumSize = new Size(400, 0));
            Style.Add<GridViewHandler>("GridView", h =>
            {
                h.Control.BackgroundColor = SystemColors.Window;
                h.Control.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            });
            Style.Add<MenuBarHandler>("MenuBar", h =>
            {
                h.Control.BackColor = SystemColors.Control;
            });
            Style.Add<ToolBarHandler>("ToolBar", h =>
            {
                h.Control.BackColor = SystemColors.Control;
                h.Control.Padding = new System.Windows.Forms.Padding(4);
                h.Control.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
                h.Control.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            });
            Style.Add<TreeViewHandler>("FilterView", h =>
            {
                h.Control.ItemHeight = 20;
                h.Control.ShowLines = false;
                h.Control.FullRowSelect = true;
            });
        }

        private static void Control_LostFocus(object sender, System.EventArgs e)
        {

        }
    }
}
