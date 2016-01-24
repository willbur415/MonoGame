// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;

using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class MainForm : IView
    {
        PipelineController _controller;
        FileDialogFilter _mgcbFileFilter, _allFileFilter, _xnaFileFilter;

        public MainForm()
        {
            InitializeComponent();

            _mgcbFileFilter = new FileDialogFilter("MonoGame Content Build Project (*.mgcb)", new [] { ".mgcb" });
            _allFileFilter = new FileDialogFilter("All Files (*.*)", new [] { ".*" });
            _xnaFileFilter = new FileDialogFilter("XNA Content Projects (*.contentproj)", new [] { ".contentproj" });

            _controller = new PipelineController(this);

            projectControl.Init(propertyGridControl);

            UpdateCommands();
        }

        private string FindSystemMono()
        {
            string[] pathsToCheck =
                {
                "/usr/bin",
                "/usr/local/bin",
                "/Library/Frameworks/Mono.framework/Versions/Current/bin",
            };

            foreach (var path in pathsToCheck)
                if (File.Exists(Path.Combine(path, "mono")))
                    return Path.Combine(path, "mono");
            
            OutputAppend("Cound not find mono. Please install the latest version from http://www.mono-project.com");
            return "mono";
        }

        #region IView implementation

        public void Attach(IController controller)
        {
            controller.OnProjectLoading += () => Application.Instance.Invoke(UpdateCommands);
            controller.OnProjectLoaded += () => Application.Instance.Invoke(UpdateCommands);
            controller.OnBuildStarted += () => Application.Instance.Invoke(UpdateCommands);
            controller.OnBuildFinished += () => Application.Instance.Invoke(UpdateCommands);

            controller.OnCanUndoRedoChanged += (canUndo, canRedo) => UpdateUndoRedo();
        }

        public AskResult AskSaveOrCancel()
        {
            throw new NotImplementedException();
        }

        public bool AskSaveName(ref string filePath, string title)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = title;
            dialog.Filters.Add(_mgcbFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _mgcbFileFilter;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            filePath = dialog.FileName;

            if (result && dialog.CurrentFilter == _mgcbFileFilter && !filePath.EndsWith(".mgcb"))
                filePath += ".mgcb";

            return result;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_mgcbFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _mgcbFileFilter;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            projectFilePath = dialog.FileName;

            return result;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(_xnaFileFilter);
            dialog.Filters.Add(_allFileFilter);
            dialog.CurrentFilter = _xnaFileFilter;

            var result = dialog.ShowDialog(this) == DialogResult.Ok;
            projectFilePath = dialog.FileName;

            return result;
        }

        public void ShowError(string title, string message)
        {
            throw new NotImplementedException();
        }

        public void ShowMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void BeginTreeUpdate()
        {
            
        }

        public void SetTreeRoot(IProjectItem item)
        {
            if(item == null)
                projectControl.SetRoot("");
            else
                projectControl.SetRoot(item.Name);
            
            UpdateCommands();
        }

        public void AddTreeItem(IProjectItem item)
        {
            projectControl.Add(projectControl.GetRoot(), item);
        }

        public void AddTreeFolder(string folder)
        {
            var item = new DirectoryItem(Path.GetFileName(folder), Path.GetDirectoryName(folder));
            item.OriginalPath = folder;

            projectControl.Add(projectControl.GetRoot(), item);
        }

        public void RemoveTreeItem(ContentItem contentItem)
        {
            
        }

        public void RemoveTreeFolder(string folder)
        {
            
        }

        public void UpdateTreeItem(IProjectItem item)
        {
            
        }

        public void EndTreeUpdate()
        {
            
        }

        public void UpdateProperties(IProjectItem item)
        {
            
        }

        public void OutputAppend(string text)
        {
            Application.Instance.Invoke(delegate
                {
                    buildOutput.WriteLine(text);
                    UpdateCommands();
                });
        }

        public void OutputClear()
        {
            Application.Instance.Invoke(delegate
                {
                    buildOutput.ClearOutput();
                    UpdateCommands();
                });
        }

        public bool ChooseContentFile(string initialDirectory, out System.Collections.Generic.List<string> files)
        {
            throw new NotImplementedException();
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            throw new NotImplementedException();
        }

        public bool CopyOrLinkFile(string file, bool exists, out CopyAction action, out bool applyforall)
        {
            throw new NotImplementedException();
        }

        public bool CopyOrLinkFolder(string folder, bool exists, out CopyAction action, out bool applyforall)
        {
            throw new NotImplementedException();
        }

        public void OnTemplateDefined(ContentItemTemplate item)
        {
            
        }

        public Process CreateProcess(string exe, string commands)
        {
            var buildProcess = new Process();

#if WINDOWS
            buildProcess.StartInfo.FileName = exe;
            buildProcess.StartInfo.Arguments = commands;
#endif
#if MONOMAC || LINUX
            buildProcess.StartInfo.FileName = FindSystemMono ();

            if (_controller.LaunchDebugger) 
            {
                var port = Environment.GetEnvironmentVariable("MONO_DEBUGGER_PORT");
                port = !string.IsNullOrEmpty (port) ? port : "55555";
                var monodebugger = string.Format ("--debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:{0}", port);
                buildProcess.StartInfo.Arguments = string.Format("{0} \"{1}\" {2}", monodebugger, exe, commands);

                OutputAppend("************************************************");
                OutputAppend("RUNNING MGCB IN DEBUG MODE!!!");
                OutputAppend(string.Format ("Attach your Debugger to localhost:{0}", port));
                OutputAppend("************************************************");
            } 
            else 
            {
                buildProcess.StartInfo.Arguments = string.Format("\"{0}\" {1}", exe, commands);
            }
#endif

            return buildProcess;
        }

        public void ItemExistanceChanged(IProjectItem item)
        {
            
        }

        #endregion

        #region Commands

        private void CmdNew_Executed (object sender, EventArgs e)
        {
            _controller.NewProject();
        }

        private void CmdOpen_Executed (object sender, EventArgs e)
        {
            _controller.OpenProject();
        }

        private void CmdClose_Executed (object sender, EventArgs e)
        {
            _controller.CloseProject();
        }

        private void CmdImport_Executed (object sender, EventArgs e)
        {
            _controller.ImportProject();
        }

        private void CmdSave_Executed (object sender, EventArgs e)
        {
            _controller.SaveProject(false);
        }

        private void CmdSaveAs_Executed (object sender, EventArgs e)
        {
            _controller.SaveProject(true);
        }

        private void CmdExit_Executed (object sender, EventArgs e)
        {
            this.Close();
        }

        private void CmdUndo_Executed (object sender, EventArgs e)
        {
            _controller.Undo();
        }

        private void CmdRedo_Executed (object sender, EventArgs e)
        {
            _controller.Redo();
        }

        private void CmdRename_Executed (object sender, EventArgs e)
        {

        }

        private void CmdDelete_Executed (object sender, EventArgs e)
        {

        }

        private void CmdNewItem_Executed (object sender, EventArgs e)
        {

        }

        private void CmdNewFolder_Executed (object sender, EventArgs e)
        {

        }

        private void CmdExistingItem_Executed (object sender, EventArgs e)
        {

        }

        private void CmdExistingFolder_Executed (object sender, EventArgs e)
        {

        }

        private void CmdBuild_Executed (object sender, EventArgs e)
        {
            _controller.Build(false);
        }

        private void CmdRebuild_Executed (object sender, EventArgs e)
        {
            _controller.Build(true);
        }

        private void CmdClean_Executed (object sender, EventArgs e)
        {
            _controller.Clean();
        }

        private void CmdCancelBuild_Executed (object sender, EventArgs e)
        {
            _controller.CancelBuild();
        }

        private void CmdDebugMode_Executed (object sender, EventArgs e)
        {
            _controller.LaunchDebugger = cmdDebugMode.Checked;
        }

        private void CmdFilterOutput_Executed (object sender, EventArgs e)
        {

        }

        private void CmdHelp_Executed (object sender, EventArgs e)
        {

        }

        private void CmdAbout_Executed (object sender, EventArgs e)
        {
            
        }

        #endregion

        private void UpdateCommands()
        {
            var notBuilding = !_controller.ProjectBuilding;
            var projectOpen = _controller.ProjectOpen;
            var projectOpenAndNotBuilding = projectOpen && notBuilding;
            var selectionCount = 0;

            cmdNew.Enabled = notBuilding;
            cmdOpen.Enabled = notBuilding;
            cmdImport.Enabled = notBuilding;

            cmdSave.Enabled = projectOpenAndNotBuilding && _controller.ProjectDirty;
            cmdSaveAs.Enabled = projectOpenAndNotBuilding;
            cmdClose.Enabled = projectOpenAndNotBuilding;

            cmdExit.Enabled = notBuilding;

            cmdExistingItem.Enabled = projectOpen;
            cmdExistingFolder.Enabled = projectOpen;
            cmdNewItem.Enabled = projectOpen;
            cmdNewFolder.Enabled = projectOpen;

            cmdRename.Enabled = selectionCount == 1;
            cmdDelete.Enabled = selectionCount > 0;

            cmdBuild.Enabled = projectOpenAndNotBuilding;
            cmdRebuild.Enabled = projectOpenAndNotBuilding;
            cmdClean.Enabled = projectOpenAndNotBuilding;
            cmdCancelBuild.Enabled = !notBuilding;

            if (ToolBar != null)
            {
                if (notBuilding && ToolBar.Items.Contains(toolCancelBuild))
                {
                    ToolBar.Items.Remove(toolCancelBuild);

                    ToolBar.Items.Insert(12, toolBuild);
                    ToolBar.Items.Insert(13, toolRebuild);
                    ToolBar.Items.Insert(14, toolClean);
                }
                else if(!notBuilding && ToolBar.Items.Contains(toolBuild))
                {
                    ToolBar.Items.Remove(toolBuild);
                    ToolBar.Items.Remove(toolRebuild);
                    ToolBar.Items.Remove(toolClean);

                    ToolBar.Items.Insert(12, toolCancelBuild);
                }
            }

            UpdateUndoRedo();
        }

        public void UpdateUndoRedo()
        {
            cmdUndo.Enabled = _controller.CanUndo;
            cmdRedo.Enabled = _controller.CanRedo;
        }
    }
}
