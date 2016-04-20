// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class PropertyGridTable : Scrollable
    {
        Drawable drawable;

        private void InitializeComponent()
        {
            drawable = new Drawable();

            Content = drawable;

            drawable.MouseMove += Drawable_MouseMove;
            drawable.MouseLeave += Drawable_MouseLeave;
            drawable.MouseDoubleClick += Drawable_MouseDoubleClick;
            drawable.Paint += Drawable_Paint;
        }
    }
}

