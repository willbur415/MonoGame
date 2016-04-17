// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.GtkSharp.Forms;
using Gdk;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    class BuildOutputHandler : GtkControl<Notebook, BuildOutput, BuildOutput.ICallback>, IBuildOutput
    {
        public bool Filtered
        {
            get
            {
                return Control.CurrentPage == 0;
            }
            set
            {
                Control.CurrentPage = value ? 0 : 1;
            }
        }

        private ScrolledWindow _scrollTree, _scrollText;
        private TreeView _treeView;
        private TextView _textView;
        private Pixbuf _iconClean, _iconFail, _iconProcessing, _iconSkip, _iconStartEnd, _iconSucceed;
        private OutputParser _outputParser;
        private bool _textScroll, _treeScroll;
        private TreeStore _treeStore;
        private TreeIter _lastIter;

        public BuildOutputHandler()
        {
            // GUI

            Control = new Notebook();
            Control.ShowTabs = false;
            Control.ShowBorder = false;

            _scrollText = new ScrolledWindow();
            _treeView = new TreeView();
            _treeView.HeadersVisible = false;
            _scrollText.Add(_treeView);
            Control.AppendPage(_scrollText, new Label());

            _scrollTree = new ScrolledWindow();
            _textView = new TextView();
            _textView.CursorVisible = false;
            _scrollTree.Add(_textView);
            Control.AppendPage(_scrollTree, new Label());

            // Init

            _iconClean = new Pixbuf(null, "Build.Clean.png");
            _iconFail = new Pixbuf(null, "Build.Fail.png");
            _iconProcessing = new Pixbuf(null, "Build.Processing.png");
            _iconSkip = new Pixbuf(null, "Build.Skip.png");
            _iconStartEnd = new Pixbuf(null, "Build.StartEnd.png");
            _iconSucceed = new Pixbuf(null, "Build.Succeed.png");

            _outputParser = new OutputParser();
            _textScroll = true;
            _treeScroll = true;

            var column = new TreeViewColumn();

            var iconCell = new CellRendererPixbuf();
            var textCell = new CellRendererText();

            column.PackStart(iconCell, false);
            column.PackStart(textCell, false);

            _treeView.AppendColumn(column);

            column.AddAttribute(iconCell, "pixbuf", 0);
            column.AddAttribute(textCell, "text", 1);

            _treeStore = new TreeStore(typeof(Gdk.Pixbuf), typeof(string));
            _treeView.Model = _treeStore;

            // Events

            _treeView.ScrollEvent += TreeView_ScrollEvent;
            _treeView.SizeAllocated += TreeView_SizeAllocated;

            _textView.ScrollEvent += TextView_ScrollEvent;
            _textView.SizeAllocated += TextView_SizeAllocated;
        }

        private void TreeView_ScrollEvent(object o, ScrollEventArgs args)
        {
            _treeScroll = false;
        }

        private void TreeView_SizeAllocated(object o, SizeAllocatedArgs args)
        {
            if (_treeScroll)
            {
                var path = new TreePath((_treeView.Model.IterNChildren() - 1).ToString());
                _treeView.ScrollToCell(path, null, false, 0, 0);
            }
        }

        private void TextView_ScrollEvent(object o, ScrollEventArgs args)
        {
            _textScroll = false;
        }

        private void TextView_SizeAllocated(object o, SizeAllocatedArgs args)
        {
            if (_textScroll)
                _textView.ScrollToIter(_textView.Buffer.EndIter, 0, false, 0, 0);
        }

        public void ClearOutput()
        {
            lock (_textView.Buffer)
                _textView.Buffer.Text = "";

            _textScroll = true;
            _treeScroll = true;
            _treeStore.Clear();
        }

        public void WriteLine(string line)
        {
            lock (_textView.Buffer)
            {
                var iter = _textView.Buffer.EndIter;
                _textView.Buffer.Insert(ref iter, line + Environment.NewLine);
            }

            if (string.IsNullOrWhiteSpace(line))
                return;

            _outputParser.Parse(line);
            line = line.TrimEnd(new[] { ' ', '\n', '\r', '\t' });

            switch (_outputParser.State)
            {
                case OutputState.BuildBegin:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.Cleaning:
                    AddItem(_iconClean, "Cleaning " + MainWindow.Controller.GetRelativePath(_outputParser.Filename));
                    _treeStore.AppendValues(_lastIter, null, line);
                    break;
                case OutputState.Skipping:
                    AddItem(_iconSkip, "Skipping " + MainWindow.Controller.GetRelativePath(_outputParser.Filename));
                    _treeStore.AppendValues(_lastIter, null, line);
                    break;
                case OutputState.BuildAsset:
                    AddItem(_iconProcessing, "Building " + MainWindow.Controller.GetRelativePath(_outputParser.Filename));
                    _treeStore.AppendValues(_lastIter, null, line);
                    break;
                case OutputState.BuildError:
                    _treeStore.SetValue(_lastIter, 0, _iconFail);
                    _treeStore.AppendValues(_lastIter, null, _outputParser.ErrorMessage, "");
                    break;
                case OutputState.BuildErrorContinue:
                    _treeStore.AppendValues(_lastIter, null, _outputParser.ErrorMessage, "");
                    break;
                case OutputState.BuildEnd:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.BuildTime:
                    _treeStore.SetValue(_lastIter, 1, _treeStore.GetValue(_lastIter, 1).ToString().TrimEnd(new[] { '.', ' ' }) + ", " + line);
                    break;
            }
        }

        private void AddItem(Pixbuf icon, string text)
        {
            if (!_lastIter.Equals(new TreeIter()))
                if (_treeView.Model.GetValue(_lastIter, 0) == _iconProcessing)
                    _treeView.Model.SetValue(_lastIter, 0, _iconSucceed);

            _lastIter = _treeStore.AppendValues(icon, text);
        }

    }
}
