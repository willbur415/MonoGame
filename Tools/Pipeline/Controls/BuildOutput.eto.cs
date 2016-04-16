// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput : Scrollable
    {
        TextArea textArea;
        TreeGridView treeView;

        private void InitializeComponent()
        {
            textArea = new TextArea();
            textArea.Wrap = false;
            textArea.ReadOnly = true;

            treeView = new TreeGridView();
            treeView.ShowHeader = false;

            Content = textArea;
        }
    }
}

