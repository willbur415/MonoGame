using Xwt;
using Xwt.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class MainWindow
    {
        MenuItem menuFile, menuEdit, menuBuild, menuHelp;
        MenuItem miNew, miOpen, miOpenRecent, miClose, miImport, miSave, miSaveAs, miExit;
        MenuItem miUndo, miRedo, miAdd, miNewItem, miNewFolder, miExistingItem, miExistingFolder, miRename, miDelete;
        MenuItem miBuild, miRebuild, miClean;
        CheckBoxMenuItem miDebug;
        MenuItem miHelp, miAbout;

        VPaned vpanned1;
        HPaned hpanned1;
        RichTextView outputTextView1;
        ProjectView projectView1;
        PropertyGrid propertyGrid1;

        private void Build()
        {
            this.Title = "MonoGame Pipeline";
            this.Padding = 0;
            this.Icon = Image.FromResource("MonoGame.Tools.Pipeline.App.ico");
            this.Width = 800;
            this.Height = 600;

            BuildMenu();

            hpanned1 = new HPaned();

            vpanned1 = new VPaned();
            hpanned1.Panel1.Content = vpanned1;

            projectView1 = new ProjectView();
            projectView1.WidthRequest = 200;
            vpanned1.Panel1.Content = projectView1;

            propertyGrid1 = new PropertyGrid();
            vpanned1.Panel2.Content = propertyGrid1;

            outputTextView1 = new RichTextView();
            hpanned1.Panel2.Content = outputTextView1;
            hpanned1.Panel2.Resize = true;

            this.Content = hpanned1;
        }

        private void BuildMenu()
        {
            Menu menu = new Menu();

            menuFile = new MenuItem("File");
            menuFile.SubMenu = new Menu();
            menu.Items.Add(menuFile);

            miNew = new MenuItem("New...");
            menuFile.SubMenu.Items.Add(miNew);

            miOpen = new MenuItem("Open...");
            menuFile.SubMenu.Items.Add(miOpen);

            miOpenRecent = new MenuItem("Open Recent");
            miOpenRecent.SubMenu = new Menu();
            menuFile.SubMenu.Items.Add(miOpenRecent);

            miClose = new MenuItem("Close");
            menuFile.SubMenu.Items.Add(miClose);

            menuFile.SubMenu.Items.Add(new SeparatorMenuItem());

            miImport = new MenuItem("Import...");
            menuFile.SubMenu.Items.Add(miImport);

            menuFile.SubMenu.Items.Add(new SeparatorMenuItem());

            miSave = new MenuItem("Save");
            menuFile.SubMenu.Items.Add(miSave);

            miSaveAs = new MenuItem("Save As...");
            menuFile.SubMenu.Items.Add(miSaveAs);

            menuFile.SubMenu.Items.Add(new SeparatorMenuItem());

            miExit = new MenuItem("Exit");
            menuFile.SubMenu.Items.Add(miExit);

            menuEdit = new MenuItem("Edit");
            menuEdit.SubMenu = new Menu();
            menu.Items.Add(menuEdit);

            miUndo = new MenuItem("Undo");
            menuEdit.SubMenu.Items.Add(miUndo);

            miRedo = new MenuItem("Redo");
            menuEdit.SubMenu.Items.Add(miRedo);

            menuEdit.SubMenu.Items.Add(new SeparatorMenuItem());

            miAdd = new MenuItem("Add");
            miAdd.SubMenu = new Menu();
            menuEdit.SubMenu.Items.Add(miAdd);

            miNewItem = new MenuItem("New Item...");
            miAdd.SubMenu.Items.Add(miNewItem);

            miNewFolder = new MenuItem("New Folder...");
            miAdd.SubMenu.Items.Add(miNewFolder);

            miAdd.SubMenu.Items.Add(new SeparatorMenuItem());

            miExistingItem = new MenuItem("Existing Item...");
            miAdd.SubMenu.Items.Add(miExistingItem);

            miExistingFolder = new MenuItem("Existing Folder...");
            miAdd.SubMenu.Items.Add(miExistingFolder);

            menuEdit.SubMenu.Items.Add(new SeparatorMenuItem());

            miRename = new MenuItem("Rename");
            menuEdit.SubMenu.Items.Add(miRename);

            miDelete = new MenuItem("Delete");
            menuEdit.SubMenu.Items.Add(miDelete);

            menuBuild = new MenuItem("Build");
            menuBuild.SubMenu = new Menu();
            menu.Items.Add(menuBuild);

            miBuild = new MenuItem("Build");
            menuBuild.SubMenu.Items.Add(miBuild);

            miRebuild = new MenuItem("Rebuild");
            menuBuild.SubMenu.Items.Add(miRebuild);

            miClean = new MenuItem("Clean");
            menuBuild.SubMenu.Items.Add(miClean);

            menuBuild.SubMenu.Items.Add(new SeparatorMenuItem());

            miDebug = new CheckBoxMenuItem("Debug Mode");
            menuBuild.SubMenu.Items.Add(miDebug);

            menuHelp = new MenuItem("Help");
            menuHelp.SubMenu = new Menu();
            menu.Items.Add(menuHelp);

            miHelp = new MenuItem("View Help");
            menuHelp.SubMenu.Items.Add(miHelp);

            menuHelp.SubMenu.Items.Add(new SeparatorMenuItem());

            miAbout = new MenuItem("About");
            menuHelp.SubMenu.Items.Add(miAbout);

            this.MainMenu = menu;

            miNew.Clicked += (sender, e) => _controller.NewProject();
            miOpen.Clicked += (sender, e) => _controller.OpenProject();
            miClose.Clicked += (sender, e) => _controller.CloseProject();
            miImport.Clicked += (sender, e) => _controller.ImportProject();
            miSave.Clicked += (sender, e) => Save(false);
            miSaveAs.Clicked += (sender, e) => Save(true);
            miExit.Clicked += (sender, e) => this.Close();

            miNewItem.Clicked += (sender, e) => NewItem();
        }
    }
}

