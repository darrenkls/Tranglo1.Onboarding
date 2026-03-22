using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Domain.Entities.OTP;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;
using UserType = Tranglo1.Onboarding.Domain.Entities.UserType;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class ApplicationUserDbContext : BaseDbContext
    {
        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";
        public const string EVENT_SCHEMA = "event";

        public ApplicationUserDbContext(
            DbContextOptions<ApplicationUserDbContext> options,
            IEventDispatcher dispatcher,
            IUnitOfWork unitOfWorkContext,
            IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        // User / Staff DbSets
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<CustomerUser> CustomerUsers { get; set; }
        public DbSet<CustomerUserRegistration> CustomerUserRegistrations { get; set; }
        public DbSet<TrangloStaff> TrangloStaffs { get; set; }
        public DbSet<TrangloStaffAssignment> TrangloStaffAssignments { get; set; }
        public DbSet<TrangloStaffEntityAssignment> TrangloStaffEntityAssignment { get; set; }
        public DbSet<RoleStatus> RoleStatus { get; set; }

        // Meta / lookup DbSets
        public DbSet<EntityType> EntityTypes { get; set; }
        public DbSet<RelationshipTieUp> RelationshipTieUps { get; set; }
        public DbSet<ServicesOffered> ServicesOffered { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<IncorporationCompanyType> IncorporationCompanyTypes { get; set; }
        public DbSet<TrangloDepartment> TrangloDepartment { get; set; }
        public DbSet<TrangloEntity> TrangloEntity { get; set; }
        public DbSet<ActionOperation> ActionOperations { get; set; }
        public DbSet<CompanyUserAccountStatus> CompanyUserAccountStatus { get; set; }
        public DbSet<CompanyUserBlockStatus> CompanyUserBlockStatus { get; set; }
        public DbSet<Environment> Environments { get; set; }
        public DbSet<AuthorityLevel> AuthorityLevels { get; set; }
        public DbSet<AuthorisationLevel> AuthorisationLevels { get; set; }
        public DbSet<KYCReminderStatus> KYCReminderStatuses { get; set; }
        public DbSet<KYCReminderSubscription> KYCReminderSubscriptions { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AccountStatus> AccountStatuses { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatuses { get; set; }
        public DbSet<CollectionTier> CollectionTiers { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<CountryMeta> CountryMetas { get; set; }
        public DbSet<DefaultTemplate> DefaultTemplates { get; set; }
        public DbSet<ExternalUserRoleStatus> ExternalUserRoleStatuses { get; set; }

        // OTP
        public DbSet<RequisitionOTP> RequisitionOTPs { get; set; }

        // Verification / Meta
        public DbSet<VerificationStatus> VerificationStatuses { get; set; }
        public DbSet<SubmissionResult> SubmissionResults { get; set; }
        public DbSet<RiskScore> RiskScores { get; set; }
        public DbSet<RiskType> RiskTypes { get; set; }
        public DbSet<VerificationIDType> VerificationIDTypes { get; set; }
        public DbSet<VerificationIDTypeSection> VerificationIDTypeSections { get; set; }
    }
}
