using Kalbe.Library.Common.EntityFramework.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Models
{
    [Table("t_Logger")]
    public class ActivityLog : Base
    {
        public string AppCode { get; set; }
        public string ModuleCode { get; set; }
        public string DocumentNumber { get; set; }
        public string Activity { get; set; }
        public string CompanyId { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public string PayLoad { get; set; }
        public string PayLoadType { get; set; }
        public string ExternalEntity { get; set; }
    }
}
