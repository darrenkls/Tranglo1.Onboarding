using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class ExternalUserRoleDbContext : BaseDbContext
    {
        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";

        public ExternalUserRoleDbContext(DbContextOptions<ExternalUserRoleDbContext> options, IEventDispatcher dispatcher,
            IUnitOfWork unitOfWorkContext, IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        public DbSet<ExternalUserRole> ExternalUserRoles { get; set; }
    }
}
