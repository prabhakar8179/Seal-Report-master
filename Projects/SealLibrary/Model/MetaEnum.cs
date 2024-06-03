﻿//
// Copyright (c) Seal Report (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using DynamicTypeDescriptor;
using System.Drawing.Design;
using System.Xml.Serialization;
using System.Data;
using Seal.Forms;
using Seal.Helpers;
using System.Data.Common;
using RazorEngine;

namespace Seal.Model
{
    /// <summary>
    /// A MetaEnum defines an enumerated list in a MetaData
    /// </summary>
    public class MetaEnum : RootComponent
    {
        #region Editor

        protected override void UpdateEditorAttributes()
        {
            if (_dctd != null)
            {
                //Disable all properties
                foreach (var property in Properties) property.SetIsBrowsable(false);
                //Then enable
                GetProperty("Name").SetIsBrowsable(true);
                GetProperty("IsDynamic").SetIsBrowsable(true);
                GetProperty("IsDbRefresh").SetIsBrowsable(true);
                GetProperty("ValuesPerConnection").SetIsBrowsable(true);
                GetProperty("UsePosition").SetIsBrowsable(true);
                GetProperty("Translate").SetIsBrowsable(true);
                GetProperty("Sql").SetIsBrowsable(true);
                GetProperty("Script").SetIsBrowsable(true);

                GetProperty("SqlDisplay").SetIsBrowsable(true);
                GetProperty("ScriptDisplay").SetIsBrowsable(true);
                GetProperty("FilterChars").SetIsBrowsable(true);
                GetProperty("Message").SetIsBrowsable(true);

                GetProperty("Values").SetIsBrowsable(IsEditable);
                GetProperty("NumberOfValues").SetIsBrowsable(true);

                GetProperty("Information").SetIsBrowsable(true);
                GetProperty("Error").SetIsBrowsable(true);
                GetProperty("HelperRefreshEnum").SetIsBrowsable(true);

                //Read only
                GetProperty("SqlDisplay").SetIsReadOnly(!IsDynamic);
                GetProperty("IsDbRefresh").SetIsReadOnly(!IsDynamic);
                GetProperty("ValuesPerConnection").SetIsReadOnly(IsDbRefresh);
                GetProperty("NumberOfValues").SetIsReadOnly(true);

                GetProperty("FilterChars").SetIsReadOnly(!HasDynamicDisplay);
                GetProperty("Message").SetIsReadOnly(!HasDynamicDisplay || (FilterChars == 0 && !HasDependencies));

                GetProperty("Information").SetIsReadOnly(true);
                GetProperty("Error").SetIsReadOnly(true);
                GetProperty("HelperRefreshEnum").SetIsReadOnly(true);

                TypeDescriptor.Refresh(this);
            }
        }
        #endregion

        /// <summary>
        /// Create a basic enumerated list
        /// </summary>
        /// <returns></returns>
        public static MetaEnum Create()
        {
            return new MetaEnum() { GUID = Guid.NewGuid().ToString() };
        }

        /// <summary>
        /// Name of the enumerated list
        /// </summary>
        [DefaultValue(null)]
        [Category("Definition"), DisplayName("Name"), Description("Name of the enumerated list."), Id(1, 1)]
        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private bool _isDynamic = false;
        /// <summary>
        /// If True, the list is loaded using the 'SQL Select Statement' and/or the 'Script' defined.
        /// </summary>
        [DefaultValue(false)]
        [Category("Definition"), DisplayName("List is dynamically loaded from the 'SQL Select Statement' or from the 'Load Script'"), Description("If True, the list is loaded using the 'SQL Select Statement' and/or the 'Script' defined."), Id(2, 1)]
        public bool IsDynamic
        {
            get { return _isDynamic; }
            set
            {
                _isDynamic = value;
                UpdateEditorAttributes();
            }
        }

        private bool _isDbRefresh = false;
        /// <summary>
        /// If True, the list is loaded before a report execution. Should be set to False if the SQL or the Load Script has poor performances.
        /// </summary>
        [DefaultValue(false)]
        [Category("Definition"), DisplayName("List is refreshed before the report execution"), Description("If True, the list is loaded before a report execution. Should be set to False if the SQL or the Load Script has poor performances."), Id(3, 1)]
        public bool IsDbRefresh
        {
            get { return _isDbRefresh; }
            set
            {
                _isDbRefresh = value;
                UpdateEditorAttributes();
            }
        }

        /// <summary>
        /// If True, the enum loads and stores the values for each connection.
        /// </summary>
        [DefaultValue(false)]
        [Category("Definition"), DisplayName("List values depend on the connection"), Description("If True, the enum loads and stores the values for each connection."), Id(4, 1)]
        public bool ValuesPerConnection { get; set; } = false;

        /// <summary>
        /// True if the list has dynamic display
        /// </summary>
        [XmlIgnore]
        public bool HasDynamicDisplay
        {
            get { return IsDynamic && IsDbRefresh; }
        }

        /// <summary>
        /// True if the list need to request the server on popup 
        /// </summary>
        [XmlIgnore]
        public bool RequestServerOnPopup
        {
            get { return IsDynamic && IsDbRefresh && !(FilterChars == 0 && !HasDependencies); }
        }

        private string _sql;

        /// <summary>
        /// If the list is dynamic, SQL Select statement with 1, 2, 3, 4 or 5 columns used to build the list of values. The first column is used for the identifier, the second optional column is the display value shown in the table result, the third optional column is the display value shown in the restriction list, the fourth optional column defines a custom CSS Style applied to the result cell, the fifth optional column defines a custom CSS Class applied to the result cell.
        /// </summary>
        [Category("Definition"), DisplayName("SQL Select Statement"), Description("If the list is dynamic, SQL Select statement with 1, 2, 3, 4 or 5 columns used to build the list of values. The first column is used for the identifier, the second optional column is the display value shown in the table result, the third optional column is the display value shown in the restriction list, the fourth optional column defines a custom CSS Style applied to the result cell, the fifth optional column defines a custom CSS Class applied to the result cell."), Id(5, 1)]
        [Editor(typeof(SQLEditor), typeof(UITypeEditor))]
        public string Sql
        {
            get { return _sql; }
            set
            {
                _sql = value;
                if (IsDynamic && IsDbRefresh && !string.IsNullOrEmpty(_sql) && _dctd != null) RefreshEnum();
            }
        }

        private string _script;
        /// <summary>
        /// If the list is dynamic, Razor Script executed to load or update the enumerated list values. The Script is executed after the optional SQL load when 'SQL Select Statement' is not empty.
        /// </summary>
        [Category("Definition"), DisplayName("Script"), Description("If the list is dynamic, Razor Script executed to load or update the enumerated list values. The Script is executed after the optional SQL load when 'SQL Select Statement' is not empty."), Id(6, 1)]
        [Editor(typeof(TemplateTextEditor), typeof(UITypeEditor))]
        public string Script
        {
            get { return _script; }
            set
            {
                _script = value;
                if (IsDynamic && IsDbRefresh && !string.IsNullOrEmpty(_script) && _dctd != null) RefreshEnum();
            }
        }

        /// <summary>
        /// If True, the current position of the values in the list is used to sort the column in the report result
        /// </summary>
        [DefaultValue(false)]
        [Category("Definition"), DisplayName("Use defined position to sort in reports"), Description("If True, the current position of the values in the list is used to sort the column in the report result."), Id(7, 1)]
        public bool UsePosition { get; set; } = false;

        /// <summary>
        /// If True, the enumerated values are translated using the Repository translations
        /// </summary>
        [DefaultValue(false)]
        [Category("Definition"), DisplayName("Translate values"), Description("If True, the enumerated values are translated using the Repository translations."), Id(8, 1)]
        public bool Translate { get; set; } = false;

        /// <summary>
        /// If the list is dynamic, refreshed before execution and the SQL for prompted restriction contains the '{EnumFilter}' keyword, the number of characters typed by the used in the filter box before the enum is built and displayed
        /// </summary>
        [DefaultValue(0)]
        [Category("Dynamic display"), DisplayName("Filter characters to type"), Description("If the list is dynamic, refreshed before execution and the SQL for prompted restriction contains the '{EnumFilter}' keyword, the number of characters typed by the used in the filter box before the enum is built and displayed."), Id(1, 2)]
        public int FilterChars { get; set; } = 0;
        public bool ShouldSerializeFilterChars() { return FilterChars > 0; }

        /// <summary>
        /// If the list is dynamic, refreshed before execution and has filter characters or dependencies, the message displayed to the end user to trigger the list (e.g. 'Select a country first' or 'Type 5 characters').
        /// </summary>
        [Category("Dynamic display"), DisplayName("Information message"), Description("If the list is dynamic, refreshed before execution and has filter characters or dependencies, the message displayed to the end user to trigger the list (e.g. 'Select a country first' or 'Type 5 characters')."), Id(2, 2)]
        public string Message { get; set; }

        /// <summary>
        /// Optional SQL Select Statement used to build the values displayed in a prompted restriction. The SQL is used only if the list is dynamic, refreshed before report execution.
        /// </summary>
        [Category("Dynamic display"), DisplayName("SQL Select Statement for prompted restriction"), Description("Optional SQL Select Statement used to build the values displayed in a prompted restriction. The SQL can return 1 to 5 columns and follows the definition of 'SQL Select Statement' property. It can contain either the '{EnumFilter}' and/or '{EnumValues_<Name>}' keywords where <Name> is the name of another prompted enumerated list. The SQL is used only if the list is dynamic, refreshed before report execution."), Id(3, 2)]
        [Editor(typeof(SQLEditor), typeof(UITypeEditor))]
        public string SqlDisplay { get; set; }

        /// <summary>
        /// Optional Script used to build the values displayed in a prompted restriction. The Script is used only if the list is dynamic, refreshed before report execution.
        /// </summary>
        [Category("Dynamic display"), DisplayName("Script for prompted restriction"), Description("Optional Script used to build the values displayed in a prompted restriction. The Script is used only if the list is dynamic, refreshed before report execution."), Id(4, 2)]
        [Editor(typeof(TemplateTextEditor), typeof(UITypeEditor))]
        public string ScriptDisplay { get; set; }

        /// <summary>
        /// True if the list has dependencies to other list
        /// </summary>
        [XmlIgnore]
        public bool HasDependencies
        {
            get
            {
                return
                  (!string.IsNullOrEmpty(SqlDisplay) && SqlDisplay.Contains(Repository.EnumValuesKeyword)) ||
                  (!string.IsNullOrEmpty(ScriptDisplay) && ScriptDisplay.Contains(Repository.EnumValuesKeyword))
                  ;
            }
        }

        /// <summary>
        /// The list of values used for this enumerated list
        /// </summary>
        [DefaultValue(null)]
        [Category("Values"), DisplayName("Values"), Description("The list of values used for this enumerated list"), Id(1, 3)]
        [Editor(typeof(EntityCollectionEditor), typeof(UITypeEditor))]
        public List<MetaEV> Values { get; set; } = new List<MetaEV>();
        public bool ShouldSerializeValues() { return !HasDynamicDisplay && Values.Count > 0; }


        public List<MetaEV> GetValues(MetaConnection connection)
        {
            if (!ValuesPerConnection || connection == null) return Values;
            return Values.Where(i => i.ConnectionGUID == connection.GUID).ToList();
        }

        /// <summary>
        /// New enum values set by the dynamic display Script
        /// </summary>
        [XmlIgnore]
        public List<MetaEV> NewValues = new List<MetaEV>();

        /// <summary>
        /// The number of values in the collection
        /// </summary>
        [Category("Values"), DisplayName("Number of Values"), Description("The number of values in the collection"), Id(2, 3)]
        public int NumberOfValues
        {
            get { return Values.Count; }
        }

        /// <summary>
        /// True if the list is editable
        /// </summary>
        [XmlIgnore]
        public bool IsEditable = true;

        protected MetaSource _source;
        /// <summary>
        /// Current MetaSource
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public MetaSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
            }
        }

        /// <summary>
        /// True if the source is a standard SQL source
        /// </summary>
        [XmlIgnore]
        public bool IsSQL
        {
            get { return !_source.IsNoSQL; }
        }


        List<MetaEV> getValues(DbConnection connection, string sql)
        {
            var result = new List<MetaEV>();

            if (!string.IsNullOrEmpty(sql))
            {
                DataTable table = Helper.GetDataTable(connection, sql);
                if (table.Columns.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (!row.IsNull(0)) 
                        {
                            MetaEV value = new MetaEV();
                            value.Id = row[0].ToString();
                            value.Val = table.Columns.Count > 1 ? (row.IsNull(1) ? null : row[1].ToString()) : null;
                            value.ValR = table.Columns.Count > 2 ? (row.IsNull(2) ? null : row[2].ToString()) : null;
                            value.Css = table.Columns.Count > 3 ? (row.IsNull(3) ? null : row[3].ToString()) : null;
                            value.Class = table.Columns.Count > 4 ? (row.IsNull(4) ? null : row[4].ToString()) : null;
                            result.Add(value);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the list of the enum values to display after applying filter and depencies
        /// </summary>
        public List<MetaEV> GetSubSetValues(string filter, Dictionary<MetaEnum, string> dependencies)
        {
            var result = new List<MetaEV>();
            if (!string.IsNullOrEmpty(SqlDisplay))
            {
                DbConnection connection = _source.GetOpenConnection();
                var finalSQL = RazorHelper.CompileExecute(SqlDisplay, this);
                if (HasDynamicDisplay) finalSQL = finalSQL.Replace(Repository.EnumFilterKeyword + "}", filter);
                if (HasDynamicDisplay && dependencies != null)
                {
                    foreach (var d in dependencies.Keys)
                    {
                        finalSQL = finalSQL.Replace(Repository.EnumValuesKeyword + d.Name + "}", dependencies[d]);
                    }
                }
                finalSQL = Helper.ClearAllSQLKeywords(finalSQL);
                result = getValues(connection, finalSQL);
            }

            if (!string.IsNullOrEmpty(ScriptDisplay))
            {
                var finalScript = ScriptDisplay;
                if (HasDynamicDisplay) finalScript = finalScript.Replace(Repository.EnumFilterKeyword + "}", Helper.QuoteDouble(filter));
                if (HasDynamicDisplay && dependencies != null)
                {
                    foreach (var d in dependencies.Keys)
                    {
                        finalScript = finalScript.Replace(Repository.EnumValuesKeyword + d.Name + "}", dependencies[d]);
                    }
                }
                finalScript = Helper.ClearAllLINQKeywords(finalScript);
                RazorHelper.CompileExecute(finalScript, this);
                result = NewValues.ToList();
            }

            return result;
        }

        /// <summary>
        /// Refresh the values of the enumerated list
        /// </summary>
        /// <param name="checkOnly"></param>
        public void RefreshEnum(bool checkOnly = false)
        {
            if (_source == null || !IsDynamic || (string.IsNullOrEmpty(Sql) && string.IsNullOrEmpty(Script))) return;

            DbConnection connection = null;
            var initialValues = Values.ToList();
            try
            {
                Error = "";
                Information = "";

                if (!string.IsNullOrEmpty(Sql))
                {
                    if (ValuesPerConnection)
                    {
                        Values.Clear();
                        foreach (var metaConnection in _source.Connections)
                        {
                            connection = metaConnection.GetOpenConnection();
                            var vals = getValues(connection, RazorHelper.CompileExecute(Sql, this));
                            foreach (var ev in vals) ev.ConnectionGUID = metaConnection.GUID;
                            Values.AddRange(vals);
                        }
                    }
                    else
                    {
                        connection = _source.GetOpenConnection();
                        Values = getValues(connection, RazorHelper.CompileExecute(Sql, this));
                    }
                }

                if (!string.IsNullOrEmpty(Script))
                {
                    RazorHelper.CompileExecute(Script, this);
                }
                Information = string.Format("List refreshed with {0} value(s).", Values.Count);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Information = "Error got when refreshing the values.";
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (checkOnly) Values = initialValues;
            }
            Information = Helper.FormatMessage(Information);
            UpdateEditorAttributes();
        }

        /// <summary>
        /// Return the display value from the identifer
        /// </summary>
        public string GetDisplayValue(string id, MetaConnection connection, bool forRestriction = false)
        {
            var values = ValuesPerConnection ? GetValues(connection) : Values;
            MetaEV value = values.FirstOrDefault(i => i.Id == id);
            return value == null ? id : (forRestriction ? value.DisplayRestriction : value.DisplayValue);
        }

        #region Helpers
        /// <summary>
        /// Editor Helper: Refresh values of this list using the SQL Statement
        /// </summary>
        [Category("Helpers"), DisplayName("Refresh values"), Description("Refresh values of this list using the SQL Statement."), Id(1, 10)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperRefreshEnum
        {
            get { return "<Click to refresh enum values>"; }
        }

        /// <summary>
        /// Last information message when the enum list has been refreshed
        /// </summary>
        [XmlIgnore, Category("Helpers"), DisplayName("Information"), Description("Last information message when the enum list has been refreshed."), Id(2, 10)]
        [EditorAttribute(typeof(InformationUITypeEditor), typeof(UITypeEditor))]
        public string Information { get; set; }

        /// <summary>
        /// Last error message
        /// </summary>
        [XmlIgnore, Category("Helpers"), DisplayName("Error"), Description("Last error message."), Id(3, 10)]
        [EditorAttribute(typeof(ErrorUITypeEditor), typeof(UITypeEditor))]
        public string Error { get; set; }


        #endregion


    }
}
