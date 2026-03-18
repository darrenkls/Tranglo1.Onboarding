using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class ScreeningDBContext : BaseDbContext
    {
        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";

        public ScreeningDBContext(DbContextOptions<ScreeningDBContext> options, IEventDispatcher dispatcher,
            IUnitOfWork unitOfWorkContext, IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        public DbSet<ScreeningInput> ScreeningInputs { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<ScreeningDetail> ScreeningDetails { get; set; }
        public DbSet<WatchlistReview> WatchlistReviews { get; set; }
        public DbSet<WatchlistReviewDocument> WatchlistReviewDocuments { get; set; }
        public DbSet<ScreeningEntityType> ScreeningEntityTypes { get; set; }
        public DbSet<WatchlistStatus> WatchlistStatuses { get; set; }
        public DbSet<ScreeningTypeList> ScreeningTypeLists { get; set; }
        public DbSet<OwnershipStrucureType> OwnershipStrucureTypes { get; set; }
        public DbSet<ScreeningDetailsCategory> ScreeningDetailsCategory { get; set; }
        public DbSet<EnforcementActions> EnforcementActions { get; set; }
    }
}
