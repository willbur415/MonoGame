// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    public partial class PropertyGridTable
    {
        public PropertyGridTable()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            
        }

        public void AddEntry(PropertyInfo property, object value)
        {
            Console.WriteLine(property.Name);
        }
    }
}

