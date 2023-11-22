using Kalbe.Library.Common.EntityFramework.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Models
{
    [Table("t_Logbook")]
    public class InternshipLogbookLogbook : Base
    {
        [Required]
        public string Name { get; set; }
        public string Upn { get; set; }
        public string DocNo { get; set; }

        //public string DepartmentCode { get; set; }
        [Required]
        public string DepartmentName { get; set; }
        public long SchoolCode { get; set; }
        [Required]
        public string SchoolName { get; set; }
        public string FacultyCode { get; set; }
        [Required]
        public string FacultyName { get; set; }
        [Required]
        public string Month { get; set; }
        [Required]
        public long Allowance { get; set; }
        public int WFHCount { get; set; }
        public int WFOCount { get; set; }
        public string Status { get; set; }

        //set relasi
        public List<LogbookDays> LogbookDays { get; set; } = new List<LogbookDays>();

    }
}
