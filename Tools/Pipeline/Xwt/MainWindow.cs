using System;
using Xwt;
using System.Collections.Generic;

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

            projectView1.SelectionChanged += (sender, e) => UpdateMenu();
            this.CloseRequested += (sender, args) => args.AllowClose = _controller.Exit();
            this.Closed += OnWindowClosed;
        }

        #region IView implementation

        public void Attach(IController controller)
        {
            _controller = controller;

            _controller.OnProjectLoading += UpdateMenu;
            _controller.OnProjectLoaded += UpdateMenu;
            _controller.OnBuildStarted += UpdateMenu;
            _controller.OnBuildFinished += UpdateMenu;

            _controller.OnCanUndoRedoChanged += UpdateUndoRedo;

            projectView1.Attach(_controller);
            UpdateMenu();
        }

        public void ReloadRecentList(List<string> paths)
        {
            miOpenRecent.SubMenu.Items.Clear();

            foreach (string path in paths)
            {
                var item = new MenuItem(path);
                item.Clicked += (sender, e) => _controller.OpenProject(((MenuItem)sender).Label);
                miOpenRecent.SubMenu.Items.Add(item);
            }

            if (paths.Count > 0)
            {
                var item = new MenuItem("Clear");
                item.Clicked += (sender, e) => _controller.ClearRecentList();
                miOpenRecent.SubMenu.Items.Add(new SeparatorMenuItem());
                miOpenRecent.SubMenu.Items.Add(item);
            }
            else
            {
                var item = new MenuItem("No History");
                item.Sensitive = false;
                miOpenRecent.SubMenu.Items.Add(item);
            }
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

        public void ExpandPath(string path)
        {
            projectView1.ExpandPath(projectView1.GetRoot(), path);
        }

        public void AddTreeItem(IProjectItem item)
        {
            projectView1.AddItem(projectView1.GetRoot(), item.OriginalPath, true, false, item.OriginalPath);
        }

        public void AddTreeFolder(string folder)
        {
            projectView1.AddItem(projectView1.GetRoot(), folder, true, true, folder);
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

        public bool ChooseContentFile(string initialDirectory, out List<string> files)
        {
            throw new NotImplementedException();
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            throw new NotImplementedException();
        }

        public bool ChooseItemTemplate(out ContentItemTemplate template, out string name)
        {
            var dialog = new NewTemplateDialog(_controller.Templates.GetEnumerator ());
            dialog.TransientFor = this;

            Command result = dialog.Run();
            dialog.Close();

            if (result == Command.Ok)
            {
                template = dialog.TemplateFile;
                name = dialog.Name;
                return true;
            }

            template = null;
            name = null;
            return false;
        }

        public bool ChooseName(string title, string text, string oldname, bool docheck, out string newname)
        {
            var dialog = new TextEditDialog(title, text, oldname, docheck);
            dialog.TransientFor = this;

            var result = dialog.Run();
            dialog.Close();

            if (result == Command.Ok)
            {
                newname = dialog.Text;
                return true;
            }

            newname = null;
            return false;
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

        public bool GetSelection(out FileType fileType, out string path, out string location)
        {
            var rows = projectView1.SelectedRows;

            if (rows.Length != 1)
            {
                fileType = FileType.Base;
                path = "";
                location = "";

                return false;
            }

            projectView1.GetInfo(projectView1.SelectedRow, out fileType, out path, out location);
            return true;
        }

        public bool GetSelection(out FileType[] fileType, out string[] path, out string[] location)
        {
            var types = new List<FileType>();
            var paths = new List<string>();
            var locs = new List<string>();

            var rows = projectView1.SelectedRows;

            foreach (var r in rows)
            {
                FileType t;
                string p, l;

                projectView1.GetInfo(r, out t, out p, out l);

                types.Add(t);
                paths.Add(p);
                locs.Add(l);
            }

            fileType = types.ToArray();
            path = paths.ToArray();
            location = locs.ToArray();

            return (rows.Length > 0);
        }

        public List<ContentItem> GetChildItems(string path)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void UpdateMenu()
        {
            var ms = _controller.GetMenuSensitivityInfo();

            miNew.Sensitive = ms.New;
            miOpen.Sensitive = ms.Open;
            miClose.Sensitive = ms.Close;
            miSave.Sensitive = ms.Save;
            miSaveAs.Sensitive = ms.SaveAs;
            miAdd.Sensitive = ms.Add;
            miRename.Sensitive = ms.Rename;
            miDelete.Sensitive = ms.Delete;
            miBuild.Sensitive = ms.Build;
            miRebuild.Sensitive = ms.Rebuild;
            miClean.Sensitive = ms.Clean;
            miDebug.Sensitive = ms.Debug;
            menuBuild.Sensitive = ms.BuildMenu;
            UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);
        }

        void UpdateUndoRedo(bool canUndo, bool canRedo)
        {
            miUndo.Sensitive = canUndo;
            miRedo.Sensitive = canRedo;
        }

        protected void OnWindowClosed(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

