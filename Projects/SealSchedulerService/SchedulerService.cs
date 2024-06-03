﻿using Seal.Helpers;
using Seal.Model;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace SealSchedulerService
{
    public partial class SchedulerService : ServiceBase
    {
        public SchedulerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Helper.WriteLogEntryScheduler(EventLogEntryType.Information, "Starting Scheduler from the Scheduler Service");
            //Run scheduler
            Task.Run(() => StartScheduler());
        }

        private void StartScheduler()
        {
            try
            {
                SealReportScheduler.Instance.Run();
            }
            catch (Exception ex)
            {
                Helper.WriteLogEntryScheduler(EventLogEntryType.Error, ex.Message);
            }
        }

        protected override void OnStop()
        {
            SealReportScheduler.Instance.Shutdown();
        }
    }
}
