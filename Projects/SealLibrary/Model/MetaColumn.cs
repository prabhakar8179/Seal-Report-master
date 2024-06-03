﻿//
// Copyright (c) Seal Report (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using DynamicTypeDescriptor;
using Seal.Helpers;
using System.Drawing.Design;
using Seal.Forms;

namespace Seal.Model
{
    /// <summary>
    /// A MetaColumn is part of a MetaTable and defines an element that can be selected in a report
    /// </summary>
    public class MetaColumn : RootComponent, ITreeSort
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
                GetProperty("Type").SetIsBrowsable(true);
                GetProperty("IsAggregate").SetIsBrowsable(IsSQL);
                GetProperty("Category").SetIsBrowsable(true);
                GetProperty("Tag").SetIsBrowsable(true);
                GetProperty("DisplayName").SetIsBrowsable(true);
                GetProperty("DisplayOrder").SetIsBrowsable(true);
                GetProperty("Format").SetIsBrowsable(true);
                GetProperty("EnumGUID").SetIsBrowsable(true);
                GetProperty("DrillChildren").SetIsBrowsable(true);
                GetProperty("DrillChildren").SetDisplayName("Drill Children: " + (DrillChildren.Count == 0 ? "None" : DrillChildren.Count.ToString() + " Items(s)"));
                GetProperty("DrillUpOnlyIfDD").SetIsBrowsable(true);
                GetProperty("SubReports").SetIsBrowsable(true);
                GetProperty("SubReports").SetDisplayName("Sub-Reports: " + (SubReports.Count == 0 ? "None" : SubReports.Count.ToString() + " Items(s)"));

                GetProperty("HelperCreateSubReport").SetIsBrowsable(true);
                GetProperty("HelperAddSubReport").SetIsBrowsable(true);
                GetProperty("HelperOpenSubReportFolder").SetIsBrowsable(true);
                GetProperty("HelperCheckColumn").SetIsBrowsable(IsSQL);
                GetProperty("HelperCreateEnum").SetIsBrowsable(true);
                GetProperty("HelperShowValues").SetIsBrowsable(true);
                GetProperty("HelperCreateDrillDates").SetIsBrowsable(Type == ColumnType.DateTime && (Source.Connection.DatabaseType == DatabaseType.MSAccess || Source.Connection.DatabaseType == DatabaseType.Oracle || Source.Connection.DatabaseType == DatabaseType.MSSQLServer));
                GetProperty("Information").SetIsBrowsable(true);
                GetProperty("Error").SetIsBrowsable(true);

                GetProperty("NumericStandardFormat").SetIsBrowsable(Type == ColumnType.Numeric);
                GetProperty("DateTimeStandardFormat").SetIsBrowsable(Type == ColumnType.DateTime);

                //Read only
                GetProperty("Format").SetIsReadOnly((Type == ColumnType.Numeric && NumericStandardFormat != NumericStandardFormat.Custom) || (Type == ColumnType.DateTime && DateTimeStandardFormat != DateTimeStandardFormat.Custom));

                GetProperty("HelperCreateSubReport").SetIsReadOnly(true);
                GetProperty("HelperAddSubReport").SetIsReadOnly(true);
                GetProperty("HelperOpenSubReportFolder").SetIsReadOnly(true);
                GetProperty("HelperCheckColumn").SetIsReadOnly(true);
                GetProperty("HelperCreateEnum").SetIsReadOnly(true);
                GetProperty("HelperShowValues").SetIsReadOnly(true);
                GetProperty("Information").SetIsReadOnly(true);
                GetProperty("Error").SetIsReadOnly(true);

                GetProperty("Name").SetIsReadOnly(!IsSQL);
                GetProperty("Type").SetIsReadOnly(!IsSQL);

                TypeDescriptor.Refresh(this);
            }
        }

        override public void SetReadOnly()
        {
            base.SetReadOnly();
            GetProperty("HelperCreateEnum").SetIsBrowsable(false);
            GetProperty("HelperCreateSubReport").SetIsBrowsable(false);
            GetProperty("HelperAddSubReport").SetIsBrowsable(false);
            TypeDescriptor.Refresh(this);
        }

        #endregion

        /// <summary>
        /// Create a basic column
        /// </summary>
        public static MetaColumn Create(string name)
        {
            MetaColumn result = new MetaColumn() { Name = name, DisplayName = name, Type = ColumnType.Text, Category = "Default" };
            result.GUID = Guid.NewGuid().ToString();
            return result;
        }

        /// <summary>
        /// The name of the column in the table or the SQL Statement used for the column
        /// </summary>
        [DefaultValue(null)]
        [DisplayName("Name"), Description("The name of the column in the table or the SQL Statement used for the column."), Category("Definition"), Id(1, 1)]
        [Editor(typeof(TemplateTextEditor), typeof(UITypeEditor))]
        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Data type of the column
        /// </summary>
        protected ColumnType _type = ColumnType.Default;
        [DefaultValue(ColumnType.Default)]
        [Category("Definition"), DisplayName("Data type"), Description("Data type of the column."), Id(2, 1)]
        [TypeConverter(typeof(NamedEnumConverterNoDefault))]
        public ColumnType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                UpdateEditorAttributes();
            }
        }

        /// <summary>
        /// Must be True if the column contains SQL aggregate functions like SUM,MIN,MAX,COUNT,AVG
        /// </summary>
        [DefaultValue(false)]
        [Category("Definition"), DisplayName("Is Aggregate"), Description("Must be True if the column contains SQL aggregate functions like SUM,MIN,MAX,COUNT,AVG."), Id(3, 1)]
        public bool IsAggregate { get; set; } = false;
        public bool ShouldSerializeIsAggregate() { return IsAggregate; }

        /// <summary>
        /// Category used to display the column in the Report Designer tree view. Category hierarchy can be defined using the '/' character (e.g. 'Master/Name1/Name2').
        /// </summary>
        [Category("Display"), DisplayName("Category Name"), Description("Category used to display the column in the Report Designer tree view. Category hierarchy can be defined using the '/' character (e.g. 'Master/Name1/Name2')."), Id(2, 2)]
        public string Category { get; set; }

        private string _tag;
        /// <summary>
        /// Tag used to define the security of the Web Report Designer (Columns of the Security Groups defined in the Web Security)
        /// </summary>
        [Category("Security"), DisplayName("Security Tag"), Description("Tag used to define the security of the Web Report Designer (Columns of the Security Groups defined in the Web Security)."), Id(2, 2)]
        public string Tag
        {
            get { return string.IsNullOrEmpty(_tag) ? "" : _tag; }
            set { _tag = value; }
        }
        public bool ShouldSerializeTag() { return !string.IsNullOrEmpty(_tag); }

        protected string _displayName;
        /// <summary>
        /// Name used to display the column in the Report Designer tree view and in the report results
        /// </summary>
        [Category("Display"), DisplayName("Display Name"), Description("Name used to display the column in the Report Designer tree view and in the report results."), Id(3, 2)]
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// The order number used to sort the column in the tree view (by table and by category)
        /// </summary>
        [DefaultValue(1)]
        [Category("Display"), DisplayName("Display order"), Description("The order number used to sort the column in the tree view (by table and by category)."), Id(4, 2)]
        public int DisplayOrder { get; set; } = 1;
        public bool ShouldSerializeDisplayOrder() { return DisplayOrder != 0; }

        /// <summary>
        /// The display order
        /// </summary>
        public int GetSort()
        {
            return DisplayOrder;
        }


        protected NumericStandardFormat _numericStandardFormat = NumericStandardFormat.Default;
        /// <summary>
        /// Standard display format applied to the element
        /// </summary>
        [DefaultValue(NumericStandardFormat.Default)]
        [Category("Options"), DisplayName("Format"), Description("Standard display format applied to the element."), Id(2, 3)]
        [TypeConverter(typeof(NamedEnumConverter))]
        public NumericStandardFormat NumericStandardFormat
        {
            get { return _numericStandardFormat; }
            set
            {
                if (_dctd != null && _numericStandardFormat != value)
                {
                    _numericStandardFormat = value;
                    SetStandardFormat();
                    UpdateEditorAttributes();
                }
                else
                    _numericStandardFormat = value;
            }
        }
        public bool ShouldSerializeNumericStandardFormat() { return _numericStandardFormat != NumericStandardFormat.Default; }

        protected DateTimeStandardFormat _datetimeStandardFormat = DateTimeStandardFormat.Default;
        /// <summary>
        /// Standard display format applied to the element
        /// </summary>
        [DefaultValue(DateTimeStandardFormat.Default)]
        [Category("Options"), DisplayName("Format"), Description("Standard display format applied to the element."), Id(2, 3)]
        [TypeConverter(typeof(NamedEnumConverter))]
        public DateTimeStandardFormat DateTimeStandardFormat
        {
            get { return _datetimeStandardFormat; }
            set
            {
                if (_dctd != null && _datetimeStandardFormat != value)
                {
                    _datetimeStandardFormat = value;
                    SetStandardFormat();
                    UpdateEditorAttributes();
                }
                else
                    _datetimeStandardFormat = value;
            }
        }
        public bool ShouldSerializeDateTimeStandardFormat() { return _datetimeStandardFormat != DateTimeStandardFormat.Default; }

        protected string _format = "";
        /// <summary>
        /// If not empty, specify the format of the elements values displayed in the result tables (.Net Format Strings)
        /// </summary>
        [Category("Options"), DisplayName("Custom format"), Description("If not empty, specify the format of the elements values displayed in the result tables (.Net Format Strings)."), Id(3, 3)]
        [TypeConverter(typeof(CustomFormatConverter))]
        public string Format
        {
            get {
                SetDefaultFormat();
                return _format; 
            }
            set { 
                _format = value;
            }
        }
        public bool ShouldSerializeFormat() { return !string.IsNullOrEmpty(_format); }

        /// <summary>
        /// True if the column is a DateTime displaying time
        /// </summary>
        [XmlIgnore]
        public bool HasTime
        {
            get
            {
                if (Type != ColumnType.DateTime) return false;
                return Helper.HasTimeFormat(DateTimeStandardFormat, Format);
            }
        }

        /// <summary>
        /// Set standard format accroding to the type
        /// </summary>
        public void SetStandardFormat()
        {
            ColumnType type = Type;
            if (this is ReportElement)
            {
                //Force the type of the ReportElement
                ReportElement element = (ReportElement)this;
                if (element.IsDateTime) type = ColumnType.DateTime;
                else if (element.IsNumeric) type = ColumnType.Numeric;
                else type = ColumnType.Text;
            }
            SetDefaultFormat();
            if (type == ColumnType.Numeric && NumericStandardFormat != NumericStandardFormat.Custom && NumericStandardFormat != NumericStandardFormat.Default)
            {
                _format = Helper.ConvertNumericStandardFormat(NumericStandardFormat);
            }
            else if (type == ColumnType.DateTime && DateTimeStandardFormat != DateTimeStandardFormat.Custom && DateTimeStandardFormat != DateTimeStandardFormat.Default)
            {
                _format = Helper.ConvertDateTimeStandardFormat(DateTimeStandardFormat);
            }
        }

        /// <summary>
        /// Set default format defined in the repository configuration accroding to the type
        /// </summary>
        protected void SetDefaultFormat()
        {
            if (this is ReportElement)
            {
                //Force the type of the ReportElement
                ReportElement element = (ReportElement)this;
                if (element.MetaColumn == null) return;

                element.MetaColumn.SetDefaultFormat();
                if (element.IsNumeric && NumericStandardFormat == NumericStandardFormat.Default)
                {
                    _format = element.MetaColumn.Type == ColumnType.Numeric ? element.MetaColumn.Format : Source.Repository.Configuration.NumericFormat;
                }
                else if (element.IsDateTime && DateTimeStandardFormat == DateTimeStandardFormat.Default)
                {
                    _format = element.MetaColumn.Type == ColumnType.DateTime ? element.MetaColumn.Format : Source.Repository.Configuration.DateTimeFormat;
                }
            }
            else
            {
                if (Type == ColumnType.Numeric && NumericStandardFormat == NumericStandardFormat.Default) _format = Source.Repository.Configuration.NumericFormat;
                else if (Type == ColumnType.DateTime && DateTimeStandardFormat == DateTimeStandardFormat.Default) _format = Source.Repository.Configuration.DateTimeFormat;
            }
        }

        protected string _enumGUID;
        /// <summary>
        /// If defined, a list of values is proposed when the column is used for restrictions
        /// </summary>
        [DefaultValue(null)]
        [Category("Options"), DisplayName("Enumerated List"), Description("If defined, a list of values is proposed when the column is used for restrictions."), Id(4, 3)]
        [TypeConverter(typeof(MetaEnumConverter))]
        public string EnumGUID
        {
            get { return _enumGUID; }
            set { _enumGUID = value; }
        }
        public bool ShouldSerializeEnumGUID() { return !string.IsNullOrEmpty(_enumGUID); }

        /// <summary>
        /// Enumerated list if the column has an EnumGUID
        /// </summary>
        [XmlIgnore]
        public MetaEnum Enum
        {
            get {
                MetaEnum result = null;
                if (!string.IsNullOrEmpty(_enumGUID))
                {
                    if (_source  != null) result = _source.MetaData.Enums.FirstOrDefault(i => i.GUID == _enumGUID);
                    else if (this is ReportRestriction)
                    {
                        //task restriction
                        var restriction = this as ReportRestriction;
                        foreach (var source in restriction.Report.Sources)
                        {
                            result = source.MetaData.Enums.FirstOrDefault(i => i.GUID == _enumGUID);
                            if (result != null) break;
                        }
                    }
                }
                return result;
            }
        }

        protected MetaSource _source;

        /// <summary>
        /// Current MetaSource
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public MetaSource Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// True if the source is a standard SQL source
        /// </summary>
        [XmlIgnore]
        public bool IsSQL
        {
            get { return _source == null || !_source.IsNoSQL; }
        }

        MetaTable _metaTable = null;
        /// <summary>
        /// Current MetaTable of the column
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public MetaTable MetaTable
        {
            get
            {
                if (_metaTable == null && _source != null)
                {
                    _metaTable = _source.MetaData.Tables.FirstOrDefault(i => i.Columns.Exists(j => j.GUID == GUID));
                }
                return _metaTable;
            }
            set
            {
                _metaTable = value;
            }
        }

        /// <summary>
        /// Full display name
        /// </summary>
        [XmlIgnore]
        public string FullDisplayName
        {
            get {
                if (MetaTable == null) return _displayName;
                return string.Format("{0}.{1}", string.IsNullOrEmpty(MetaTable.Name) ? MetaTable.Alias : MetaTable.Name, _displayName);
            }
        }

        /// <summary>
        /// Display name used to sort the TreeView
        /// </summary>
        [XmlIgnore]
        public string DisplayName2
        {
            get
            {
                return string.Format("{0} ({1})", _name, _displayName);
            }
        }

        /// <summary>
        /// Returns the SQL column name without prefix
        /// </summary>
        [XmlIgnore]
        public string ColumnName
        {
            get
            {
                if (_name == null) return "";
                return _name.Split('.').Last();
            }
        }

        /// <summary>
        /// LINQ Column name of the element
        /// </summary>
        [XmlIgnore]
        public string RawLINQColumnName
        {
            get
            {
                var converter = "String";
                if (Type == ColumnType.DateTime) converter = "DateTime";
                else if (Type == ColumnType.Numeric) converter = "Double";
                return string.Format("Helper.To{0}({1}[{2}])", converter, MetaTable.LINQResultName, Helper.QuoteDouble(Name));
            }
        }
        /// <summary>
        /// Defines the child columns to navigate from this column with the drill feature
        /// </summary>
        [Category("Drill"), DisplayName("Drill Children"), Description("Defines the child columns to navigate from this column with the drill feature."), Id(1, 4)]
        [Editor(typeof(ColumnsSelector), typeof(UITypeEditor))]
        public List<string> DrillChildren { get; set; } = new List<string>();
        public bool ShouldSerializeDrillChildren() { return DrillChildren.Count > 0; }

        /// <summary>
        /// If true, Drill Up is activated only if a drill down occured
        /// </summary>
        [DefaultValue(false)]
        [Category("Drill"), DisplayName("Drill Up only if drill down occured."), Description("If true, Drill Up is activated only if a drill down occured."), Id(2, 4)]
        public bool DrillUpOnlyIfDD { get; set; } = false;

        /// <summary>
        /// Editor Helper: Create automatically a 'Year' column and a 'Month' column to drill down to the date
        /// </summary>
        [Category("Drill"), DisplayName("Create Date Columns for Drill"), Description("Create automatically a 'Year' column and a 'Month' column to drill down to the date."), Id(3, 4)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperCreateDrillDates
        {
            get { return "<Click to create a 'Year' and a 'Month' column having Drill navigation>"; }
        }

        /// <summary>
        /// Defines sub-reports to navigate from this column
        /// </summary>
        [Category("Sub-Reports"), DisplayName("Sub Reports"), Description("Defines sub-reports to navigate from this column."), Id(1, 5)]
        [Editor(typeof(EntityCollectionEditor), typeof(UITypeEditor))]
        public List<SubReport> SubReports { get; set; } = new List<SubReport>();
        public bool ShouldSerializeSubReports() { return SubReports.Count > 0; }

        /// <summary>
        /// Editor Helper: Create a Sub-Report to display the detail of this table
        /// </summary>
        [Category("Sub-Reports"), DisplayName("Create a Sub-Report"), Description("Create a Sub-Report to display the detail of this table."), Id(2, 5)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperCreateSubReport
        {
            get { return "<Click to create a Sub-Report to display the detail of this table>"; }
        }

        /// <summary>
        /// Editor Helper: Add an existing Sub-Report to this column
        /// </summary>
        [Category("Sub-Reports"), DisplayName("Add an existing Sub-Report"), Description("Add an existing Sub-Report to this column."), Id(2, 5)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperAddSubReport
        {
            get { return "<Click to add an existing Sub-Report to this column>"; }
        }

        /// <summary>
        /// Editor Helper: Open the Sub-Report folder in Windows Explorer
        /// </summary>
        [Category("Sub-Reports"), DisplayName("Open Sub-Report folder"), Description("Open the Sub-Report folder in Windows Explorer."), Id(3, 5)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperOpenSubReportFolder
        {
            get { return "<Click to open the Sub-Report folder in Windows Explorer>"; }
        }

        #region Helpers

        /// <summary>
        /// Editor Helper: Check the column SQL statement in the database
        /// </summary>
        [Category("Helpers"), DisplayName("Check column SQL syntax"), Description("Check the column SQL statement in the database."), Id(1, 10)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperCheckColumn
        {
            get { return "<Click to check the column SQL syntax>"; }
        }

        /// <summary>
        /// Editor Helper:  Click to create an enumerated list from this table column
        /// </summary>
        [Category("Helpers"), DisplayName("Create enumerated list from this column"), Description("Click to create an enumerated list from this table column."), Id(2, 10)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperCreateEnum
        {
            get { return "<Click to create an enum from the column>"; }
        }

        /// <summary>
        /// Editor Helper:  Show the first 1000 values of the column
        /// </summary>
        [Category("Helpers"), DisplayName("Show column values"), Description("Show the first 1000 values of the column."), Id(3, 10)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperShowValues
        {
            get { return "<Click to show the column values>"; }
        }

        /// <summary>
        /// Last information message ther column has been checked
        /// </summary>
        [XmlIgnore, Category("Helpers"), DisplayName("Information"), Description("Last information message after the column has been checked."), Id(5, 10)]
        [EditorAttribute(typeof(InformationUITypeEditor), typeof(UITypeEditor))]
        public string Information { get; set; }

        /// <summary>
        /// Last error message
        /// </summary>
        [XmlIgnore, Category("Helpers"), DisplayName("Error"), Description("Last error message."), Id(6, 10)]
        [EditorAttribute(typeof(ErrorUITypeEditor), typeof(UITypeEditor))]
        public string Error { get; set; }

        #endregion

    }
}
