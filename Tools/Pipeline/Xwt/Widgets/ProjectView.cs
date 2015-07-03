using System;
using Xwt;
using Xwt.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public class ProjectView : TreeView
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

        public void AddItem(TreePosition start, string path, bool exists, bool folder, string fullpath)
        {
            int id = (folder || path.Contains("/")) ? ID_FOLDER : ID_FILE;
            Image icon = GetImage(folder || path.Contains("/"));

            string[] split = path.Split ('/');

            TreePosition pos = GetItem(start, split[0]) ?? _treeStore.AddNode(start).SetValue(_idCol, id).SetValue(_imgCol, icon).SetValue(_textCol, split[0]).CurrentPosition;

            if (split.Length > 1) {

                string newpath = split [1];
                for(int i = 2;i < split.Length;i++)
                    newpath += "/" + split[i];

                AddItem (pos, newpath, exists, folder, fullpath);
            }
        }
    }
}

