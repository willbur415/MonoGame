// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class ProjectControl : Scrollable
    {
        TreeGridView treeView1;

        private void InitializeComponent()
        {
            treeView1 = new TreeGridView();
            //treeView1.AllowMultipleSelection = true;
            treeView1.ShowHeader = false;

            Content = treeView1;

            treeView1.SelectionChanged += TreeView1_SelectionChanged;
        }
    }
}
