// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.Wpf.Forms.Controls;
using Eto.Wpf.Forms.Menu;
using Eto.Wpf.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<MenuBarHandler>("Menu", h => h.Control.Background = System.Windows.Media.Brushes.White );
            Style.Add<ToolBarHandler>("ToolBar", h =>
            {
                System.Windows.Controls.ToolBarTray.SetIsLocked(h.Control, true);

                var overflowGrid = h.Control.Template.FindName("OverflowGrid", h.Control) as System.Windows.FrameworkElement;
                if (overflowGrid != null)
                    overflowGrid.Visibility = System.Windows.Visibility.Collapsed;

                var mainPanelBorder = h.Control.Template.FindName("MainPanelBorder", h.Control) as System.Windows.FrameworkElement;
                if (mainPanelBorder != null)
                    mainPanelBorder.Margin = new System.Windows.Thickness(0);

                h.Control.Background = System.Windows.Media.Brushes.White;
            });

            /*Style.Add<LabelHandler>("Wrap", h => h.Control.MaximumSize = new System.Drawing.Size(400, 0));
            Style.Add<GridViewHandler>("GridView", h =>
            {
                h.Control.BackgroundColor = System.Drawing.SystemColors.Window;
                h.Control.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            });
            Style.Add<LinkButtonHandler>("Center", h => h.Control.TextAlign = System.Drawing.ContentAlignment.MiddleCenter);*/
        }
    }
}
