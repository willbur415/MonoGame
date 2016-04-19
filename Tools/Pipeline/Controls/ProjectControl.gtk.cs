// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.GtkSharp.Forms.Controls;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl
    {
        Gtk.TreeView _gtkTreeView;

        private void Init()
        {
            _gtkTreeView = (treeView1.ControlObject as Gtk.ScrolledWindow).Children[0] as Gtk.TreeView;
            _gtkTreeView.Selection.Mode = Gtk.SelectionMode.Multiple;
            _gtkTreeView.Selection.Changed += Selection_Changed;
        }

        public void Selection_Changed(object sender, EventArgs e)
        {
            var items = new List<IProjectItem>();
            var paths = _gtkTreeView.Selection.GetSelectedRows();

            foreach (var path in paths)
            {
                var item = (treeView1.Handler as TreeViewHandler).GetItem(path) as TreeItem;
                items.Add(item.Tag as IProjectItem);
            }

            MainWindow.Controller.SelectionChanged(items);
        }
    }
}
