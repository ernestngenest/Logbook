using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.Library.Common.EntityFramework.Data;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Utilities
{
    public interface ILoggerHelper : ISimpleBaseCrud<ActivityLog>
    {
    }

    public class LoggerHelper : SimpleBaseCrud<ActivityLog>, ILoggerHelper
    {
        private readonly InternshipLogbookLogbookDataContext _dbContext;
        public LoggerHelper(Library.Common.Logs.ILogger logger, InternshipLogbookLogbookDataContext dbContext) : base(logger, dbContext)
        {
            _dbContext = dbContext;
        }
        public async override Task<ActivityLog> Save(ActivityLog data)
        {
            try
            {
                _dbContext.ChangeTracker.Clear();
                _dbContext.Loggers.Add(data);
                await _dbContext.SaveChangesAsync();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
