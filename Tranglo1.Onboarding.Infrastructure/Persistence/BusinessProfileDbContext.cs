using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Verification;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Infrastructure.Event;
using Tranglo1.Onboarding.Infrastructure.Services;
using UserType = Tranglo1.Onboarding.Domain.Entities.UserType;

namespace Tranglo1.Onboarding.Infrastructure.Persistence
{
    public class BusinessProfileDbContext : BaseDbContext
    {
        public BusinessProfileDbContext(
            DbContextOptions<BusinessProfileDbContext> options, IEventDispatcher dispatcher,
            IUnitOfWork unitOfWorkContext, IIdentityContext identityContext)
            : base(options, dispatcher, unitOfWorkContext, identityContext)
        {

        }

        public const string DEFAULT_SCHEMA = "dbo";
        public const string META_SCHEMA = "meta";
        public const string HISTORY_SCHEMA = "history";
        public const string EVENT_SCHEMA = "event";

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //System.Diagnostics.Debugger.Launch();
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

            //builder.Entity<ApplicationUser>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<CustomerUser>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<UserType>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<Solution>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<Country>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<AccountStatus>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<UserRole>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<TrangloStaff>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<ApplicationUserClaim>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<Gender>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<Country>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<IndividualProfile>().Metadata.SetIsTableExcludedFromMigrations(true);
            //builder.Entity<SystemEnvironment>().Metadata.SetIsTableExcludedFromMigrations(true);



            //builder.ApplyConfiguration(new BusinessNatureEntityTypeConfiguration());
            //builder.ApplyConfiguration(new ReviewResultEntityTypeConfiguration());
            //builder.ApplyConfiguration(new WorkFlowStatusEntityTypeConfiguration());
            //builder.ApplyConfiguration(new KYCStatusEntityTypeConfiguration());
            //builder.ApplyConfiguration(new IDTypeEntityTypeConfiguration());
            //builder.ApplyConfiguration(new BusinessProfileEntityTypeConfiguration());
            //builder.ApplyConfiguration(new CustomerUserBusinessProfileEntityTypeConfiguration());
            //builder.ApplyConfiguration(new CustomerUserBusinessProfileRoleEntityTypeConfiguration());

            //To be ignored as these list are handled in another bounded context in a separate DB Context
            //builder.Ignore<ApplicationUser>();
            //builder.Ignore<CustomerUser>();
            //builder.Ignore<UserType>();
            //builder.Ignore<Solution>();
            //builder.Ignore<Country>();
            //builder.Ignore<AccountStatus>();
            //builder.Ignore<UserRole>();
            //builder.Ignore<TrangloStaff>();
            //builder.Ignore<ApplicationUserClaim>();


            /*
			 * 
			builder.ApplyConfiguration(new ApplicationUserEntityTypeConfiguration());
			builder.ApplyConfiguration(new CustomerUserEntityTypeConfiguration());
			builder.ApplyConfiguration(new SolutionEntityTypeConfiguration());
			builder.ApplyConfiguration(new CountryEntityTypeConfiguration());
			builder.ApplyConfiguration(new AccountStatusEntityTypeConfiguration());
			builder.ApplyConfiguration(new UserTypeEntityTypeConfiguration());

			builder.Entity<ApplicationUser>().ToTable("ApplicationUsers", t => t.ExcludeFromMigrations());
			builder.Entity<CustomerUser>().ToTable("CustomerUsers", t => t.ExcludeFromMigrations());
			builder.Entity<UserType>().ToTable("UserTypes", t => t.ExcludeFromMigrations());
			builder.Entity<Solution>().ToTable("Solutions", t => t.ExcludeFromMigrations());
			builder.Entity<Country>().ToTable("Countries", t => t.ExcludeFromMigrations());
			builder.Entity<AccountStatus>().ToTable("AccountStatuses", t => t.ExcludeFromMigrations());
			*/
        }

        //KYC - Business Profile related entities
        public DbSet<ShareholderType> ShareholderTypes { get; set; }
        public DbSet<DocumentCategoryBPStatus> DocumentCategoryBPStatuses { get; set; }
        public DbSet<BusinessNature> BusinessNatures { get; set; }
        public DbSet<ReviewResult> ReviewResults { get; set; }
        public DbSet<KYCCategory> KYCCategories { get; set; }
        public DbSet<WorkflowStatus> WorkflowStatuses { get; set; }
        public DbSet<KYCStatus> KYCStatuses { get; set; }
        public DbSet<IDType> IDTypes { get; set; }
        public DbSet<BusinessProfile> BusinessProfiles { get; set; }
        public DbSet<CustomerUserBusinessProfile> CustomerUserBusinessProfiles { get; set; }
        public DbSet<CustomerUserBusinessProfileRole> CustomerUserBusinessProfileRoles { get; set; }
        public DbSet<LicenseInformation> LicenseInformations { get; set; }
        public DbSet<COInformation> COInformations { get; set; }
        public DbSet<KYCSubmissionStatus> KYCInformationStatuses { get; set; }
        public DbSet<IndividualLegalEntity> IndividualLegalEntities { get; set; }
        public DbSet<CompanyLegalEntity> CompanyLegalEntities { get; set; }
        public DbSet<IndividualShareholder> IndividualShareholders { get; set; }
        public DbSet<CompanyShareholder> CompanyShareholders { get; set; }
        public DbSet<LegalEntity> LegalEntities { get; set; }
        public DbSet<Shareholder> Shareholders { get; set; }
        public DbSet<ShareholderCompanyLegalEntity> ShareholderCompanyLegalEntities { get; set; }
        public DbSet<ShareholderIndividualLegalEntity> ShareholderIndividualLegalEntities { get; set; }

        public DbSet<BoardOfDirector> BoardOfDirectors { get; set; }
        public DbSet<PrimaryOfficer> PrimaryOfficers { get; set; }
        public DbSet<PoliticallyExposedPerson> PoliticallyExposedPersons { get; set; }
        public DbSet<ParentHoldingCompany> ParentHoldingCompanies { get; set; }
        public DbSet<AffiliateAndSubsidiary> AffiliatesAndSubsidiaries { get; set; }
        public DbSet<AMLCFTDisplayRules> AMLCFTDisplayRule { get; set; }
        public DbSet<DocumentCommentUploadBP> DocumentCommentUploadBPs { get; set; }
        public DbSet<DocumentCategory> DocumentCategories { get; set; }
        public DbSet<DocumentCategoryBP> DocumentCategoryBPs { get; set; }
        public DbSet<DocumentCategoryGroup> DocumentCategoryGroups { get; set; }
        public DbSet<DocumentCategoryTemplate> DocumentCategoryTemplates { get; set; }
        public DbSet<QuestionManagement> QuestionManagements { get; set; }
        public DbSet<DocumentCommentBP> DocumentCommentBPs { get; set; }
        public DbSet<DocumentUploadBP> DocumentUploadBPs { get; set; }
        public DbSet<DocumentReleaseBP> DocumentReleaseBPs { get; set; }
        public DbSet<EmailRecipient> EmailRecipients { get; set; }
        public DbSet<RecipientType> RecipientTypes { get; set; }
        public DbSet<BusinessProfileIDType> BusinessProfileIDTypes { get; set; }

        public DbSet<AMLCFTQuestionnaire> AMLCFTQuestionnaires { get; set; }
        public DbSet<AMLCFTQuestionnaireAnswer> AMLCFTQuestionnaireAnswers { get; set; }
        public DbSet<AnswerChoice> AnswerChoices { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionInputType> QuestionInputTypes { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionnaireSolution> QuestionnaireSolutions { get; set; }
        public DbSet<QuestionSection> QuestionSections { get; set; }

        public DbSet<Declaration> Declarations { get; set; }
        public DbSet<KYCSubModuleReview> kYCSubModuleReviews { get; set; }
        public DbSet<KYCSummaryFeedback> KYCSummaryFeedback { get; set; }
        public DbSet<KYCCustomerSummaryFeedback> KYCCustomerSummaryFeedbacks { get; set; }
        public DbSet<InternalDocumentUpload> InternalDocumentUploads { get; set; }
        public DbSet<RequisitionRunningNumber> RequisitionRunningNumbers { get; set; }
        public DbSet<BusinessDocumentGroupCategory> BusinessDocumentGroupCategories { get; set; }
        public DbSet<BusinessDocumentCategory> BusinessDocumentCategories { get; set; }
        public DbSet<Title> Titles { get; set; }

	


        //Declare only to refer to these list of DbSet but will be ignored in migration
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<CustomerUser> CustomerUsers { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<AccountStatus> AccountStatuses { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatuses { get; set; }
        public DbSet<RelationshipTieUp> RelationshipTieUps { get; set; }
        public DbSet<ServicesOffered> ServicesOffered { get; set; }
        public DbSet<EntityType> EntityTypes { get; set; }
        public DbSet<UserVerificationToken> UserVerificationTokens { get; set; }
        public DbSet<ExternalUserRole> ExternalUserRoles { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<CollectionTier> CollectionTiers { get; set; }
        public DbSet<DeclarationQuestion> DeclarationQuestions { get; set; }
        public DbSet<CustomerBusinessDeclaration> CustomerBusinessDeclarations { get; set; }
        public DbSet<CustomerBusinessDeclarationAnswer> CustomerBusinessDeclarationAnswers { get; set; }
        public DbSet<BusinessDeclarationStatus> BusinessDeclarationStatuses { get; set; }
        public DbSet<DeclarationQuestionType> DeclarationQuestionTypes { get; set; }
        public DbSet<IncorporationCompanyType> IncorporationCompanyTypes { get; set; }
        public DbSet<BusinessDeclarationRejectionMatrix> BusinessDeclarationRejectionMatrixes { get; set; }


        //Transaction Evaluation start
        public DbSet<TransactionEvaluationQuestion> TransactionEvaluationQuestions { get; set; }
        public DbSet<TransactionEvaluationInfo> TransactionEvaluationInfos { get; set; }
        public DbSet<TransactionEvaluationQuestionInputType> TransactionEvaluationQuestionInputTypes { get; set; }
        public DbSet<TransactionEvaluationAnswerChoice> TransactionEvaluationAnswerChoices { get; set; }
        public DbSet<CustomerBusinessTransactionEvaluationAnswer> CustomerBusinessTransactionEvaluationAnswers { get; set; }
        //Transaction Evaluation end

        public DbSet<Gender> Genders { get; set; }
        public DbSet<AuthorisedPerson> AuthorisedPeople { get; set; }
        public DbSet<AuthorisationLevel> AuthorisationLevels { get; set; }
        public DbSet<KYCCategoryCustomerType> KYCCategoryCustomerTypes { get; set; }
        public DbSet<CountryMeta> CountryMetas { get; set; }

        //Verification Changes
        public DbSet<VerificationStatus> VerificationStatuses { get; set; }
        public DbSet<SubmissionResult> SubmissionResults { get; set; }
        public DbSet<RiskScore> RiskScores { get; set; }
        public DbSet<RiskType> RiskTypes { get; set; }
        public DbSet<CustomerVerification> CustomerVerifications { get; set; }
        public DbSet<CustomerVerificationDocuments> CustomerVerificationDocuments { get; set; }
        public DbSet<VerificationIDType> VerificationIDTypes { get; set; }
        public DbSet<VerificationIDTypeSection> VerificationIDTypeSections { get; set; }
        public DbSet<JumioVerification> JumioVerifications { get; set; }
        public DbSet<JumioAccountCreation> JumioAccountCreations { get; set; }
        public DbSet<JumioUpload> JumioUploads { get; set; }
        public DbSet<JumioFinalization> JumioFinalizations { get; set; }
        public DbSet<JumioGetWorkflowDetail> JumioGetWorkflowDetails { get; set; }
        public DbSet<BusinessUserDeclaration> BusinessUserDeclarations { get; set; }
        public DbSet<KYCSubmissionStatus> KYCSubmissionStatuses { get; set; }
        public DbSet<ScreeningInput> ScreeningInputs { get; set; }
        public DbSet<ScreeningDetail> ScreeningDetails { get; set; }
        public DbSet<WatchlistStatus> WatchlistStatuses { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<ScreeningEntityType> ScreeningEntityTypes { get; set; }
        public DbSet<OwnershipStrucureType> OwnershipStrucureTypes { get; set; }
        public DbSet<CompanyUserAccountStatus> CompanyUserAccountStatuses { get; set; }
        public DbSet<CompanyUserBlockStatus> CompanyUserBlockStatuses { get; set; }
        public DbSet<ChangeCustomerTypeDocumentUploadBP> ChangeCustomerTypeDocumentUploadBPs { get; set; }
        public DbSet<ChangeCustomerTypeLicenseInformation> ChangeCustomerTypeLicenseInformations { get; set; }
        public DbSet<ChangeCustomerTypeCOInformation> ChangeCustomerTypeCOInformations { get; set; }
        public DbSet<DefaultTemplate> DefaultTemplates { get; set; }
        public DbSet<DefaultTemplateDocument> DefaultTemplateDocuments { get; set; }
        public DbSet<KYCCustomerSummaryFeedbackNotification> KYCCustomerSummaryFeedbackNotifications { get; set; }
        public DbSet<KYCSummaryFeedbackNotification> KYCSummaryFeedbackNotifications { get; set; }
    }
}