// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Gtk;
using Gdk;
using Eto.GtkSharp.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private bool _rootExists;

        public ProjectControlHandler()
        {
            // GUI

            Control = new ScrolledWindow();

            _treeView = new TreeView();
            _treeView.HeadersVisible = false;
            _treeView.Selection.Mode = SelectionMode.Multiple;

            Control.Add(_treeView);

            // Init

            _rootExists = false;
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

                if (_treeView.GetDestRowAtPos((int)args.Event.X, (int)args.Event.Y, out path, out pos) && _treeStore.GetIter(out iter, path))
                {
                    if (MainWindow.Controller.SelectedItems.Contains(_treeStore.GetValue(iter, 2) as IProjectItem))
                       args.RetVal = true;
                }
            }
        }

        private void TreeView_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            TreeViewDropPosition pos;
            TreePath path;

            if (args.Event.Button == 3 &&
                _treeView.GetDestRowAtPos((int)args.Event.X, (int)args.Event.Y, out path, out pos) &&
                _treeView.Selection.GetSelectedRows().ToList().Contains(path))
            {
                MainWindow.Instance.ShowContextMenu();
            }
        }

        public void SetRoot(IProjectItem item)
        {
            if (item == null)
            {
                _treeStore.Clear();
                _rootExists = false;
                return;
            }

            if (!_rootExists)
            {
                _iterRoot = _treeStore.AppendNode();
                _rootExists = true;
            }

            _treeStore.SetValue(_iterRoot, 0, _iconRoot);
            _treeStore.SetValue(_iterRoot, 1, item.Name);
            _treeStore.SetValue(_iterRoot, 2, item);
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
            TreeIter iter;
            if (FindItem(_iterRoot, item.OriginalPath, out iter))
                _treeStore.Remove(ref iter);
        }

        public void UpdateItem(IProjectItem item)
        {
            if (item is PipelineProject)
            {
                _treeView.ExpandRow(_treeStore.GetPath(_iterRoot), false);
                _treeView.Selection.UnselectAll();
                _treeView.Selection.SelectIter(_iterRoot);
                return;
            }

            TreeIter iter;
            if (FindItem(_iterRoot, item.OriginalPath, out iter))
            {
                if (item.SelectThis)
                {
                    _treeView.Selection.UnselectAll();
                    _treeView.Selection.SelectIter(iter);
                    item.SelectThis = false;
                }

                if (item.ExpandToThis)
                {
                    _treeView.ExpandToPath(_treeStore.GetPath(iter));
                    item.ExpandToThis = false;
                }

                SetExists(iter, item.Exists);
            }
        }

        public void SetExists(TreeIter iter, bool exists)
        {
            var item = _treeStore.GetValue(iter, 2) as IProjectItem;

            if (item is PipelineProject)
                return;

            if (item is DirectoryItem)
            {
                TreeIter iterChild;
                bool fex = exists;

                if (_treeStore.IterChildren(out iterChild, iter))
                {
                    do
                    {
                        var iex = (_treeStore.GetValue(iter, 2) as IProjectItem).Exists;

                        if (!exists)
                            fex = false;
                    }
                    while (_treeStore.IterNext(ref iterChild));
                }

                _treeStore.SetValue(iter, 0, Global.GetDirectoryIcon(fex).ControlObject);
            }
            else
                _treeStore.SetValue(iter, 0, Global.GetFileIcon(MainWindow.Controller.GetFullPath(item.OriginalPath), exists).ControlObject);
            
            _treeStore.IterParent(out iter, iter);
            SetExists(iter, exists);
        }

        public bool FindItem(TreeIter root, string path, out TreeIter iter)
        {
            var split = path.Split('/');

            if (GetItem(root, split[0], out iter))
            {
                if (split.Length != 1)
                    return FindItem(iter, string.Join("/", split, 1, split.Length - 1), out iter);
                
                return true;
            }

            return false;
        }

        public bool GetItem(TreeIter root, string text, out TreeIter iter)
        {
            TreeIter childIter;
            if (_treeStore.IterChildren(out childIter, root))
            {
                do
                {
                    if (_treeStore.GetValue(childIter, 1).ToString() == text)
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
                icon = Global.GetDirectoryIcon(item.Exists).ControlObject as Pixbuf;
            else
                icon = Global.GetFileIcon(MainWindow.Controller.GetFullPath(item.OriginalPath), item.Exists).ControlObject as Pixbuf;

            var ret = _treeStore.InsertNode(root, pos);
            _treeStore.SetValue(ret, 0, icon);
            _treeStore.SetValue(ret, 1, item.Name);
            _treeStore.SetValue(ret, 2, item);

            return ret;
        }
    }
}
