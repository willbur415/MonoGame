using System;
using Xwt;
using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    public class FalseWidget
    {
        public object newvalue;

        public FalseWidget(object newvalue)
        {
            this.newvalue = newvalue;
        }
    }

    public class EventItem
    {
        public int id;
        public EventHandler eventHandler;

        public EventItem(int id, EventHandler eventHandler)
        {
            this.id = id;
            this.eventHandler = eventHandler;
        }
    }

    public class TreeItem
    {
        public string group;

        public string label;
        public object value;
        public PropertyGridTable.EntryType type;
        public EventHandler eventHandler;
        public Dictionary<string, object> comboItems;

        public TreeItem(string label, object value, PropertyGridTable.EntryType type, EventHandler eventHandler = null, Dictionary<string, object> comboItems = null)
        {
            group = "";

            this.label = label;
            this.value = value;
            this.type = type;
            this.eventHandler = eventHandler;
            this.comboItems = comboItems;
        }
    }

    public class PropertyGridTable : Table
    {
        public bool sortgroup;

        public enum EntryType {
            Text,
            Readonly,
            LongText,
            Color,
            Combo,
            Check,
            Integer,
            List,
            FilePath,
            Unkown
        }

        public PropertyGridTable()
        {
            
        }

        public void Initalize(Window window)
        {
            
        }

        public void Refresh()
        {
        }

        public void ClearA() 
        {
            this.Clear();
        }

        int y = 0;

        public void AddEntry(string label, object value, EntryType type, EventHandler eventHandler = null, Dictionary<string, object> comboItems = null) {

            this.Add(new Label(label + ": "), 0, y);

            switch (type)
            {
                case EntryType.Check:
                    break;
                case EntryType.Color:
                    this.Add(new ColorPicker(), 1, y, 1, 1, true);
                    break;
                case EntryType.Combo:
                    break;
                case EntryType.FilePath:
                    break;
                case EntryType.Integer:
                    break;
                case EntryType.List:
                    break;
                case EntryType.LongText:
                    break;
                case EntryType.Readonly:
                    break;
                case EntryType.Text:
                    break;
                case EntryType.Unkown:
                    break;
            }

            y++;
        }

        public void AddProcEntry(string label, object value, EntryType type, EventHandler eventHandler = null, Dictionary<string, object> comboItems = null) {

            this.Add(new Label(label + ": "), 0, y);

            switch (type)
            {
                case EntryType.Check:
                    break;
                case EntryType.Color:
                    ColorPicker cp = new ColorPicker();
                    cp.BackgroundColor = this.BackgroundColor;
                    this.Add(cp, 2, y, 1, 1, true);



                    break;
                case EntryType.Combo:
                    break;
                case EntryType.FilePath:
                    break;
                case EntryType.Integer:
                    break;
                case EntryType.List:
                    break;
                case EntryType.LongText:
                    break;
                case EntryType.Readonly:
                    break;
                case EntryType.Text:
                    break;
                case EntryType.Unkown:
                    break;
            }

            y++;
        }
    }
}

