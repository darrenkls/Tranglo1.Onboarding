using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class SignUpCodeDBContext : BaseDbContext
    {
        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";

        public SignUpCodeDBContext(DbContextOptions<SignUpCodeDBContext> options, IEventDispatcher dispatcher,
            IUnitOfWork unitOfWorkContext, IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        public DbSet<SignUpCode> SignUpCodes { get; set; }
        public DbSet<SignUpAccountStatus> SignUpAccountStatuses { get; set; }
        public DbSet<LeadsOrigin> LeadsOrigins { get; set; }
    }
}
