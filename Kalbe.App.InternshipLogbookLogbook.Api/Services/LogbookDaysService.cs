using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.Library.Common.EntityFramework.Data;
using Kalbe.Library.Common.Logs;
using Kalbe.Library.Data.EntityFrameworkCore.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ILogger = Kalbe.Library.Common.Logs.ILogger;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Services
{
    public interface ILogbookDaysService : IChildService<LogbookDays>
    {

    }
    public class LogbookDaysService : ChildService<LogbookDays>, ILogbookDaysService
    {
        private readonly InternshipLogbookLogbookDataContext _dbContext;
        private readonly ILogger _logger;
        private readonly string _moduleCode = "LOGB";
        public LogbookDaysService(Library.Common.Logs.ILogger logger, InternshipLogbookLogbookDataContext dbContext) : base(logger, dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
    }
}
