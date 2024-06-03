﻿//
// Copyright (c) Seal Report (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Seal.Helpers;
using Seal.Model;

namespace Seal.Forms
{
    public partial class SecurityEditorForm : Form
    {
        SealSecurity _security;

        public SecurityEditorForm(SealSecurity security)
        {
            Visible = false;
            _security = security;

            InitializeComponent();
            toolStripStatusLabel.Image = null;

            security.InitEditor();
            mainPropertyGrid.ToolbarVisible = false;
            mainPropertyGrid.PropertySort = PropertySort.Categorized;
            mainPropertyGrid.LineColor = SystemColors.ControlLight;
            mainPropertyGrid.SelectedObject = security;
            mainPropertyGrid.PropertyValueChanged += mainPropertyGrid_PropertyValueChanged;

            Text = Repository.SealRootProductName + " Security Editor";

            ShowIcon = true;
            Icon = Properties.Resources.serverManager; 
        }

        private void ConfigurationEditorForm_Load(object sender, EventArgs e)
        {

                infoTextBox.Text = @"This editor allows to configure the security used to publish your reports on the Web Server.

Security groups define:
which repository folders are published,
if the user can view reports,
if the user has personal folders,
which columns, data sources, connections or devices can be selected with the Web Report Designer,

The security provider performs the authentication and select the security groups of the user.

Rules applied if a user belongs to several groups:
- Folders: the highest right is used (No right, Execute reports / View files, Execute reports and outputs / View files, Edit schedules / View files, Edit reports / Manage files)
- Personal folder: the highest right is used (No personal folder, Personal folder for files only, Personal folder for reports and files)
- Show folders view: true if one group has this flag set to true
- Tree View: Show all folders: true if one group has this flag set to true
- Folders, Folder Detail and Menu Scripts are executed sequentially sorted by group name

Web Report Designer Security
- SQL Models: True if true in one group
- Devices: Cannot be selected if it is specified in one group
- Sources: Cannot be selected if it is specified in one group
- Connections: Cannot be selected if it is specified in one group
- Columns: Cannot be selected if it is specified in one group

- Default options (Edit Profile, Culture, Logo, Startup) are taken from the group having the highest weight.
";
            
            Visible = true;
        }


        private void cancelToolStripButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void okToolStripButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void mainPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.PropertyDescriptor.Name == "ProviderName" && _security.UseCustomScript && !string.IsNullOrEmpty(_security.Script))
            {
                var result = MessageBox.Show("The custom script for this security provider will not be valid anymore, the 'Use Custom Script' property will be set to false. Do you want to continue ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _security.UseCustomScript = false;
                }
                else
                {
                    _security.ProviderName = e.OldValue.ToString();
                    return;
                }
            }
        }
    }
}
