using Hangfire.Storage;
using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.App.InternshipLogbookLogbook.Api.Services;
using Kalbe.App.InternshipLogbookLogbook.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Job
{
    public interface IReminderJob
    {
        Task Execute();
    }
    public class ReminderJob : IReminderJob
    {
        private readonly IInternshipLogbookLogbookService _LogbookServices;
        private readonly ILoggerHelper _loggerHelper;

        public ReminderJob(IInternshipLogbookLogbookService logbook, ILoggerHelper loggerHelper)
        {
            _LogbookServices = logbook;
            _loggerHelper = loggerHelper;
        }

        public async Task Execute()
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.LogType = "Information";
            logData.Activity = "Reminder";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion
            try
            {
                await _LogbookServices.ReminderLogbook();

            }catch (Exception ex)
            {
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }
    }
}
