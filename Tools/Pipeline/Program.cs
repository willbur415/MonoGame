// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Eto;
using Eto.Forms;
using Eto.GtkSharp.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    static class Program
    {
        [STAThread]
        static void Main(string [] args)
        {
            Style.Add<ToolBarHandler>("toolbar", h => 
                {
                    h.Control.ToolbarStyle = Gtk.ToolbarStyle.Icons;
                    h.Control.IconSize = Gtk.IconSize.SmallToolbar;
                });
            new Application(Platforms.Gtk3).Run(new MainForm());
        }
    }
}
