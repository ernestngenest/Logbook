namespace Kalbe.App.InternshipLogbookLogbook.Api.Models
{
    public class AllowanceResponse
    {
        public string EducationCode { get; set; }
        public string EducationName { get; set; }

        public List<Allowance> Allowances { get; set; } = new List<Allowance>();
    }

    public class Allowance
    {
        public string WorkType { get; set; }
        public long AllowanceFee { get; set; }
    }
}
