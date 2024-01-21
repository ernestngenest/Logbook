using DinkToPdf;
using Elastic.CommonSchema;
using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.App.InternshipLogbookLogbook.Api.Models.Commons;
using Kalbe.App.InternshipLogbookLogbook.Api.Utilities;
using Kalbe.Library.Common.EntityFramework.Data;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.HPSF;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Email = Kalbe.App.InternshipLogbookLogbook.Api.Models.Commons.Email;
using File = System.IO.File;
using ILogger = Kalbe.Library.Common.Logs.ILogger;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Services
{
    public interface IInternshipLogbookLogbookService : ISimpleBaseCrud<Models.InternshipLogbookLogbook>
    {
        Task<PagedList<Models.InternshipLogbookLogbook>> GetLogbookData(PagedOptions pagedOptions);
        Task<List<CalculatedWorkType>> CalculatedWorkTypeandAllowance(Models.InternshipLogbookLogbook data);
        Task<byte[]> GeneratePDF(Models.InternshipLogbookLogbook logbook);
        Task<string> UploadSign(IFormFile file);
        Task<string> PreviewSign();
        Task<List<string>> GetFilterMonth(DateTime start, DateTime end);
        Task<Models.InternshipLogbookLogbook> Save(Models.InternshipLogbookLogbook data);
        Task<Models.InternshipLogbookLogbook> Submit(Models.InternshipLogbookLogbook data);
        Task Revise(long id, string notes);
        Task Approve(long id);
        Task<PagedList<Models.InternshipLogbookLogbook>> GetMentorTask(PagedOptions pagedOptions);
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
        private readonly IOptions<AppSettingModel> _appSettingModel;
        private readonly AppSettingModel _settingModel;
        private readonly string _moduleCode = "LOGB";
        private string cUpn;
        private string cDisplayName;
        private string cRole;
        private string cEducation;
        private string cEmail;
        public InternshipLogbookLogbookService(ILogger logger, InternshipLogbookLogbookDataContext dbContext, ILoggerHelper loggerHelper, IOptions<AppSettingModel> appSettingModel, IHttpContextAccessor httpContextAccessor, ILogbookDaysService logbookDaysService, IPDFGenerator pdfGenerator, IMasterClientService masterClient, IGlobalHelper globalHelper) : base(logger, dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logbookDaysService = logbookDaysService;
            _loggerHelper = loggerHelper;
            _appSettingModel = appSettingModel;
            _settingModel = appSettingModel.Value;
            _httpContextAccessor = httpContextAccessor;
            _pdfGenerator = pdfGenerator;
            _masterClient = masterClient;
            _globalHelper = globalHelper;
            cDisplayName = _httpContextAccessor.HttpContext.Request.Headers["CSTM-NAME"];
            cRole = _httpContextAccessor.HttpContext.Request.Headers["CSTM-ROLE"];
            cUpn = _httpContextAccessor.HttpContext.Request.Headers["CSTM-UPN"];
            cEducation = _httpContextAccessor.HttpContext.Request.Headers["CSTM-EDUCATION"];
            cEmail = _httpContextAccessor.HttpContext.Request.Headers["CSTM-EMAIL"];
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
            logData.PayLoad = JsonConvert.SerializeObject(data, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
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
                if (dataLogbook == null)
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

        public async Task<PagedList<Models.InternshipLogbookLogbook>> GetLogbookData(PagedOptions pagedOptions)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Get Logbook Data";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion

            try
            {
                if (pagedOptions == null)
                {
                    throw new Exception("Pagedoptions is empty, please check header");
                }
                timerFunction.Start();
                timer.Start();
                logData.ExternalEntity += "Start to get logbook data ";
                logData.PayLoadType += "EF";

                var data = _dbContext.InternshipLogbookLogbooks
                            .AsNoTracking()
                            .Include(s => s.LogbookDays.Where(x => !x.IsDeleted).OrderBy(x => x.Date))
                            .Where(s => s.Upn.Equals(cUpn) && !s.IsDeleted);

                timer.Stop();
                logData.ExternalEntity += "End get logbook data  duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timer.Start();
                logData.ExternalEntity += "2. Start Get Pagedlist";
                logData.PayLoadType += "EF";



                var result = await PagedList<Models.InternshipLogbookLogbook>.GetPagedList(data, pagedOptions);

                timer.Stop();
                logData.ExternalEntity += "End get pagedlist duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timerFunction.Stop();
                logData.Message += "Duration call get logbook data : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);

                return result;

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

        public async Task<Models.InternshipLogbookLogbook> Submit(Models.InternshipLogbookLogbook data)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Submit";
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

                #region hit api approval
                var nextApprover = await _masterClient.GetMentorByUPN(dataLogbook.Upn);

                timer.Start();
                logData.ExternalEntity += "2. Start hit api Approval ";
                logData.PayLoadType += "API approval,";

                ApprovalTransactionData approvalTransactionData = new()
                {
                    SystemCode = Constant.SystemCode,
                    ModuleCode = Constant.ModuleCode,
                    DocNo = dataLogbook.DocNo,
                    Role = Constant.Mentor,
                    ApproverFrom = cUpn,
                    ApproverFromName = dataLogbook.Name,
                    ApproverFromEmail = dataLogbook.Upn,
                    ApproverTo = nextApprover.MentorEmail,
                    CreatedBy = dataLogbook.CreatedBy,
                    CreatedDate = DateTime.Now.ToString(),
                };

                var resultApprovalData = await _masterClient.GetApprovalMaster(Constant.SystemCode, Constant.ModuleCode);
                ApprovalTransactionDataModel approvalTransactionDataModels = new();
                approvalTransactionDataModels.SystemCode = Constant.SystemCode;
                approvalTransactionDataModels.ModuleCode = Constant.ModuleCode;
                approvalTransactionDataModels.ApprovalLevel = resultApprovalData.Select(x => x.ApprovalLevel).FirstOrDefault();
                approvalTransactionDataModels.DocNo = dataLogbook.DocNo;
                approvalTransactionDataModels.EmailPIC = nextApprover.MentorEmail;
                approvalTransactionDataModels.NamePIC = nextApprover.MentorName;
                approvalTransactionDataModels.PIC = nextApprover.MentorUPN;
                approvalTransactionDataModels.ApprovalLine = 1;
                approvalTransactionDataModels.NeedApprove = false;
                approvalTransactionDataModels.CreatedByUpn = cUpn;
                approvalTransactionDataModels.CreatedByEmail = dataLogbook.Upn;
                approvalTransactionDataModels.CreatedByName = dataLogbook.Name;
                approvalTransactionDataModels.CreatedDate = DateTime.Now;
                approvalTransactionData.ApprovalTransactionDataModel.Add(approvalTransactionDataModels);
                await _masterClient.Submit(approvalTransactionData);
                timer.Stop();
                logData.ExternalEntity += "End hit api approval duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();
                #endregion

                //update status and calculating 

                timer.Start();
                logData.ExternalEntity += "2. Start update data ";
                logData.PayLoadType += "EF ";
                await _dbContext.Database.BeginTransactionAsync();
                dataLogbook.Status = "Waiting for " + nextApprover.MentorName + "'s approval";
                if(dataLogbook.Allowance == 0)
                {
                    var calculate = await CalculatedWorkTypeandAllowance(data);
                    var WFHCount =  calculate.Where(s => s.Worktype.Equals("WFH")).FirstOrDefault();
                    if (WFHCount != null)
                    {
                        dataLogbook.WFHCount = WFHCount.WorkTypeCount;
                        dataLogbook.Allowance += WFHCount.CalculcatedAllowance;
                    }
                    else
                    {
                        dataLogbook.WFHCount = 0;
                    }
                    var WFOCount =  calculate.Where(s => s.Worktype.Equals("WFO")).FirstOrDefault();
                    if (WFOCount != null)
                    {
                        dataLogbook.WFOCount = WFOCount.WorkTypeCount;
                        dataLogbook.Allowance += WFOCount.CalculcatedAllowance;
                    }
                    else
                    {
                        dataLogbook.WFOCount = 0;
                    }
                }
                _dbContext.Entry(dataLogbook).State = EntityState.Modified;
                _dbContext.SaveChanges();
                await _dbContext.Database.CommitTransactionAsync();
                timer.Stop();
                logData.ExternalEntity += "End update data duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timer.Start();
                logData.ExternalEntity += "2. Start update data ";
                logData.PayLoadType += "APi Mail ";

                string emailBody = "Dear " + nextApprover.MentorName + "<br>";
                emailBody += "Logbook berikut ini membutuhkan persetujuan anda :<br><br>";
                emailBody += "<table><tr><td>Nama mentee : " + dataLogbook.Name + "</td></tr>";
                emailBody += "<tr><td>Bulan : " + dataLogbook.Month + "</td></tr>";
                emailBody += "<tr><td>Universitas/Sekolah : " + dataLogbook.SchoolName + "</td></tr>";
                emailBody += "<tr><td>Total WFO : " + dataLogbook.WFOCount + "</td></tr>";
                emailBody += "<tr><td>Total WFH : " + dataLogbook.WFHCount + "</td></tr></table>";
                emailBody += "Silakan klik " + "<a href=" + "isi domain di sini" + "/detail/" + data.Id + ">link</a>" + " berikut ini untuk melihat dokumen tersebut." + "<br>";
                emailBody += "Mohon untuk login menggunakan Username : " + nextApprover.MentorUPN + "<br><br>";
                emailBody += "Terima kasih.<br><br>";
                emailBody += "Please do not reply this message.";
                Email email = new Email
                {
                    EmailTo = nextApprover.MentorEmail,
                    EmailCC = dataLogbook.Upn,
                    EmailSubject = "E-Logbook Mail System - Logbook " + dataLogbook.Name + " (" + dataLogbook.Month + ") membutuhkan persetujuan anda",
                    EmailBody = emailBody,
                    DocumentNumber = dataLogbook.DocNo
                };
                if(_settingModel.EmailDummy)
                {
                    email.EmailTo = _settingModel.RecipientEmail;
                    email.EmailCC = _settingModel.RecipientEmail;
                }

                await _masterClient.SendEmail(email);

                timer.Stop();
                logData.ExternalEntity += "End update data duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timerFunction.Stop();
                logData.Message += "Duration call submit logbook : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);

                return data;

            }
            catch (Exception ex)
            {
                if(_dbContext.Database.CurrentTransaction != null )
                {
                    await _dbContext.Database.RollbackTransactionAsync();
                }
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }

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
                logData.ExternalEntity += "2. Start to get allowance by education";
                logData.PayLoadType += "EF, ";

                var allowanceRes = await _masterClient.GetAllowanceByEducation(cEducation);
                timer.Stop();
                logData.ExternalEntity += "End get allowance by education duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timer.Start();
                logData.ExternalEntity += "3. Start to update data ";
                logData.PayLoadType += "EF";

                var workTypeGrouping = new List<CalculatedWorkType>();

                foreach (var item in grouping)
                {
                    var groupWorkType = new CalculatedWorkType
                    {
                        Worktype = item.Key,
                        WorkTypeCount = item.Count(),
                        CalculcatedAllowance = item.Count() * allowanceRes.Allowances.Where(s => s.WorkType.Contains(item.Key)).Select(s => s.AllowanceFee).FirstOrDefault()
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

        public async Task Revise(long id, string notes)
        {
            #region log data
            ActivityLog logData = new()
            {
                CreatedDate = DateTime.Now,
                AppCode = Constant.SystemCode,
                ModuleCode = Constant.ModuleCode,
                LogType = "Information",
                Activity = "Revise",
                DocumentNumber = id.ToString()
            };
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(cRole) || string.IsNullOrEmpty(cUpn)
                    || string.IsNullOrEmpty(cDisplayName))
                {
                    throw new Exception("failed get header. please check role, upn, name, or email session apps");
                }

                timerFunction.Start();

                timer.Start();
                logData.ExternalEntity += "1. Start to get data ";
                logData.PayLoadType = "EF,";
                var dataLogbook = _dbContext.InternshipLogbookLogbooks
                            .AsNoTracking()
                            .Include(s => s.LogbookDays.Where(x => !x.IsDeleted))
                            .Where(s => s.Id.Equals(id))
                            .FirstOrDefault();
                timer.Stop();
                logData.ExternalEntity += "End Get Data duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                if (dataLogbook != null)
                {
                    #region hit api approval
                    timer.Start();
                    logData.ExternalEntity += "2. Start hit api approval ";
                    logData.PayLoadType += "API Approval,";

                    ApprovalLogModel approvalLogModel = new()
                    {
                        SystemCode = Constant.SystemCode,
                        ModuleCode = Constant.ModuleCode,
                        DocNo = dataLogbook.DocNo,
                        Role = Constant.Mentor,
                        ApproverFrom = cUpn,
                        ApproverFromName = cDisplayName,
                        ApproverFromEmail = cEmail,
                        ApproverTo = dataLogbook.Upn,
                        CreatedBy = cUpn,
                        Notes = notes,
                        Status = "Revise",
                        CreatedDate = DateTime.Now.ToString()
                    };

                    await _masterClient.Reject(approvalLogModel);
                    timer.Stop();
                    logData.ExternalEntity += "End call api approval duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();
                    #endregion

                    #region send mail
                    timer.Start();
                    logData.ExternalEntity += "3. Start hit api notification ";
                    logData.PayLoadType += "API Notification";

                    string emailBody = "Dear " + dataLogbook.Name + "<br>";
                    emailBody += "Logbook berikut ini membutuhkan perbaikan anda :<br><br>";
                    emailBody += "<table><tr><td>Nama mentee : " + dataLogbook.Name + "</td></tr>";
                    emailBody += "<tr><td>Bulan : " + dataLogbook.Month + "</td></tr>";
                    emailBody += "<tr><td>Universitas/Sekolah : " + dataLogbook.SchoolName + "</td></tr>";
                    emailBody += "<tr><td>Total WFO : " + dataLogbook.WFOCount + "</td></tr>";
                    emailBody += "<tr><td>Total WFH : " + dataLogbook.WFHCount + "</td></tr>";
                    emailBody += "<tr><td>Alasan revise : " + notes + "</td></tr></table>";
                    emailBody += "Silakan klik " + "<a href=" + "isi domain di sini" + "/detail/" + dataLogbook.Id + ">link</a>" + " berikut ini untuk melihat dokumen tersebut." + "<br>";
                    emailBody += "Mohon untuk login menggunakan Username : " + dataLogbook.Upn + "<br><br>";
                    emailBody += "Terima kasih.<br><br>";
                    emailBody += "Please do not reply this message.";

                    var notification = new Email()
                    {
                        EmailTo = dataLogbook.Upn,
                        EmailCC = cEmail,
                        ModuleCode = _moduleCode,
                        DocumentNumber = dataLogbook.DocNo,
                        EmailSubject = "E-Logbook Mail System - Logbook " + dataLogbook.Name + " (" + dataLogbook.Month + ") membutuhkan perbaikan dari anda",
                        EmailBody = emailBody
                    };

                    if (_settingModel.EmailDummy)
                    {
                        notification.EmailTo = _settingModel.RecipientEmail;
                        notification.EmailCC = _settingModel.RecipientEmail;
                    }

                    //send email
                    await _masterClient.SendEmail(notification);

                    timer.Stop();
                    logData.ExternalEntity += "End hit api notification duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();
                    #endregion
                    timer.Start();
                    logData.ExternalEntity += "4. Start update status ";
                    dataLogbook.Status = "Waiting for revision from " + dataLogbook.Name + " - " + notes;
                    _dbContext.Entry(dataLogbook).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    timer.Stop();
                    logData.ExternalEntity += "End update status duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();

                    timerFunction.Stop();
                    logData.Message += "Duration Call : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    await _loggerHelper.Save(logData);
                }
                else
                {
                    throw new Exception("data not found");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                timerFunction.Stop();
                logData.LogType = "Error";
                logData.Message += "Error " + ex + ". Duration : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);
                throw;
            }
        }

        public async Task Approve(long id)
        {
            #region log data
            ActivityLog logData = new()
            {
                CreatedDate = DateTime.Now,
                AppCode = Constant.SystemCode,
                ModuleCode = Constant.ModuleCode,
                LogType = "Information",
                Activity = "Approve",
                DocumentNumber = id.ToString()
            };
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                //var role = _globalHelper.GetRoleFromToken(cAuth, cDisplayName);
                //if (!role.ToUpper().Contains(Constant.PicHse.ToUpper()))
                //{
                if (string.IsNullOrEmpty(cRole) || string.IsNullOrEmpty(cUpn)
                || string.IsNullOrEmpty(cDisplayName) || string.IsNullOrEmpty(cEmail))
                {
                    throw new Exception("failed get header. please check role, upn, name, or email session apps");
                }
                //    throw new Exception("You have no access");
                //}

                timerFunction.Start();

                timer.Start();
                logData.ExternalEntity += "1. Start to get data ";
                logData.PayLoadType = "EF,";
                var dataLogbook = _dbContext.InternshipLogbookLogbooks
                            .AsNoTracking()
                            .Include(s => s.LogbookDays.Where(x => !x.IsDeleted))
                            .Where(s => s.Id.Equals(id))
                            .FirstOrDefault();
                timer.Stop();
                logData.ExternalEntity += "End Get Data duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                if (dataLogbook != null)
                {
                    #region hit api approval
                    timer.Start();
                    logData.ExternalEntity += "2. Start hit api approval ";
                    logData.PayLoadType += "API Approval,";

                    ApprovalTransactionData approvalTransactionData = new()
                    {
                        SystemCode = Constant.SystemCode,
                        ModuleCode = _moduleCode,
                        DocNo = dataLogbook.DocNo,
                        Role = Constant.Mentor,
                        ApproverFrom = cUpn,
                        ApproverFromName = cDisplayName,
                        ApproverFromEmail = cEmail,
                        ApproverTo = dataLogbook.Upn,
                        CreatedBy = cUpn,
                        CreatedDate = DateTime.Now.ToString(),
                    };
                    var result = await _masterClient.GetCurrentWF(dataLogbook.DocNo);

                    //Start approve document
                    var currentApprover = result.Where(w => w.PIC.ToLower() == cUpn.ToLower()).FirstOrDefault();
                    approvalTransactionData.ApprovalTransactionDataModel.Add(currentApprover);
                    await _masterClient.Approve(approvalTransactionData);

                    timer.Stop();
                    logData.ExternalEntity += "End call api approval duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();
                    #endregion


                    timer.Start();
                    logData.ExternalEntity += "2. Start update status ";
                    dataLogbook.Status = "Approved";
                    _dbContext.Entry(dataLogbook).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    timer.Stop();
                    logData.ExternalEntity += "End update status duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();

                    #region send mail
                    timer.Start();
                    logData.ExternalEntity += "3. Start hit api notification ";
                    logData.PayLoadType += "API Notification";

                    string emailBody = "Dear " + dataLogbook.Name + "<br>";
                    emailBody += "Logbook berikut ini telah disetujui :<br><br>";
                    emailBody += "<table><tr><td>Nama mentee : " + dataLogbook.Name + "</td></tr>";
                    emailBody += "<tr><td>Bulan : " + dataLogbook.Month + "</td></tr>";
                    emailBody += "<tr><td>Universitas/Sekolah : " + dataLogbook.SchoolName + "</td></tr>";
                    emailBody += "<tr><td>Total WFO : " + dataLogbook.WFOCount + "</td></tr>";
                    emailBody += "<tr><td>Total WFH : " + dataLogbook.WFHCount + "</td></tr></table>";
                    emailBody += "Silakan klik " + "<a href=" + "isi domain di sini" + "/detail/" + dataLogbook.Id + ">link</a>" + " berikut ini untuk melihat dokumen tersebut." + "<br>";
                    emailBody += "Mohon untuk login menggunakan Username : " + dataLogbook.Upn + "<br><br>";
                    emailBody += "Terima kasih.<br><br>";
                    emailBody += "Please do not reply this message.";

                    var email = new Email()
                    {
                        EmailTo = dataLogbook.Upn,
                        EmailCC = cEmail,
                        ModuleCode = _moduleCode,
                        DocumentNumber = dataLogbook.DocNo,
                        EmailSubject = "E-Logbook Mail System - Logbook " + dataLogbook.Name + " (" + dataLogbook.Month + ") telah disetujui",
                        EmailBody = emailBody
                    };

                    if (_settingModel.EmailDummy)
                    {
                        email.EmailTo = _settingModel.RecipientEmail;
                        email.EmailCC = _settingModel.RecipientEmail;
                    }

                    //send email
                    await _masterClient.SendEmail(email);

                    timer.Stop();
                    logData.ExternalEntity += "End hit api notification duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    timer.Reset();
                    #endregion


                    timerFunction.Stop();
                    logData.Message += "Duration Call : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                    await _loggerHelper.Save(logData);
                }
                else
                {
                    throw new Exception("data not found");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                var name = user.Name.Replace(" ", "");
                var extension = "." + file.FileName.Split(".")[file.FileName.Split(".").Length - 1];
                filename =  name + extension;

                var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\" + name);

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\", filename);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
            return filename;
        }

        public async Task<string> PreviewSign()
        {
            string imgUrl = "";
            try
            {
                var user = await _masterClient.GetUserInternalByUPN(cUpn);
                var name = user.Name.Replace(" ", "");
                var imageName = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\", name);
                var extension = Path.GetExtension(imageName);
                imgUrl = imageName + extension;
                return imgUrl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        public async Task<byte[]> GeneratePDF(Models.InternshipLogbookLogbook logbook)
        {

            var mentor = await _masterClient.GetMentorByUPN(logbook.Upn);
            var fileName = mentor.MentorName.Replace(" ", "");
            var signImage = _pdfGenerator.ImageUrl(fileName);
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
                    "<td style=\"border: 1px solid; border-collapse: collapse;\">"+signImage+"</td></tr>";
                i++;
            }
            htmlContent += "<table><tr><td colspam=\"2\">Jakarta, " + logbook.UpdatedDate?.ToString("dd MMM yyyy") + "</td>" +
                "</tr><tr><td colspam=\"2\" style=\"text-align: center;\">" + signImage + "</td></tr>" +
                "<tr><td colspan=\"2\" style=\"word-wrap: break-word; text-align: center;\">(" + mentor.MentorName + ")</td></tr>" +
                "<tr><td colspan=\"2\" style=\"word-wrap: break-word; text-align: center;\"><b>MENTOR</b></td>        </tr>    </table></body></html>";
            // generate PDF
            byte[] pdfBytes = _pdfGenerator.GeneratePDF(htmlContent);
            return pdfBytes;
        }

        public async Task<PagedList<Models.InternshipLogbookLogbook>> GetMentorTask(PagedOptions pagedOptions)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Get Mentor Task";
            var timer = new Stopwatch();
            var timerFunction = new Stopwatch();
            #endregion

            try
            {
                if (pagedOptions == null)
                {
                    throw new Exception("Pagedoptions is empty, please check header");
                }
                timerFunction.Start();
                timer.Start();
                logData.ExternalEntity += "Start to get mentor task";
                logData.PayLoadType += "EF";

                var data = _dbContext.InternshipLogbookLogbooks
                            .AsNoTracking()
                            .Include(s => s.LogbookDays.Where(x => !x.IsDeleted).OrderBy(x => x.Date))
                            .Where(s => s.Status.ToLower().Contains(cDisplayName.ToLower()) && !s.IsDeleted);

                timer.Stop();
                logData.ExternalEntity += "End get mentor task duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timer.Start();
                logData.ExternalEntity += "2. Start Get Pagedlist";
                logData.PayLoadType += "EF";



                var result = await PagedList<Models.InternshipLogbookLogbook>.GetPagedList(data, pagedOptions);

                timer.Stop();
                logData.ExternalEntity += "End get pagedlist duration : " + timer.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                timer.Reset();

                timerFunction.Stop();
                logData.Message += "Duration call get mentor task data : " + timerFunction.Elapsed.ToString(@"m\:ss\.fff") + ". ";
                await _loggerHelper.Save(logData);

                return result;

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
    }
}
