using Kalbe.Library.Common.EntityFramework.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Models
{
    [Table("d_LogbookDays")]
    public class LogbookDays : Base
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Activity { get; set; }
        public string Status { get; set; }
        [Required]
        public string WorkType { get; set; }
        public long AllowanceFee { get; set; }

        public long LogbookId { get; set; }

        [ForeignKey("LogbookId")]
        [JsonIgnore]
        public InternshipLogbookLogbook Logbook { get; set; }
    }
}
