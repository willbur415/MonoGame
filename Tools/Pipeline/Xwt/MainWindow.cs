using System;
using Xwt;

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow : Window, IView
    {
        IController _controller;

        FileDialogFilter MonoGameContentProjectFileFilter, XnaContentProjectFileFilter, AllFilesFilter;

        public MainWindow()
        {
            Build();

            MonoGameContentProjectFileFilter = new FileDialogFilter("MonoGame Content Build Projects (*.mgcb)", "*.mgcb");
            XnaContentProjectFileFilter = new FileDialogFilter("XNA Content Projects (*.contentproj)", "*.contentproj");
            AllFilesFilter = new FileDialogFilter("All Files (*.*)", "*.*");

            MessageDialog.RootWindow = this;

            this.CloseRequested += (sender, args) => args.AllowClose = _controller.Exit();
            this.Closed += OnWindowClosed;
        }

        #region IView implementation

        public void Attach(IController controller)
        {
            _controller = controller;
        }

        public AskResult AskSaveOrCancel()
        {
            var no = new Command(Command.No.Id, "No");
            var cancel = new Command(Command.Cancel.Id, "Cancel");
            var save = new Command(Command.Yes.Id, "Save");

            var c = MessageDialog.AskQuestion("Save", "Do you want to save the project first?", new[] { save, no, cancel });

            if (c.Id == save.Id)
                return AskResult.Yes;
            else if (c.Id == cancel.Id)
                return AskResult.No;

            return AskResult.Cancel;
        }

        public bool AskSaveName(ref string filePath, string title)
        {
            var sfdialog = new SaveFileDialog(title);

            sfdialog.Filters.Add(MonoGameContentProjectFileFilter);
            sfdialog.Filters.Add(AllFilesFilter);

            if (sfdialog.Run(this))
            {
                filePath = sfdialog.FileName;

                if (sfdialog.ActiveFilter == MonoGameContentProjectFileFilter && !filePath.EndsWith(".mgcb"))
                    filePath += ".mgcb";

                return true;
            }

            return false;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            var ofdialog = new OpenFileDialog("Open MGCB Project");

            ofdialog.Filters.Add(MonoGameContentProjectFileFilter);
            ofdialog.Filters.Add(AllFilesFilter);

            if (ofdialog.Run(this))
            {
                projectFilePath = ofdialog.FileName;
                return true;
            }

            projectFilePath = "";
            return false;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var ofdialog = new OpenFileDialog("Import XNA Content Project");

            ofdialog.Filters.Add(XnaContentProjectFileFilter);
            ofdialog.Filters.Add(AllFilesFilter);

            if (ofdialog.Run(this))
            {
                projectFilePath = ofdialog.FileName;
                return true;
            }

            projectFilePath = "";
            return false;
        }

        public void ShowError(string title, string message)
        {
            MessageDialog.ShowError(message);
        }

        public void ShowMessage(string message)
        {
            MessageDialog.ShowMessage(message);
        }

        public void BeginTreeUpdate()
        {
            
        }

        public void SetTreeRoot(IProjectItem item)
        {
            if (item != null) 
                projectView1.SetRoot (System.IO.Path.GetFileNameWithoutExtension (item.OriginalPath));
            else 
                projectView1.Close ();
        }

        public void AddTreeItem(IProjectItem item)
        {
            projectView1.AddItem(projectView1.GetRoot(), item.OriginalPath, true, false, item.OriginalPath);
        }

        public void AddTreeFolder(string folder)
        {
            
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
            
        }

        public void OutputClear()
        {
            
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

        public System.Diagnostics.Process CreateProcess(string exe, string commands)
        {
            throw new NotImplementedException();
        }

        public void ItemExistanceChanged(IProjectItem item)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected void OnWindowClosed(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

