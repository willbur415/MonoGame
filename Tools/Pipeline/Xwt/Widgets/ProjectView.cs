using System;
using Xwt;
using Xwt.Drawing;
using System.IO;
using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    class ProjectView : TreeView
    {
        private Image GetImage(bool folder)
        {
            return (folder) ? Image.FromResource("MonoGame.Tools.Pipeline.Icons.folder_closed.png") : Image.FromResource("MonoGame.Tools.Pipeline.Icons.blueprint.png");
        }

        public Image ICON_BASE = Image.FromResource("MonoGame.Tools.Pipeline.Icons.settings.png");

        public static int ID_BASE = 0, ID_FOLDER = 1, ID_FILE = 2;

        TreePosition _root;
        string _rootname;
        TreeStore _treeStore;
        IController _controller;

        DataField<int> _idCol = new DataField<int>();
        DataField<Image> _imgCol = new DataField<Image>();
        DataField<string> _textCol = new DataField<string>();

        public ProjectView()
        {
            _rootname = "";

            _treeStore = new TreeStore(_idCol, _imgCol, _textCol);
            this.DataSource = _treeStore;

            this.Columns.Add("", _imgCol, _textCol);
            this.HeadersVisible = false;
            this.SelectionMode = SelectionMode.Multiple;
        }

        public void Attach(IController controller)
        {
            this._controller = controller;
        }

        public void SetRoot(string rootname)
        {
            this._rootname = rootname;
            _treeStore.GetNavigatorAt(GetRoot()).SetValue(_textCol, _rootname);
        }

        public TreePosition GetRoot()
        {
            if (_root == null)
                _root = _treeStore.AddNode().SetValue(_idCol, ID_BASE).SetValue(_imgCol, ICON_BASE).CurrentPosition;

            return _root;
        }

        public void Close()
        {
            this._rootname = "";
            _treeStore.Clear();
        }

        public TreePosition GetItem(TreePosition pos, string name)
        {
            var nav = _treeStore.GetNavigatorAt(pos);

            if (nav.MoveToChild())
            {
                do
                {
                    string navname = nav.GetValue(_textCol);

                    if (navname == name)
                        return nav.CurrentPosition;
                }
                while(nav.MoveNext());
            }

            return null;
        }

        public void ExpandPath(TreePosition start, string path)
        {
            this.ExpandRow(start, false);

            string[] split = path.Split ('/');
            TreePosition pos = GetItem(start, split[0]);

            if (pos == null)
                return;

            if (split.Length > 1) {

                string newpath = split [1];
                for(int i = 2;i < split.Length;i++)
                    newpath += "/" + split[i];

                ExpandPath(pos, path);
            }
            else
                this.ExpandRow(pos, false);
        }

        public void AddItem(TreePosition start, string path, bool exists, bool folder, string fullpath)
        {
            int id = (folder || path.Contains("/")) ? ID_FOLDER : ID_FILE;
            Image icon = GetImage(folder || path.Contains("/"));

            string[] split = path.Split ('/');

            TreePosition pos = GetItem(start, split[0]) ?? AddAndSort(start, id, icon, split[0]);

            if (split.Length > 1) {

                string newpath = split [1];
                for(int i = 2;i < split.Length;i++)
                    newpath += "/" + split[i];

                AddItem (pos, newpath, exists, folder, fullpath);
            }
        }

        private TreePosition AddAndSort(TreePosition pos, int id, Image img, string text)
        {
            var nav = _treeStore.GetNavigatorAt(pos);

            if (nav.MoveToChild())
            {
                int count = 0;

                do
                {
                    if (nav.GetValue(_idCol) == id)
                    {
                        count++;

                        if(nav.GetValue(_textCol).CompareTo(text) >= 0)
                            return nav.InsertBefore().SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
                    }
                    else if(nav.GetValue(_idCol) == ID_FILE)
                        return nav.InsertBefore().SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
                }
                while(nav.MoveNext());

                if (count == 0 && id == ID_FOLDER)
                {
                    nav = _treeStore.GetNavigatorAt(pos);
                    nav.MoveToChild();
                    return nav.InsertBefore().SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
                }
            }

            return _treeStore.AddNode(pos).SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
        }

        public void GetInfo(TreePosition pos, out FileType type, out string path, out string loc)
        {
            TreeNavigator nav = _treeStore.GetNavigatorAt(pos);
            path = "";

            do
            {
                path = nav.GetValue(_textCol) + "/" + path;
            }
            while(nav.MoveToParent());
            path = path.Remove(path.Length - 1);

            var id = _treeStore.GetNavigatorAt(pos).GetValue(_idCol);

            if (id == ID_BASE)
            {
                loc = "";
                type = FileType.Base;
            }
            else if (id == ID_FOLDER)
            {
                loc = Path.GetDirectoryName(path);
                type = FileType.Folder;
            }
            else
            {
                loc = path;
                type = FileType.Folder;
            }
        }
    }
}

