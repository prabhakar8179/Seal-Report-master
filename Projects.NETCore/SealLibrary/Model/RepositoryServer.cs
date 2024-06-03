﻿//
// Copyright (c) Seal Report (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Seal.Helpers;

namespace Seal.Model
{
    /// <summary>
    /// The RepositoryServer is used to maintain a static list of ReportViewTemplate for performances purpose
    /// </summary>
    public class RepositoryServer
    {
        private static List<ReportViewTemplate> _viewTemplates = null;
        private static object _viewLock = new object();
        private static List<MetaTableTemplate> _tableTemplates = null;
        private static object _tableLock = new object();

        public static string ViewsFolder = "";
        public static string TableTemplatesFolder = "";

        public static void PreLoadTemplates()
        {
            lock (_viewLock)
            {
                var templates = ViewTemplates;
                foreach (var template in templates)
                {
                    if (!template.IsParsed) template.ParseConfiguration();
                    RazorHelper.Compile(template.Text, typeof(Report), template.CompilationKey);

                    //Create a view to compile partial templates
                    ReportView view = ReportView.Create(template);
                    view.InitPartialTemplates();
                    foreach (var partialTemplate in view.PartialTemplates)
                    {
                        view.GetPartialTemplateKey(partialTemplate.Name, view);
                    }

                }
            }
        }

        /// <summary>
        /// Current list of ReportViewTemplate
        /// </summary>
        public static List<ReportViewTemplate> ViewTemplates
        {
            get
            {
                //used from the Report Designer, load and parse all...
                if (_viewTemplates == null)
                {
                    _viewTemplates = ReportViewTemplate.LoadTemplates(ViewsFolder);
                }
                foreach (var template in _viewTemplates.Where(i => !i.IsParsed)) template.ParseConfiguration();
                return _viewTemplates;
            }
        }

        /// <summary>
        /// Returns a ReportViewTemplate from a given name
        /// </summary>
        public static ReportViewTemplate GetViewTemplate(string name)
        {
            lock (_viewLock)
            {
                if (_viewTemplates == null)
                {
                    _viewTemplates = ReportViewTemplate.LoadTemplates(ViewsFolder);
                }
            }

            var result = _viewTemplates.FirstOrDefault(i => i.Name == name);
            if (result == null)
            {
                lock (_viewLock)
                {
                    //Get the name for configuration text to avoid useless parsing and save time
                    foreach (var template in _viewTemplates.Where(i => !i.IsParsed))
                    {
                        if (template.Configuration.Contains(string.Format("\"{0}\";", name)))
                        {
                            template.ParseConfiguration();
                            break;
                        }
                    }
                }
            }
            result = _viewTemplates.FirstOrDefault(i => i.Name == name);
            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine("!! Loading all the templates !!");

                lock (_viewLock)
                {
                    //Name not found in configuration -> we parse all...
                    foreach (var template in _viewTemplates.Where(i => !i.IsParsed)) template.ParseConfiguration();
                }
                if (name.EndsWith(" HTML")) name = name.Replace(" HTML", ""); //backward compatibility before 5.0
                if (name == "Model CSV Excel") name = "Model"; //backward compatibility before 5.0

                result = _viewTemplates.FirstOrDefault(i => i.Name == name);
            }

            if (result == null) throw new Exception(string.Format("Unable to find view template named '{0}'", name));

            //Check if the file has changed
            if (result.IsModified)
            {
                lock (_viewLock)
                {
                    result.Init(result.FilePath);
                }
            }

            //Check if configuration has been parsed
            if (!result.IsParsed)
            {
                lock (_viewLock)
                {
                    result.ParseConfiguration();
                }
            }

            return result;
        }


        /// <summary>
        /// Current list of MetaTableTemplate
        /// </summary>
        public static List<MetaTableTemplate> TableTemplates
        {
            get
            {
                if (_tableTemplates == null)
                {
                    _tableTemplates = MetaTableTemplate.LoadTemplates(TableTemplatesFolder);
                }
                return _tableTemplates;
            }
        }
    }
}

