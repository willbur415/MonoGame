// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;
using Eto;

namespace MonoGame.Tools.Pipeline
{
    interface IBuildOutput : Control.IHandler
    {
        bool Filtered { get; set; }

        void ClearOutput();

        void WriteLine(string line);
    }

    [Handler(typeof(IBuildOutput))]
    class BuildOutput : Control
    {
        public new IBuildOutput Handler { get { return (IBuildOutput)base.Handler; } }

        public bool Filtered
        {
            get { return Handler.Filtered; }
            set { Handler.Filtered = value; }
        }

        public void ClearOutput()
        {
            Handler.ClearOutput();
        }

        public void WriteLine(string line)
        {
            Handler.WriteLine(line);
        }
    }
}

