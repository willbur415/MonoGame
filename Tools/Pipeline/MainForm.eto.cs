// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class MainForm : Form
    {
        Command cmdNew, cmdOpen, cmdOpenRecent, cmdClose, cmdImport, cmdSave, cmdSaveAs, cmdExit;
        Command cmdUndo, cmdRedo, cmdRename, cmdDelete;
        Command cmdNewItem, cmdNewFolder, cmdExistingItem, cmdExistingFolder;
        Command cmdBuild, cmdRebuild, cmdClean, cmdCancelBuild;
        CheckCommand cmdDebugMode, cmdFilterOutput;
        Command cmdHelp, cmdAbout;

        ToolItem toolBuild, toolRebuild, toolClean, toolCancelBuild;

        ProjectControl projectControl;
        PropertyGridControl propertyGridControl;
        BuildOutput buildOutput;

        private void InitializeComponent()
        {
            Title = "MonoGame Pipeline";
            ClientSize = new Size(640, 480);
            MinimumSize = new Size(400, 400);

            InitalizeCommands();
            InitalizeMenu();
            InitalizeToolbar();

            Content = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Panel1 = new Splitter
                {
                    Orientation = Orientation.Vertical,
                    Panel1 = projectControl = new ProjectControl(),
                    Panel2 = propertyGridControl = new PropertyGridControl(),
                    Position = 230
                },
                Panel2 = buildOutput = new BuildOutput(),
                Position = 200
            };

            cmdNew.Executed += CmdNew_Executed;
            cmdOpen.Executed += CmdOpen_Executed;
            cmdClose.Executed += CmdClose_Executed;
            cmdImport.Executed += CmdImport_Executed;
            cmdSave.Executed += CmdSave_Executed;
            cmdSaveAs.Executed += CmdSaveAs_Executed;
            cmdExit.Executed += CmdExit_Executed;

            cmdUndo.Executed += CmdUndo_Executed;
            cmdRedo.Executed += CmdRedo_Executed;
            cmdRename.Executed += CmdRename_Executed;
            cmdDelete.Executed += CmdDelete_Executed;

            cmdNewItem.Executed += CmdNewItem_Executed;
            cmdNewFolder.Executed += CmdNewFolder_Executed;
            cmdExistingItem.Executed += CmdExistingItem_Executed;
            cmdExistingFolder.Executed += CmdExistingFolder_Executed;

            cmdBuild.Executed += CmdBuild_Executed;
            cmdRebuild.Executed += CmdRebuild_Executed;
            cmdClean.Executed += CmdClean_Executed;
            cmdCancelBuild.Executed += CmdCancelBuild_Executed;
            cmdDebugMode.CheckedChanged += CmdDebugMode_Executed;
            cmdFilterOutput.CheckedChanged += CmdFilterOutput_Executed;

            cmdHelp.Executed += CmdHelp_Executed;
            cmdAbout.Executed += CmdAbout_Executed;
        }


        private void InitalizeCommands()
        {
            // File Commands

            cmdNew = new Command
            { 
                MenuText = "New...",
                ToolTip = "New",
                Image = Icon.FromResource("Toolbar.New.png"),
                Shortcut = Application.Instance.CommonModifier | Keys.N
            };

            cmdOpen = new Command
            { 
                MenuText = "Open...",
                ToolTip = "Open",
                Image = Icon.FromResource("Toolbar.Open.png"),
                Shortcut = Application.Instance.CommonModifier | Keys.O
            };

            cmdOpenRecent = new Command
            { 
                MenuText = "Open Recent"
            };

            cmdClose = new Command
            { 
                MenuText = "Close",
                Shortcut = Application.Instance.CommonModifier | Keys.C
            };

            cmdImport = new Command
            { 
                MenuText = "Import" 
            };

            cmdSave = new Command
            { 
                MenuText = "Save...",
                ToolTip = "Save",
                Image = Icon.FromResource("Toolbar.Save.png"),
                Shortcut = Application.Instance.CommonModifier | Keys.S
            };

            cmdSaveAs = new Command
            { 
                MenuText = "Save As"
            };

            cmdExit = new Command
            { 
                MenuText = "Exit",
                Shortcut = Application.Instance.CommonModifier | Keys.Q
            };

            // Edit Commands

            cmdUndo = new Command
            { 
                MenuText = "Undo",
                ToolTip = "Undo",
                Image = Icon.FromResource("Toolbar.Undo.png"),
                Shortcut = Application.Instance.CommonModifier | Keys.Z
            };

            cmdRedo = new Command
            { 
                MenuText = "Redo",
                ToolTip = "Redo",
                Image = Icon.FromResource("Toolbar.Redo.png"),
                Shortcut = Application.Instance.CommonModifier | Keys.Y
            };

            cmdRename = new Command
            {
                MenuText = "Rename"
            };

            cmdDelete = new Command
            {
                MenuText = "Delete",
                Shortcut = Keys.Delete
            };

            // Add Submenu

            cmdNewItem = new Command
            { 
                MenuText = "New Item...",
                ToolTip = "New Item",
                Image = Icon.FromResource("Toolbar.NewItem.png")
            };

            cmdNewFolder = new Command
            { 
                MenuText = "New Folder...",
                ToolTip = "New Folder",
                Image = Icon.FromResource("Toolbar.NewFolder.png")
            };

            cmdExistingItem = new Command
            { 
                MenuText = "Existing Item...",
                ToolTip = "Add Existing Item",
                Image = Icon.FromResource("Toolbar.ExistingItem.png")
            };

            cmdExistingFolder = new Command
            { 
                MenuText = "Existing Folder...",
                ToolTip = "Add Existing Folder",
                Image = Icon.FromResource("Toolbar.ExistingFolder.png")
            };

            // Build Commands

            cmdBuild = new Command
            { 
                MenuText = "Build",
                ToolTip = "Build",
                Image = Icon.FromResource("Toolbar.Build.png"),
                Shortcut = Keys.F6
            };

            cmdRebuild = new Command
            { 
                MenuText = "Rebuild",
                ToolTip = "Rebuild",
                Image = Icon.FromResource("Toolbar.Rebuild.png")
            };

            cmdClean = new Command
            { 
                MenuText = "Clean",
                ToolTip = "Clean",
                Image = Icon.FromResource("Toolbar.Clean.png")
            };

            cmdCancelBuild = new Command
            { 
                MenuText = "Cancel Build",
                ToolTip = "Cancel Build",
                Image = Icon.FromResource("Toolbar.CancelBuild.png")
            };

            cmdDebugMode = new CheckCommand
            {
                MenuText = "Debug Mode"  
            };

            cmdFilterOutput = new CheckCommand
            {
                MenuText = "Filter Output",
                ToolTip = "Filter Output",
                Image = Icon.FromResource("Toolbar.FilterOutput.png")
            };

            // Help Commands

            cmdHelp = new Command
            {
                MenuText = "View Help",
                Shortcut = Keys.F1
            };

            cmdAbout = new Command
            {
                MenuText = "About"
            };
        }

        private void InitalizeMenu()
        {
            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem
                    { 
                        Text = "File", 
                        Items =
                        {
                            cmdNew,
                            cmdOpen,
                            cmdOpenRecent,
                            cmdClose,
                            new SeparatorMenuItem(),
                            cmdImport,
                            new SeparatorMenuItem(),
                            cmdSave,
                            cmdSaveAs
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Edit",
                        Items =
                        {
                            cmdUndo,
                            cmdRedo,
                            new SeparatorMenuItem(),
                            new ButtonMenuItem
                            { 
                                Text = "Add", 
                                Items =
                                {
                                    cmdNewItem,
                                    cmdNewFolder,
                                    new SeparatorMenuItem(),
                                    cmdExistingItem,
                                    cmdExistingFolder
                                }
                            },
                            new SeparatorMenuItem(),
                            cmdRename,
                            cmdDelete
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Build",
                        Items =
                        {
                            cmdBuild,
                            cmdRebuild,
                            cmdClean,
                            cmdCancelBuild,
                            new SeparatorMenuItem(),
                            cmdDebugMode,
                            cmdFilterOutput
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Help",
                        Items =
                        {
                            cmdHelp
                        }
                    }
                },
                QuitItem = cmdExit,
                AboutItem = cmdAbout
            };
        }

        private void InitalizeToolbar()
        {
            toolBuild = cmdBuild.CreateToolItem();
            toolRebuild = cmdRebuild.CreateToolItem();
            toolClean = cmdClean.CreateToolItem();
            toolCancelBuild = cmdCancelBuild.CreateToolItem();

            ToolBar = new ToolBar
            {  
                Style = "toolbar",
                Items =
                { 
                    cmdNew,
                    cmdOpen,
                    cmdSave,
                    new SeparatorToolItem(),
                    cmdUndo,
                    cmdRedo,
                    new SeparatorToolItem(),
                    cmdNewItem,
                    cmdExistingItem,
                    cmdNewFolder,
                    cmdExistingFolder,
                    new SeparatorToolItem(),
                    toolBuild,
                    toolRebuild,
                    toolClean,
                    new SeparatorToolItem(),
                    cmdFilterOutput
                } 
            };
        }
    }
}
