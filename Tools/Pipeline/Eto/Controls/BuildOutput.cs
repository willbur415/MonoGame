// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput
    {
        public BuildOutput()
        {
            InitializeComponent();
        }

        public void ClearOutput()
        {
            textArea.Text = "";
        }

        public void WriteLine(string line)
        {
            textArea.Append(line + Environment.NewLine, true);
        }
    }
}

