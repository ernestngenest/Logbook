using DinkToPdf;
using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.App.InternshipLogbookLogbook.Api.Utilities;
using Kalbe.Library.Common.EntityFramework.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using ILogger = Kalbe.Library.Common.Logs.ILogger;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Services
{
    public interface IInternshipLogbookLogbookService : ISimpleBaseCrud<Models.InternshipLogbookLogbook>
    {
        Task<List<CalculatedWorkType>> CalculatedWorkTypeandAllowance(Models.InternshipLogbookLogbook data);
        Task<byte[]> GeneratePDF(Models.InternshipLogbookLogbook logbook);
        Task<string> UploadSign(IFormFile file);
        Task<List<string>> GetFilterMonth(DateTime start, DateTime end);
    }

    public class InternshipLogbookLogbookService : SimpleBaseCrud<Models.InternshipLogbookLogbook>, IInternshipLogbookLogbookService
    {
        private readonly InternshipLogbookLogbookDataContext _dbContext;
        private readonly ILogger _logger;
        private readonly ILoggerHelper _loggerHelper;
        private readonly ILogbookDaysService _logbookDaysService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPDFGenerator _pdfGenerator;
        private readonly IMasterClientService _masterClient;
        private readonly IGlobalHelper _globalHelper;
        private readonly string _moduleCode = "LOGB";
        private string cUpn;
        private string cDisplayName;
        private string cRole;
        public InternshipLogbookLogbookService(ILogger logger, InternshipLogbookLogbookDataContext dbContext, ILoggerHelper loggerHelper, IHttpContextAccessor httpContextAccessor, ILogbookDaysService logbookDaysService, IPDFGenerator pdfGenerator, IMasterClientService masterClient, IGlobalHelper globalHelper) : base(logger, dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logbookDaysService = logbookDaysService;
            _loggerHelper = loggerHelper;
            _httpContextAccessor = httpContextAccessor;
            _pdfGenerator = pdfGenerator;
            _masterClient = masterClient;
            _globalHelper = globalHelper;
            cDisplayName = _httpContextAccessor.HttpContext.Request.Headers["CSTM-NAME"];
            cRole = _httpContextAccessor.HttpContext.Request.Headers["CSTM-ROLE"];
            cUpn = _httpContextAccessor.HttpContext.Request.Headers["CSTM-UPN"];
        }

        public override async Task<Models.InternshipLogbookLogbook> GetById(long id)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Get By Id";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion

            try
            {
                timerFunction.Start();
                timer.Start();
                logData.ExternalEntity += "Start to get by id ";
                logData.PayLoadType += "EF";

                var data = _dbContext.InternshipLogbookLogbooks
                            .AsNoTracking()
                            .Include(s => s.LogbookDays.Where(x => !x.IsDeleted))
                            //.Where(s => s.Id.Equals(id) && s.Upn.Equals(cUpn)).FirstOrDefault();
                            .Where(s => s.Id.Equals(id))
                            .FirstOrDefault();

                if (data == null)
                {
                    throw new Exception("data with id : " + id + " not found");
                }
                timer.Stop();
                logData.ExternalEntity += "End get data by id duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timerFunction.Stop();
                logData.Message += "Duration call get data by id : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);

                return data;

            }
            catch (Exception ex)
            {
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }

        public override async Task<Models.InternshipLogbookLogbook> Save(Models.InternshipLogbookLogbook data)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Save";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion

            try
            {
                timerFunction.Start();
                timer.Start();
                logData.ExternalEntity += "Start to get by id ";
                logData.PayLoadType += "EF, ";

                var dataLogbook = _dbContext.InternshipLogbookLogbooks
                            .AsNoTracking()
                            .Include(s => s.LogbookDays.Where(x => !x.IsDeleted))
                            .Where(s => s.Id.Equals(data.Id))
                            .FirstOrDefault();
                timer.Stop();
                logData.ExternalEntity += "End get data by id duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();
                // first add to logbook
                if (data == null)
                {
                    timer.Start();
                    logData.ExternalEntity = "Start to save new data";
                    logData.PayLoadType = "EF, ";

                    data.DocNo = await _globalHelper.GenerateDocNoAsync();
                    data.Status = "Draft";
                    _dbContext.InternshipLogbookLogbooks.Add(data);
                    _dbContext.SaveChanges();

                    timer.Stop();
                    logData.ExternalEntity += "End save new data duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();
                    return data;
                }
                else
                {
                    timer.Start();
                    logData.ExternalEntity = "Start to update logbook days";
                    logData.PayLoadType = "EF, ";
                    await _dbContext.Database.BeginTransactionAsync();
                    var existingLogDays = await _dbContext.LogbookDays.AsNoTracking().Where(s => s.LogbookId == data.Id).ToListAsync();

                    await _logbookDaysService.Update(existingLogDays, data.LogbookDays);
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    
                    timer.Stop();
                    logData.ExternalEntity += "End update logbook days duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();
                }

                timerFunction.Stop();
                logData.Message += "Duration call get data by id : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);

                return data;

            }
            catch (Exception ex)
            {
                await _dbContext.Database.RollbackTransactionAsync();
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }

        //public async Task<Models.InternshipLogbookLogbook> Submit()
        //{

        //}

        public async Task<List<CalculatedWorkType>> CalculatedWorkTypeandAllowance(Models.InternshipLogbookLogbook data)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Calculate Work Type and Allowance";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                timerFunction.Start();
                timer.Start();
                logData.ExternalEntity += "1. Start to group by work type ";
                logData.PayLoadType += "EF, ";

                var grouping = _dbContext.LogbookDays.Where(s => s.LogbookId == data.Id).AsEnumerable().GroupBy(s => s.WorkType);
                timer.Stop();
                logData.ExternalEntity += "End group by work type duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timer.Start();
                logData.ExternalEntity += "1. Start to update data ";
                logData.PayLoadType += "EF";

                var workTypeGrouping = new List<CalculatedWorkType>();

                foreach (var item in grouping)
                {
                    var groupWorkType = new CalculatedWorkType
                    {
                        Worktype = item.Key,
                        WorkTypeCount = item.Count(),
                        CalculcatedAllowance = item.Sum(s => s.AllowanceFee)
                    };
                    workTypeGrouping.Add(groupWorkType);
                }
                return workTypeGrouping;
            }
            catch (Exception ex)
            {
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }

        public async Task<List<string>> GetFilterMonth(DateTime start, DateTime end)
        {
            #region log data
            ActivityLog logData = new ActivityLog();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Get Filter Month";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion
            try
            {
                timerFunction.Start();
                timer.Start();

                logData.ExternalEntity += "Start get filter month ";
                logData.PayLoadType += "EF";

                DateTime iterator = new DateTime(start.Year, start.Month, 1);

                var dateTimeFormat = CultureInfo.GetCultureInfo("id-ID").DateTimeFormat;

                var listMonth = new List<string>();

                while (iterator <= end)
                {
                    listMonth.Add(dateTimeFormat.GetMonthName(iterator.Month));
                    iterator = iterator.AddMonths(1);
                }

                timer.Stop();
                logData.ExternalEntity += "End get filter month duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timerFunction.Stop();
                logData.Message += "Duration call filter month : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);

                return listMonth;
            }
            catch (Exception ex)
            {
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }

        public async Task<string> UploadSign(IFormFile file)
        {
            string filename = "";
            try
            {
                var user = await _masterClient.GetUserInternalByUPN(cUpn);
                var extension = "." + file.FileName.Split(".")[file.FileName.Split(".").Length - 1];
                filename = user.Name.Replace(" ", "") + extension;

                var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\");

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\", filename);

                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch { 
            
            
            }
            return filename;
        }

        public async Task<byte[]> GeneratePDF(Models.InternshipLogbookLogbook logbook)
        {

            var mentor = await _masterClient.GetMentorByUPN(logbook.Upn);
            var signImage = _pdfGenerator.ImageUrl(mentor.MentorName.Replace(" ", ""));
            var htmlContent = "";
            htmlContent += "<html><body style=\"padding: 20px; font-family: Calibri, sans-serif;\">    " +
                "<h1 style=\"font-size: 16px;\"><b>PT KALBE FARMA</b></h1>    <br>    <h1 style=\"text-align: center; font-size: 16px; \"><b>INTERNSHIP ATTENDANCE</b></h1>    " +
                "<table style=\"width: 100%; border: 1px solid; border-collapse: collapse; margin-bottom: 20px;\">        <tr>" +
                "<td colspan=\"2\" style=\"width: 30%; border: 1px solid; border-collapse: collapse;\">Name</td>            " +
                "<td style=\"width: 70%; border: 1px solid; border-collapse: collapse;\">: " + logbook.Name + "</td>        </tr>" +
                "<tr><td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">Division / Department</td>" +
                "<td style=\"border: 1px solid; border-collapse: collapse;\">: " + logbook.DepartmentName + "</td>        </tr>" +
                "<tr><td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">School / College</td>" +
                "<td style=\"border: 1px solid; border-collapse: collapse;\">: " + logbook.SchoolName + "a</td>        </tr>" +
                "<tr><td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">Faculty</td>" +
                "<td style=\"border: 1px solid; border-collapse: collapse;\">: " + logbook.FacultyName + "</td></tr>" +
                "<tr><td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">Month</td>" +
                "<td style=\"border: 1px solid; border-collapse: collapse;\">: " + logbook.Month + "</td></tr></table>" +
                "<table style=\"text-align: center; border: 1px solid; border-collapse: collapse; margin-bottom: 30px;\">        <tr>            <td style=\"width: 5%; border: 1px solid; border-collapse: collapse;\">No</td>            <td style=\"width: 5%; border: 1px solid; border-collapse: collapse;\">Date</td>            <td style=\"width: 50%; border: 1px solid; border-collapse: collapse;\">Activity</td>            <td style=\"width: 7%;border: 1px solid; border-collapse: collapse;\">WFH / WFO</td>            <td style=\"width: 5%; border: 1px solid; border-collapse: collapse;\">Mentor&#39;s Sign</td>        </tr>";
            int i = 1;
            foreach (var item in logbook.LogbookDays)
            {
                htmlContent += "<tr><td style=\"border: 1px solid; border-collapse: collapse;\">" + i.ToString() + "</td>" +
                    "<td style=\"border: 1px solid; border-collapse: collapse;\">" + item.Date.ToString("dd/MM/yyyy") + "</td>" +
                    "<td style=\"text-align: left; border: 1px solid; border-collapse: collapse;\">" + item.Activity + "</td>" +
                    "<td style=\"border: 1px solid; border-collapse: collapse;\">" + item.WorkType + "</td>" +
                    "<td style=\"border: 1px solid; border-collapse: collapse;\"></td>        </tr>";
                i++;
            }
            htmlContent += "<table><tr><td colspam=\"2\">Jakarta, "+ logbook.UpdatedDate?.ToString("dd MMM yyyy")+"</td>" +
                "</tr><tr><td colspam=\"2\" style=\"text-align: center;\">"+ signImage +"</td></tr>" +
                "<tr><td colspan=\"2\" style=\"word-wrap: break-word; text-align: center;\">("+ mentor.MentorName +")</td></tr>" +
                "<tr><td colspan=\"2\" style=\"word-wrap: break-word; text-align: center;\"><b>MENTOR</b></td>        </tr>    </table></body></html>";
            // generate PDF
            byte[] pdfBytes = _pdfGenerator.GeneratePDF(htmlContent);
            return pdfBytes;
        }
    }
}
