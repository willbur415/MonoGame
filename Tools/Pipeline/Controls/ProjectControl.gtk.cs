// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Gtk;
using Gdk;
using Eto.GtkSharp.Forms;
using System.Collections.Generic;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    class CellRendererTag : CellRenderer
    {
        public object Tag { get; set; }

        public CellRendererTag()
        {
            Visible = false;
        }
    }

    class ProjectControlHandler : GtkControl<ScrolledWindow, ProjectControl, ProjectControl.ICallback>, IProjectControl
    {
        private TreeView _treeView;
        private Pixbuf _iconRoot;
        private TreeStore _treeStore;
        private TreeIter _iterRoot;

        public ProjectControlHandler()
        {
            // GUI

            Control = new ScrolledWindow();

            _treeView = new TreeView();
            _treeView.HeadersVisible = false;
            _treeView.Selection.Mode = SelectionMode.Multiple;

            Control.Add(_treeView);

            // Init

            _iconRoot = new Pixbuf(null, "TreeView.Root.png");

            var column = new TreeViewColumn();

            var cellIcon = new CellRendererPixbuf();
            var cellText = new CellRendererText();
            var cellTag = new CellRendererTag();

            column.PackStart(cellIcon, false);
            column.PackStart(cellText, false);
            column.PackStart(cellTag, false);

            _treeView.AppendColumn(column);

            column.AddAttribute(cellIcon, "pixbuf", 0);
            column.AddAttribute(cellText, "text", 1);
            column.AddAttribute(cellTag, "tag", 2);

            _treeStore = new TreeStore(typeof(Gdk.Pixbuf), typeof(string), typeof(object));
            _treeView.Model = _treeStore;

            // Events

            _treeView.Selection.Changed += TreeView_SelectionChanged;
            _treeView.ButtonPressEvent += TreeView_ButtonPressEvent;
            _treeView.ButtonReleaseEvent += TreeView_ButtonReleaseEvent;
        }

        private void TreeView_SelectionChanged(object sender, System.EventArgs e)
        {
            var selectedPaths = _treeView.Selection.GetSelectedRows();
            var selectedItems = new List<IProjectItem>();

            foreach (var path in selectedPaths)
            {
                TreeIter iter;

                if (_treeStore.GetIter(out iter, path))
                    selectedItems.Add(_treeStore.GetValue(iter, 2) as IProjectItem);
            }

            MainWindow.Controller.SelectionChanged(selectedItems);
        }

        [GLib.ConnectBefore]
        private void TreeView_ButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            if (args.Event.Button == 3)
            {
                TreeViewDropPosition pos;
                TreePath path;
                TreeIter iter;

                _treeView.GetDestRowAtPos((int)args.Event.X, (int)args.Event.Y, out path, out pos);

                if (_treeStore.GetIter(out iter, path))
                {
                    if (MainWindow.Controller.SelectedItems.Contains(_treeStore.GetValue(iter, 2) as IProjectItem))
                       args.RetVal = true;
                }
            }
        }

        private void TreeView_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button == 3)
                MainWindow.Instance.ShowContextMenu();
        }

        public void SetRoot(IProjectItem item)
        {
            if (item == null)
            {
                _treeStore.Clear();
                return;
            }

            _iterRoot = _treeStore.AppendValues(_iconRoot, item.Name, item);
        }

        public void AddItem(IProjectItem item)
        {
            AddItem(_iterRoot, item, item.OriginalPath, "");
        }

        public void AddItem(TreeIter root, IProjectItem item, string path, string currentPath)
        {
            var split = path.Split('/');
            var iter = GetorAddItem(root, split.Length > 1 ? new DirectoryItem(split[0], currentPath) { Exists = item.Exists } : item);

            if (path.Contains("/"))
                AddItem(iter, item, string.Join("/", split, 1, split.Length - 1), (currentPath + Path.DirectorySeparatorChar + split[0]).TrimStart(Path.DirectorySeparatorChar));
        }

        public void RemoveItem(IProjectItem item)
        {
            RemoveItem(_iterRoot, item.OriginalPath);
        }

        public void RemoveItem(TreeIter root, string path)
        {
            TreeIter iter;
            var split = path.Split('/');

            if (GetItem(root, split[0], out iter))
            {
                if (split.Length == 1)
                    _treeStore.Remove(ref iter);
                else
                    RemoveItem(iter, string.Join("/", split, 1, split.Length - 1));
            }
        }

        public bool GetItem(TreeIter root, string name, out TreeIter iter)
        {
            TreeIter childIter;
            if (_treeStore.IterChildren(out childIter, root))
            {
                do
                {
                    if (_treeStore.GetValue(childIter, 1).ToString() == name)
                    {
                        iter = childIter;
                        return true;
                    }
                }
                while (_treeStore.IterNext(ref childIter));
            }

            iter = new TreeIter();
            return false;
        }

        public TreeIter GetorAddItem(TreeIter root, IProjectItem item)
        {
            TreeIter childIter;
            var folder = item is DirectoryItem;

            var items = new List<string>();
            int pos = 0;

            if (_treeStore.IterChildren(out childIter, root))
            {
                do
                {
                    var itemtext = _treeStore.GetValue(childIter, 1).ToString();

                    if (itemtext == item.Name)
                        return childIter;

                    var itemTag = _treeStore.GetValue(childIter, 2) as IProjectItem;

                    if (folder)
                    {
                        if (itemTag is DirectoryItem)
                            items.Add(itemtext);
                    }
                    else
                    {
                        if (itemTag is DirectoryItem)
                            pos++;
                        else
                            items.Add(itemtext);
                    }
                }
                while (_treeStore.IterNext(ref childIter));
            }

            items.Add(item.Name);
            items.Sort();
            pos += items.IndexOf(item.Name);

            Pixbuf icon;
            if (item is DirectoryItem)
                icon = Global.GetGtkDirectoryIcon(item.Exists);
            else
                icon = Global.GetGtkFileIcon(MainWindow.Controller.GetFullPath(item.OriginalPath), item.Exists);

            var ret = _treeStore.InsertNode(root, pos);
            _treeStore.SetValue(ret, 0, icon);
            _treeStore.SetValue(ret, 1, item.Name);
            _treeStore.SetValue(ret, 2, item);

            return ret;
        }
    }
}
