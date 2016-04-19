// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            var platform = Platform.Detect;
            platform.Add<IProjectControl>(() => new ProjectControlHandler());

            var app = new Application (platform);
            Styles.Load();

            var win = new MainWindow ();
            PipelineController.Create(win);

            /*
            if (Global.UseHeaderBar && Global.App != null)
                Global.App.AddWindow(win);
            
            if (args != null && args.Length > 0)
            {
            	var projectFilePath = string.Join(" ", args);
            	win.OpenProjectPath = projectFilePath;
            }

            var project = Environment.GetEnvironmentVariable("MONOGAME_PIPELINE_PROJECT");
            if (!string.IsNullOrEmpty (project)) {
            	win.OpenProjectPath = project;
            }*/

            app.Run (win);
        }
    }
}
