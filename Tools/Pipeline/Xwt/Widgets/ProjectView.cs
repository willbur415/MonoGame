using System;
using Xwt;
using Xwt.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public class ProjectView : TreeView
    {
        public Image ICON_BASE = Image.FromResource("MonoGame.Tools.Pipeline.Icons.settings.png");

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
            var root = GetRoot();
            _treeStore.GetNavigatorAt(root).SetValue(_textCol, _rootname);
        }

        public TreePosition GetRoot()
        {
            if (_root == null)
                _root = _treeStore.AddNode().SetValue(_imgCol, ICON_BASE).CurrentPosition;

            return _root;
        }

        public void Close()
        {
            this._rootname = "";
            _treeStore.Clear();
        }
    }
}

