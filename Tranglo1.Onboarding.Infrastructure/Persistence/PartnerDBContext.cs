using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class PartnerDBContext : BaseDbContext
    {
        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";
        public const string EVENTS_SCHEMA = "events";
        public DbSet<PartnerAccountStatus> PartnerAccountStatus { get; set; }
        public DbSet<PartnerRegistration> PartnerRegistrations { get; set; }
        public DbSet<ChangeType> ChangeTypes { get; set; }
        public DbSet<@string> Currencies { get; set; }
        public DbSet<TrangloEntity> Entities { get; set; }
        public DbSet<PartnerType> PartnerTypes { get; set; }
        public DbSet<PartnerAgreementTemplate> PartnerAgreementTemplate { get; set; }
        public DbSet<SignedPartnerAgreement> SignedPartnerAgreement { get; set; }
        public DbSet<PartnerAgreementStatus> PartnerAgreementStatus { get; set; }
        public DbSet<PartnerRegistration> PartnerRegistration { get; set; }
        public DbSet<PartnerAccountStatusType> PartnerAccountStatusTypes { get; set; }
        public DbSet<HelloSignDocument> HelloSignDocument { get; set; }
        public DbSet<OnboardWorkflowStatus> OnboardWorkflowStatuses { get; set; }
        public DbSet<PartnerAPISetting> PartnerAPISettings { get; set; }
        public DbSet<WhitelistIP> WhitelistIP { get; set; }
        public DbSet<PartnerCMSIntegrationDetail> PartnerCMSIntegrationDetails { get; set; }
        public DbSet<PartnerWalletCMSIntegrationDetail> PartnerWalletCMSIntegrationDetails { get; set; }
        public DbSet<APIURL> APIURL { get; set; }
        public DbSet<PartnerTPNMigrationDetail> PartnerTPNMigrationDetails { get; set; }
        public DbSet<PartnerSubscription> PartnerSubscriptions { get; set; }
        public DbSet<CountryMeta> Countries { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<PartnerProfileChangedEvent> PartnerProfileChangedEvent { get; set; }
        public DbSet<KYCCategoryCustomerType> kYCCategoryCustomerTypes { get; set; }
        public DbSet<KYCReminderStatus> KYCReminderStatuses { get; set; }

        public DbSet<RelationshipTieUp> RelationshipTieUps { get; set; }
        public PartnerDBContext(
            DbContextOptions<PartnerDBContext> options,
            IUnitOfWork unitOfWorkContext,
            IEventDispatcher dispatcher, IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

        }



    }


}
