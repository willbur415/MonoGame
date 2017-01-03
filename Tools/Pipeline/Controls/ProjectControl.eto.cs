// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl : Pad
    {
        public TreeView TreeView;

        private void InitializeComponent()
        {
            Title = "Project";
            TreeView = new TreeView();
            TreeView.DataStore = _treeBase = new TreeItem();
            CreateContent(TreeView);

            TreeView.SelectionChanged += TreeView_SelectedItemChanged;
            TreeView.Expanded += TreeView1_SaveExpanded;
            TreeView.Collapsed += TreeView1_SaveExpanded;
        }
    }
}
