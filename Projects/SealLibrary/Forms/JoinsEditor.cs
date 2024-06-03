﻿//
// Copyright (c) Seal Report (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using Seal.Model;
using System.Linq;

namespace Seal.Forms
{
    public class JoinsEditor : UITypeEditor
    {
        ReportModel _model;

        void setContext(ITypeDescriptorContext context)
        {
            _model = context.Instance as ReportModel;
        }


        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            setContext(context);
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                setContext(context);
                if (_model != null && _model.Source != null)
                {
                    var joins = _model.Source.MetaData.Joins.OrderBy(i => i.Name).ToList();
                    MultipleSelectForm frm = new MultipleSelectForm("Please select the Joins", joins, "Name");
                    //select existing values
                    for (int i = 0; i < frm.checkedListBox.Items.Count; i++)
                    {
                        if (_model.JoinsToUse.Contains(((MetaJoin)frm.checkedListBox.Items[i]).GUID)) frm.checkedListBox.SetItemChecked(i, true); 
                    }

                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        _model.JoinsToUse = new List<string>();
                        foreach (object item in frm.CheckedItems)
                        {
                            _model.JoinsToUse.Add(((MetaJoin) item).GUID);
                        }
                        value = ""; //indicates a modification
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return value;
        }
    }
}
