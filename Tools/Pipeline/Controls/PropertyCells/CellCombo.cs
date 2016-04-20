// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    class CellCombo : CellBase
    {
        public CellCombo(string category, string name, object value, EventHandler eventHandler) : base(category, name, value, eventHandler)
        {
            if (value is ImporterTypeDescription)
                DisplayValue = (value as ImporterTypeDescription).DisplayName;
            else if (value is ProcessorTypeDescription)
                DisplayValue = (value as ProcessorTypeDescription).DisplayName;
        }

        public override void Edit(Control control)
        {
            var dialog = new DialogBase();
            var combo = new DropDown();

            if (Value is Enum)
            {
                var values = Enum.GetValues(Value.GetType());
                foreach (var v in values)
                {
                    combo.Items.Add(v.ToString());

                    if (v.ToString() == Value.ToString())
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }
            else if (Value is Boolean)
            {
                combo.Items.Add("True");
                combo.Items.Add("False");

                combo.SelectedIndex = (bool)Value ? 0 : 1;
            }
            else if (Value is ImporterTypeDescription)
            {
                foreach (var v in PipelineTypes.Importers)
                {
                    combo.Items.Add(v.DisplayName);

                    if (v.DisplayName == (Value as ImporterTypeDescription).DisplayName)
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }
            else
            {
                foreach (var v in PipelineTypes.Processors)
                {
                    combo.Items.Add(v.DisplayName);

                    if (v.DisplayName == (Value as ProcessorTypeDescription).DisplayName)
                        combo.SelectedIndex = combo.Items.Count - 1;
                }
            }

            dialog.CreateContent(combo);
            if (dialog.Run(control) != DialogResult.Ok || _eventHandler == null)
                return;

            if (Value is Enum)
                _eventHandler(Enum.Parse(Value.GetType(), combo.SelectedValue.ToString()), EventArgs.Empty);
            else if (Value is Boolean)
                _eventHandler(combo.SelectedIndex == 0, EventArgs.Empty);
            else if (Value is ImporterTypeDescription)
                _eventHandler(PipelineTypes.Importers[combo.SelectedIndex], EventArgs.Empty);
            else
                _eventHandler(PipelineTypes.Processors[combo.SelectedIndex], EventArgs.Empty);
        }
    }
}

