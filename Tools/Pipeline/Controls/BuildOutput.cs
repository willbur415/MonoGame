// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput
    {
        public bool Filtered
        {
            set
            {
                this.Content = value ? (Control)treeView : textArea;
            }
        }

        private OutputParser _output;
        private TreeGridItem _root, _last;
        private Icon _iconClean, _iconFail, _iconProcessing, _iconSkip, _iconStartEnd, _iconSucceed;

        public BuildOutput()
        {
            InitializeComponent();

            _output = new OutputParser();

            _iconClean = Icon.FromResource("Build.Clean.png");
            _iconFail = Icon.FromResource("Build.Fail.png");
            _iconProcessing = Icon.FromResource("Build.Processing.png");
            _iconSkip = Icon.FromResource("Build.Skip.png");
            _iconStartEnd = Icon.FromResource("Build.StartEnd.png");
            _iconSucceed = Icon.FromResource("Build.Succeed.png");

            treeView.Columns.Add(new GridColumn { DataCell = new ImageTextCell(0, 1), Editable = false });
            _root = new TreeGridItem();
        }

        public void ClearOutput()
        {
            textArea.Text = "";
            _root.Children.Clear();
            treeView.DataStore = _root;
        }

        public void WriteLine(string line)
        {
            textArea.Append(line + Environment.NewLine, true);

            if (string.IsNullOrEmpty(line))
                return;
            
            _output.Parse(line);
            line = line.Trim(new[] { ' ', '\n', '\r', '\t' });

            switch (_output.State)
            {
                case OutputState.BuildBegin:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.Cleaning:
                    AddItem(_iconClean, "Cleaning " + MainWindow.Controller.GetRelativePath(_output.Filename));
                    AddItem(line);
                    break;
                case OutputState.Skipping:
                    AddItem(_iconSkip, "Skipping " + MainWindow.Controller.GetRelativePath(_output.Filename));
                    AddItem(line);
                    break;
                case OutputState.BuildAsset:
                    AddItem(_iconProcessing, "Building " + MainWindow.Controller.GetRelativePath(_output.Filename));
                    AddItem(line);
                    break;
                case OutputState.BuildError:
                    _last.SetValue(0, _iconFail);
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildErrorContinue:
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildEnd:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.BuildTime:
                    _last.SetValue(1, _last.GetValue(1).ToString().TrimEnd(new[] { '.', ' ' }) + ", " + line);
                    break;
            }
        }

        private void AddItem(Icon icon, string text)
        {
            var item = new TreeGridItem();
            item.SetValue(0, icon);
            item.SetValue(1, text);

            if (_last != null && _last.GetValue(0) == _iconProcessing)
                _last.SetValue(0, _iconSucceed);

            _last = item;
            _root.Children.Add(item);

            //treeView.DataStore = _root;
            //treeView.ScrollToRow(_root.Children.Count - 1);
        }

        private void AddItem(string text)
        {
            var item = new TreeGridItem();
            item.SetValue(1, text);

            _last.Children.Add(item);
        }
    }
}

