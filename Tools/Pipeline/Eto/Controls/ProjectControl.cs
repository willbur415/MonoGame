// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl : Scrollable
    {
        TreeGridItem _treeBase, _treeRoot;
        PropertyGridControl _propertyGridControl;

        Icon _iconRoot;

        public ProjectControl()
        {
            InitializeComponent();

            _iconRoot = Icon.FromResource("Treeview.Root.png");

            treeView1.Columns.Add(new GridColumn { DataCell = new ImageTextCell(0, 1), HeaderText = "Image and Text", AutoSize = true, Resizable = true, Editable = false });
            treeView1.DataStore = _treeBase = new TreeGridItem();
        }

        public void Init(PropertyGridControl propertyGridControl)
        {
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

        private void TreeView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        public TreeGridItem GetRoot()
        {
            if (_treeBase.Children.Count == 0)
            {
                _treeRoot = new TreeGridItem { Expanded = true };
                _treeRoot.Values = new object[] { _iconRoot, "" };

                _treeBase.Children.Add(_treeRoot);
                treeView1.DataStore = _treeBase;
            }

            return _treeRoot;
        }

        public void SetRoot(string name)
        {
            if (name == "")
            {
                treeView1.DataStore = _treeBase = new TreeGridItem();
                return;
            }

            GetRoot().SetValue(1, name);
        }

        public TreeGridItem GetItem(TreeGridItem root, string text, bool folder)
        {
            var enumerator = root.Children.GetEnumerator();

            var items = new List<string>();
            int pos = 0;

            while (enumerator.MoveNext())
            {
                var item = enumerator.Current as TreeGridItem;
                var itemtext = item.GetValue(1).ToString();

                if (itemtext == text)
                    return item;

                var itemFolder = !(item.Tag is ContentItem);

                if (itemFolder)
                {
                    if (folder)
                        items.Add(itemtext);
                    else
                        pos++;
                }
                else if (!folder)
                    items.Add(itemtext);
            }

            items.Add(text);
            items.Sort();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == text)
                {
                    pos += i;
                    break;
                }
            }

            var ret = new TreeGridItem();
            ret.Values = new object[] { null, text };
            ret.Tag = new DirectoryItem(text, root.GetValue(1).ToString());
            root.Children.Insert(pos, ret);
            treeView1.DataStore = _treeBase;

            return ret;
        }

        public void Add(TreeGridItem root, IProjectItem citem)
        {
            Add(root, citem, citem.OriginalPath);
        }

        public void Add(TreeGridItem root, IProjectItem citem, string path)
        {
            var split = path.Split('/');
            var item = GetItem(root, split[0], !(citem is ContentItem));

            if (path.Contains("/"))
            {
                if (!citem.Exists)
                    item.SetValue(0, Global.GetDirectoryIcon(false));
                else if (citem.Exists && item.GetValue(0) == null)
                    item.SetValue(0, Global.GetDirectoryIcon(true));

                Add(item, citem, string.Join("/", split, 1, split.Length - 1));
            }
            else
            {
                if (citem is ContentItem)
                    item.SetValue(0, Global.GetFileIcon(MainWindow.Controller.GetFullPath(citem.OriginalPath), citem.Exists));
                else
                    item.SetValue(0, Global.GetDirectoryIcon(citem.Exists));

                item.Tag = citem;
            }
        }
    }
}
