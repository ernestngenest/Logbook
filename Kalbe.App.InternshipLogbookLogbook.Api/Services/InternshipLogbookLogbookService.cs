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
        Task<byte[]> GeneratePDF();
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
        private readonly string _moduleCode = "LOGB";
        private string cUpn;
        private string cDisplayName;
        private string cRole;
        public InternshipLogbookLogbookService(ILogger logger, InternshipLogbookLogbookDataContext dbContext, ILoggerHelper loggerHelper, IHttpContextAccessor httpContextAccessor, ILogbookDaysService logbookDaysService, IPDFGenerator pdfGenerator) : base(logger, dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logbookDaysService = logbookDaysService;
            _loggerHelper = loggerHelper;
            _httpContextAccessor = httpContextAccessor;
            _pdfGenerator = pdfGenerator;
            //cDisplayName = _httpContextAccessor.HttpContext.Request.Headers["CSTM-NAME"];
            //cRole = _httpContextAccessor.HttpContext.Request.Headers["CSTM-ROLE"];
            //cUpn = _httpContextAccessor.HttpContext.Request.Headers["CSTM-UPN"];
        }

        public override async Task<Models.InternshipLogbookLogbook> GetById(long id)
        {
            #region log data
            ActivityLog logData = new();
            logData.CreatedDate = DateTime.Now;
            logData.ModuleCode = _moduleCode;
            logData.LogType = "Information";
            logData.Activity = "Get All By Month Current User";
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
                    var groupWorkType = new CalculatedWorkType {
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

        public async Task<byte[]> GeneratePDF()
        {
            DataTable dt = new DataTable();

            var htmlContent = "";
            htmlContent += "<html>\r\n    <head>\r\n        <link rel=\"stylesheet\" href=\"\\Kalbe.App.InternshipLogbookLogbook.Api\\Utilities\\StyleSheet.css\">\r\n    </head>\r\n<body style=\"padding: 20px; font-family: Calibri, sans-serif;\">\r\n    <h1 style=\"font-size: 16px;\"><b>PT KALBE FARMA</b></h1>\r\n    <br>\r\n    <h1 style=\"text-align: center; font-size: 16px; \"><b>INTERNSHIP ATTENDANCE</b></h1>\r\n    <table style=\"width: 100%; border: 1px solid; border-collapse: collapse; margin-bottom: 20px;\">\r\n        <tr>\r\n            <td colspan=\"2\" style=\"width: 30%; border: 1px solid; border-collapse: collapse;\">Name</td>\r\n            <td style=\"width: 70%; border: 1px solid; border-collapse: collapse;\">: Devita Azka Tsaniya</td>\r\n        </tr>\r\n        <tr>\r\n            <td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">Division / Department</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">: Corporate IT</td>\r\n        </tr>\r\n        <tr>\r\n            <td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">School / College</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">: Universitas Bina Nusantara</td>\r\n        </tr>\r\n        <tr>\r\n            <td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">Faculty</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">: Computer Science</td>\r\n        </tr>\r\n        <tr>\r\n            <td colspan=\"2\" style=\"border: 1px solid; border-collapse: collapse;\">Month</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">: Maret</td>\r\n        </tr>\r\n    </table>\r\n    <table style=\"text-align: center; border: 1px solid; border-collapse: collapse; margin-bottom: 30px;\">\r\n        <tr>\r\n            <td style=\"width: 5%; border: 1px solid; border-collapse: collapse;\">No</td>\r\n            <td style=\"width: 5%; border: 1px solid; border-collapse: collapse;\">Date</td>\r\n            <td style=\"width: 50%; border: 1px solid; border-collapse: collapse;\">Activity</td>\r\n            <td style=\"width: 7%;border: 1px solid; border-collapse: collapse;\">WFH / WFO</td>\r\n            <td style=\"width: 5%; border: 1px solid; border-collapse: collapse;\">Mentor&#39;s Sign</td>\r\n        </tr>\r\n        <tr>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">1</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">3/1/23</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">Mempelajari basic .NET Core 3.1</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">WFH</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\"></td>\r\n        </tr>\r\n        <tr>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">2</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">3/2/23</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">Membuat dummy API menggunakan .NET Core 3.1</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\">WFH</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse;\"><img src=\"https://upload.wikimedia.org/wikipedia/commons/thumb/f/fb/Indonesia_road_sign_%28Prohibitory%29_3b.svg/900px-Indonesia_road_sign_%28Prohibitory%29_3b.svg.png\" alt=\"\" style=\"width: 30;\" srcset=\"\"></td>\r\n        </tr>\r\n        <tr>\r\n            <td>3</td>\r\n            <td>3/3/23</td>\r\n            <td>Resolve error dummy API .NET Core 3.1</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>4</td>\r\n            <td>3/6/23</td>\r\n            <td>Memperbaiki error code dummy API .NET 3.1</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>5</td>\r\n            <td>3/7/23</td>\r\n            <td>Mempelajari RabbitMQ dan dasar .NET 6 MVC</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>6</td>\r\n            <td>3/8/23</td>\r\n            <td>Mempelajari flow controller .NET 6 MVC dan View</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>7</td>\r\n            <td>3/9/23</td>\r\n            <td>Mempelajari flow kerja antara controller dan view .NET Core 3.1</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>8</td>\r\n            <td>3/10/23</td>\r\n            <td>Mempelajari flow coding project sebelumnya yang menggunakan MVC .NET Core 3.1</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>9</td>\r\n            <td>3/13/23</td>\r\n            <td>Membuat dummy MVC menggunakan .NET Core 3.1 </td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>10</td>\r\n            <td>3/14/23</td>\r\n            <td style=\"border: 1px solid; border-collapse: collapse; word-wrap: break-word;\">Membuat dummy MVC menggunakan .NET Core 3.1 dengan tech stack kalbe simple CRUD dan sharing session devops OCP jhkshdkjhsadksahdkjashdjkashdkjhasjkdhsajhdkjash</td>\r\n            <td>WFO</td>\r\n            <td><img src=\"https://upload.wikimedia.org/wikipedia/commons/thumb/f/fb/Indonesia_road_sign_%28Prohibitory%29_3b.svg/900px-Indonesia_road_sign_%28Prohibitory%29_3b.svg.png\" alt=\"\" style=\"width: 30;\" srcset=\"\"></td>\r\n        </tr>\r\n        <tr>\r\n            <td>11</td>\r\n            <td>3/15/23</td>\r\n            <td>Merging dan cloning branch untuk deploy ke OCP</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>12</td>\r\n            <td>3/16/23</td>\r\n            <td>Unit Test fixing, deploy global api ke OCP</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>13</td>\r\n            <td>3/17/23</td>\r\n            <td>Redeployed global api ke OCP</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>14</td>\r\n            <td>3/20/23</td>\r\n            <td>Redeployed global api ke OCP</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>15</td>\r\n            <td>3/21/23</td>\r\n            <td>Redeployed global api ke OCP, mempelajari CRUD One To Many dan cara membuat Unit Test</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>16</td>\r\n            <td>3/23/23</td>\r\n            <td>Mempelajari pembuatan Unit Test dan membuat Unit Test untuk Global Location API</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>17</td>\r\n            <td>3/24/23</td>\r\n            <td>Membuat Unit Test untuk Global Location API</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>18</td>\r\n            <td>3/27/23</td>\r\n            <td>Mempelajari cara mendaftarkan API pada Red Hat (create product dan subscribe) dan mempelajari Postgre</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>19</td>\r\n            <td>3/28/23</td>\r\n            <td>Mempelajari Postgres</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>20</td>\r\n            <td>3/29/23</td>\r\n            <td>Memperdalam javascript</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>21</td>\r\n            <td>3/30/23</td>\r\n            <td>Mempelajari javascript untuk backend dan mempelajari unit test</td>\r\n            <td>WFO</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td>22</td>\r\n            <td>3/31/23</td>\r\n            <td>Memperdalam vanilla javascript</td>\r\n            <td>WFH</td>\r\n            <td></td>\r\n        </tr>\r\n        <tr>\r\n            <td></td>\r\n            <td></td>\r\n            <td></td>\r\n            <td></td>\r\n            <td></td>\r\n        </tr>\r\n    </table>\r\n    <table>\r\n        <tr>\r\n            <td colspam=\"2\">Jakarta, ........ 2023</td>\r\n        </tr>\r\n        <tr>\r\n            <td colspam=\"2\" style=\"text-align: center;\"><img src=\"https://upload.wikimedia.org/wikipedia/commons/thumb/f/fb/Indonesia_road_sign_%28Prohibitory%29_3b.svg/900px-Indonesia_road_sign_%28Prohibitory%29_3b.svg.png\" alt=\"\" style=\"width: 60px;\" srcset=\"\"></td>\r\n        </tr>\r\n        <tr>\r\n            <td colspan=\"2\" style=\"word-wrap: break-word; text-align: center;\">(MOHAMMAD FAISAL AMIRRUDIN)</td>\r\n        </tr>\r\n        <tr>\r\n            <td colspan=\"2\" style=\"word-wrap: break-word; text-align: center;\"><b>MENTOR</b></td>\r\n        </tr>\r\n    </table>\r\n</body>\r\n</html>";
            // PDF generation
            byte[] pdfBytes = _pdfGenerator.GeneratePDF(htmlContent);
            return pdfBytes;
        }
    }
}
