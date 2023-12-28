using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Utilities
{
    public interface IGlobalHelper
    {
        Task<string> GenerateDocNoAsync();
    }
    public class GlobalHelper : IGlobalHelper
    {
        private readonly InternshipLogbookLogbookDataContext _dbContext;
        private readonly IConfiguration _configuration;
        public GlobalHelper(InternshipLogbookLogbookDataContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        public async Task<string> GenerateDocNoAsync()
        {
            try
            {
                string docNo = "";
                var latestNo = GetLatestNo();

                docNo = latestNo + "/LOGB/" + DateTime.Now.ToString("yyyy");

                return docNo;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string GetLatestNo()
        {
            try
            {
                int number = 1;
                string lastNumber = "";
                var data = _dbContext.InternshipLogbookLogbooks
                    .AsNoTracking()
                    .OrderByDescending(s => s.CreatedDate)
                    .FirstOrDefault(s => s.CreatedDate.Year.Equals(DateTime.Now.Year))?.DocNo;

                if (data != null)
                {
                    number = Convert.ToInt32(data.Split("/")[0]) + 1;
                }

                lastNumber = number.ToString("0000");
                return lastNumber;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
