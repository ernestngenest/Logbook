using Kalbe.Library.Common.EntityFramework.Models;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Models
{
    public class Mentor
    {
        public string MentorUPN { get; set; }
        public string MentorName { get; set; }
        public string MentorEmail { get; set; }
    }

    public class UserInternal
    {
        public string UserPrincipalName { get; set; }
        public string NIK { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string JobTitle { get; set; }
        public string DeptName { get; set; }
        public string CompCode { get; set; }
        public string CompName { get; set; }
        public string Password { get; set; }
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {

        public string UserPrincipalName { get; set; }
        public string Email { get; set; }

        public string Name { get; set; }

        public string RoleCode { get; set; }
        public string RoleName { get; set; }

        public long? UserInternalId { get; set; }
    }
}
