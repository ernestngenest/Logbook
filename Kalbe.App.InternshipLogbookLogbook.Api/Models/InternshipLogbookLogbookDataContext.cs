using Kalbe.Library.Common.EntityFramework.Data;
using Kalbe.Library.Common.EntityFramework.Models;
using Kalbe.Library.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Models
{
    public class InternshipLogbookLogbookDataContext : BaseDbContext<InternshipLogbookLogbookDataContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public InternshipLogbookLogbookDataContext(DbContextOptions<InternshipLogbookLogbookDataContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options, httpContextAccessor)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                modelBuilder.HasPostgresExtension("citext");
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("citext");
        }

        public override void SetDefaultValues()
        {
            IEnumerable<EntityEntry> enumerable = from x in ChangeTracker.Entries()
                                                  where x.Entity is Base && (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted)
                                                  select x;
            string text = Constant.DefaultActor;
            string text2 = Constant.DefaultActor;
            if (_httpContextAccessor.HttpContext != null)
            {
                ClaimsPrincipal user = _httpContextAccessor.HttpContext!.User;
                Claim claim = user.FindFirst((Claim x) => x.Type == "UserPrincipalName");
                Claim claim2 = user.FindFirst((Claim x) => x.Type == Constant.Name);
                if (claim != null)
                {
                    text = claim.Value;
                }

                if (claim2 != null)
                {
                    text2 = claim2.Value;
                }
            }

            foreach (EntityEntry item in enumerable)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        ((Base)item.Entity).CreatedDate = DateTime.Now;
                        ((Base)item.Entity).CreatedBy = text;
                        ((Base)item.Entity).CreatedByName = text2;
                        ((Base)item.Entity).IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        ((Base)item.Entity).UpdatedDate = DateTime.Now;
                        ((Base)item.Entity).UpdatedBy = text;
                        ((Base)item.Entity).UpdatedByName = text2;
                        ((Base)item.Entity).IsDeleted = false;
                        break;
                    case EntityState.Deleted:
                        item.State = EntityState.Modified;
                        ((Base)item.Entity).UpdatedDate = DateTime.Now;
                        ((Base)item.Entity).UpdatedBy = text;
                        ((Base)item.Entity).UpdatedByName = text2;
                        ((Base)item.Entity).IsDeleted = true;
                        break;
                }
            }
        }

        public virtual DbSet<Models.InternshipLogbookLogbook> InternshipLogbookLogbooks { get; set; }
        public virtual DbSet<ActivityLog> Loggers { get; set; }
        public virtual DbSet<LogbookDays> LogbookDays { get; set; }
    }
}
