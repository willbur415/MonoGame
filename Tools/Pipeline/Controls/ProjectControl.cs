// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using Eto.Forms;
using Eto.Drawing;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl : Scrollable
    {
        Icon _iconRoot;
        TreeGridItem _treeBase, _treeRoot;
        PropertyGridControl _propertyGridControl;

        public ProjectControl()
        {
            InitializeComponent();

            _iconRoot = Icon.FromResource("TreeView.Root.png");
            
            treeView1.Columns.Add(new GridColumn { DataCell = new ImageTextCell(0, 1), Editable = false });
            treeView1.DataStore = _treeBase = new TreeGridItem();
        }

        public void Init(ContextMenu menu, PropertyGridControl propertyGridControl)
        {
            treeView1.ContextMenu = menu;
            _propertyGridControl = propertyGridControl;
        }

        public bool GetSelectedItem(out IProjectItem item)
        {
            var ret = treeView1.SelectedItem != null;

            if (ret)
                item = (treeView1.SelectedItem as TreeGridItem).Tag as IProjectItem;
            else
                item = new DirectoryItem("", "");

            return ret;
        }

        public bool GetSelectedItems(out IProjectItem[] items)
        {
            IProjectItem item;
            var ret = GetSelectedItem(out item);

            items = new[] { item };
            return ret;
        }

        private void TreeView1_SelectionChanged(object sender, EventArgs e)
        {
            MainWindow.Controller.SelectionChanged();
        }

        public TreeGridItem GetRoot()
        {
            if (_treeBase.Children.Count == 0)
            {
                _treeRoot = new TreeGridItem { Expanded = true };
                _treeRoot.Values = new object[] { _iconRoot, "" };

                _treeBase.Children.Add(_treeRoot);
            }

            return _treeRoot;
        }

        public void SetRoot(string name, IProjectItem item)
        {
            if (name == "")
            {
                treeView1.DataStore = _treeBase = new TreeGridItem();
                return;
            }

            var root = GetRoot();
            root.SetValue(1, name);
            root.Tag = item;
        }

        public void UpdateTree()
        {
            treeView1.DataStore = _treeBase;
        }

        public TreeGridItem GetorAddItem(TreeGridItem root, IProjectItem item)
        {
            var enumerator = root.Children.GetEnumerator();
            var folder = item is DirectoryItem;

            var items = new List<string>();
            int pos = 0;

            while (enumerator.MoveNext())
            {
                var citem = enumerator.Current as TreeGridItem;
                var itemtext = citem.GetValue(1).ToString();

                if (itemtext == item.Name)
                    return citem;

                if (folder)
                {
                    if (citem.Tag is DirectoryItem)
                        items.Add(itemtext);
                }
                else
                {
                    if (citem.Tag is DirectoryItem)
                        pos++;
                    else
                        items.Add(itemtext);
                }
            }

            items.Add(item.Name);
            items.Sort();
            pos += items.IndexOf(item.Name);

            Image icon;
            if (item is DirectoryItem)
                icon = Global.GetDirectoryIcon(item.Exists);
            else
                icon = Global.GetFileIcon(MainWindow.Controller.GetFullPath(item.OriginalPath), item.Exists);

            var ret = new TreeGridItem();
            ret.Values = new object[] { icon, item.Name };
            ret.Tag = item;
            root.Children.Insert(pos, ret);

            return ret;
        }

        public void Add(TreeGridItem root, IProjectItem citem)
        {
            Add(root, citem, citem.OriginalPath, "");
        }

        public void Add(TreeGridItem root, IProjectItem citem, string path, string currentPath)
        {
            var split = path.Split('/');
            var item = GetorAddItem(root, split.Length > 1 ? new DirectoryItem(split[0], currentPath) { Exists = citem.Exists } : citem);

            if (path.Contains("/"))
                Add(item, citem, string.Join("/", split, 1, split.Length - 1), (currentPath + Path.DirectorySeparatorChar + split[0]).TrimStart(Path.DirectorySeparatorChar));
        }
    }
}
