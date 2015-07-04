using System;
using Xwt;

namespace MonoGame.Tools.Pipeline
{
    public partial class NewTemplateDialog
    {
        VBox vbox1;
        HBox hbox1;
        ListView listView1;
        Label label1;
        TextEntry entry1;

        protected void Build()
        {
            this.Title = "New Item";
            this.Width = 320;
            this.Height = 350;
            this.Padding = 4;

            vbox1 = new VBox();
            vbox1.Spacing = 2;

            listView1 = new ListView();
            vbox1.PackStart(listView1, true);

            hbox1 = new HBox();

            label1 = new Label("Name: ");
            hbox1.PackStart(label1);

            entry1 = new TextEntry();
            hbox1.PackStart(entry1, true);

            vbox1.PackStart(hbox1);

            Content = vbox1;

            this.Buttons.Add(new DialogButton(Command.Ok));
            this.Buttons.Add(new DialogButton(Command.Cancel));
        }
    }
}

