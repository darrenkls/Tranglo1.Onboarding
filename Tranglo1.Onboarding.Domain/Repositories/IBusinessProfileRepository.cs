using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
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
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IBusinessProfileRepository
    {
        Task<IReadOnlyList<IDType>> GetIDTypesAsync(Specification<IDType> filters);
        Task<IReadOnlyList<ShareholderType>> GetShareholderTypesAsync(Specification<ShareholderType> filters);
        Task<IReadOnlyList<DocumentCategoryBPStatus>> GetDocumentCategoryBPStatusesAsync(Specification<DocumentCategoryBPStatus> filters);
        Task<IReadOnlyList<KYCStatus>> GetKYCStatusesAsync(Specification<KYCStatus> filters);
        Task<WorkflowStatus> GetWorkflowStatusesAsync(long? workflowStatus);
        Task<IReadOnlyList<ReviewResult>> GetReviewResultsAsync(Specification<ReviewResult> filters);
        Task<IReadOnlyList<BusinessNature>> GetBusinessNaturesAsync(Specification<BusinessNature> filters);
        Task<IReadOnlyList<BusinessProfile>> GetBusinessProfilesAsync(Specification<BusinessProfile> filters, bool isNoTrackKYCSubmissionStatus = false);
        Task<IReadOnlyList<BusinessProfile>> GetBusinessProfileListAsync();
        Task<IReadOnlyList<BusinessProfile>> GetSubmittedKycBusinessProfilesAsync();
        Task<List<AMLCFTDisplayRules>> GetAMLCFTDisplayRuleAsync(EntityType entity, RelationshipTieUp tieUp, List<ServicesOffered> servicesOffered);
        Task<AMLCFTDisplayRules> GetAMLCFTDisplayRuleSingleAsync(EntityType entity, RelationshipTieUp tieUp, ServicesOffered servicesOffered);
        Task<List<Questionnaire>> GetQuestionnairesByAMLCFTQuestionnairesAsync(int businessProfileCode);
        Task<AMLCFTDisplayRules> GetAMLCFTDisplayRuleQuestionnaireAsync(EntityType entity, RelationshipTieUp tieUp, ServicesOffered servicesOffered, Questionnaire questionnaire);
        //
        Task<AMLCFTDisplayRules> GetAMLCFTDisplayRuleByQuestionnaireAsync(EntityType entity, RelationshipTieUp tieUp, ServicesOffered servicesOffered, Questionnaire questionnaire);
        Task<IReadOnlyList<CustomerUserBusinessProfile>> GetCustomerUserBusinessProfilesAsync(Specification<CustomerUserBusinessProfile> filters);
        Task<IReadOnlyList<CustomerUserBusinessProfileRole>> GetCustomerUserBusinessProfilesRolesAsync(Specification<CustomerUserBusinessProfileRole> filters);
        Task<CustomerUserBusinessProfileRole> GetCustomerUserBusinessProfileRolesByCodeAsync(long? customerUserBusinessProfileCode);
        Task<List<CustomerUserBusinessProfileRole>> GetCustomerUserBusinessProfilesByRoleCodeAsync(string roleCode);
        Task<DocumentCategory> GetCategoryInfo(int documentCategoryCode);
        Task<BusinessProfile> GetBusinessProfileByCodeTrackAsync(long? businessProfileCode);
        Task<BusinessProfileIDType> GetBusinessProfileIDTypeByCode(long? BusinessProfileIDTypeCode);
        Task<IReadOnlyList<BusinessProfileIDType>> GetBusinessProfileIDTypesAsync(Specification<BusinessProfileIDType> filters);
        //
        Task<Result<CustomerUserBusinessProfile>> UpdateCustomerUserBusinessProfileStatusAsync(CustomerUserBusinessProfile customerUserBusinessProfile);
        Task<UserVerificationToken> FindUserTokenByEmailAsync(string email);
        //Task<Result<IReadOnlyList<CustomerUserBusinessProfile>>> GetPartnerRegistrationAsync(CustomerUser customer, long partnerCode);
        Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfileByIdAndCodeAsync(int userId, int businessProfileCode);
        Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfileByBusinessProfileCodeAsync(int? businessProfileCode);
        Task<List<CustomerUserBusinessProfile>> GetCustomerUserBusinessProfilesByIdAsync(long userId);
        Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfilesByUserIdAsync(long userId);
        Task<UserVerificationToken> AddUserVerificationToken(UserVerificationToken userVerificationToken);
        Task<InternalDocumentUpload> GetInternalDocumentUploadByDocumentIdAsync(InternalDocumentUpload checkFileUpload);
        Task<ShareholderIndividualLegalEntity> GetShareholderIndividualLegalEntityByLegalEntityCodeAsync(long id);
        Task<BusinessProfile> AddBusinessProfilesAsync(BusinessProfile BusinessProfile);
        void UpdateBusinessProfile(BusinessProfile BusinessProfile);
        Task<UserVerificationToken> UpdateUserVerificationToken(UserVerificationToken updateUserVerificationToken);
        Task<BusinessProfile> UpdateBusinessProfileAsync(BusinessProfile BusinessProfile);
        Task<DocumentCategoryBP> GetDocumentCategoryBPAsync(long documentCategoryCode, int businessProfileCode);
        Task<IReadOnlyList<DocumentCategoryBP>> GetDocumentCategoryBPAsync(int businessProfileCode);
        Task GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(Specification<AffiliateAndSubsidiary> documentSpec);
        Task<InternalDocumentUpload> RemoveInternalDocumentUploadAsync(InternalDocumentUpload documentUploadResult);
        Task<DocumentUploadBP> GetDocumentUploadDocumentId(long id);
        Task<DocumentCategoryTemplate> GetTemplateInfo(long id, long? questionnaireCode);
        CustomerUserBusinessProfile AddCustomerUserBusinessProfile(CustomerUserBusinessProfile customerUserBusinessProfile);

        Task<DocumentUploadBP> GetDocumentUploadByIdAsync(long id, Guid documentId);
        Task<DocumentReleaseBP> GetDocumentReleasedByIdAsync(int businessProfileCode, Guid documentId);
        Task<CustomerUserBusinessProfileRole> AddCustomerUserBusinessProfileRole(CustomerUserBusinessProfileRole customerUserBusinessProfileRole);

        Task<DocumentUploadBP> UpdateDocumentUploadBP(DocumentUploadBP documentInfo);
        Task<DocumentCategoryBP> UpdateDocumentCategoryBPInfo(DocumentCategoryBP documentCategoryInfo);
        Task<DocumentCommentUploadBP> UpdateDocumentCommentUploadBP(DocumentCommentUploadBP documentCommentUploadBP);
        Task<DocumentCommentUploadBP> AddDocumentCommentUploadBP(DocumentCommentUploadBP documentCommentUploadBP);
        Task<List<DocumentCommentBP>> GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(long documentCategoryBPCode);
        Task<List<DocumentCommentBP>> AddDocumentCommentBP(List<DocumentCommentBP> documentCommentBP);
        Task<DocumentReleaseBP> UpdateDocumentReleasedBP(DocumentReleaseBP documentReleasedInfo);
        Task<DocumentReleaseBP> AddDocumentReleasedUploadAsync(DocumentReleaseBP documentReleasedUpload);
        Task<List<DocumentUploadBP>> AddListDocumentUploadAsync(List<DocumentUploadBP> documentUploadBPs);
        /// <summary>
        /// Find all business profiles that are assigned to the given customer email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<BusinessProfile> GetBusinessProfileByCompanyNameAsync(string companyName);
        Task<IReadOnlyList<BusinessProfile>> FindBusinessProfileByEmailAsync(Email email);

        Task<CustomerUserBusinessProfile> AddCustomerUserBusinessProfileMappingAsync(CustomerUserBusinessProfile mapping);
        Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfilesByCodeAsync(long customerUserBusinessProfileCode);
        Task<InternalDocumentUpload> AddInternalDocumentUploadAsync(InternalDocumentUpload internalDocumentUpload);
        Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfilesByUserIdAsync(long userId, long businessProfileCode);
        Task<CustomerUserBusinessProfileRole> GetCustomerUserBusinessProfileRolesByCodeAsync(long? customerUserBusinessProfileCode, string roleCode);
        Task<BusinessProfile> GetBusinessProfileByCodeAsync(long? businessProfileCode);
        Task UpdateCustomerUserBusinessProfileAsync(CustomerUserBusinessProfile customerUserBusinessProfile, CancellationToken cancellationToken);

        Task UpdateCustomerUserBusinessProfileRangeAsync(List<CustomerUserBusinessProfile> customerUserBusinessProfiles, CancellationToken cancellationToken);
        Task<Result<CustomerUserBusinessProfile>> AddCustomerUserBusinessProfileAsync(CustomerUserBusinessProfile customerUserBusinessProfile);
        Task<Result<CustomerUserBusinessProfileRole>> AddCustomerUserBusinessProfileRoleAsync(CustomerUserBusinessProfileRole customerUserBusinessProfileRole);
        Task<Result<CustomerUserBusinessProfile>> DeleteCustomerUserBusinessProfileAsync(CustomerUserBusinessProfile customerUserBusinessProfile);
        Task<Result<CustomerUserBusinessProfileRole>> DeleteCustomerUserBusinessProfileRoleAsync(CustomerUserBusinessProfileRole customerUserBusinessProfileRole);


        Task<Result<LicenseInformation>> AddLicenseInformationsAsync(LicenseInformation licenseInformation);
        Task<Result<LicenseInformation>> UpdateLicenseInformationsAsync(LicenseInformation updatelicenseInfo);
        Task<Result<COInformation>> AddCOInformationsAsync(COInformation coInfo);
        Task<Result<COInformation>> UpdateCOInformationsAsync(COInformation cOInfo);

        Task<BoardOfDirector> AddBoardOfDirectorAsync(BoardOfDirector boardOfDirector);
        Task<IndividualLegalEntity> AddLegalEntityAsync(IndividualLegalEntity legalEntity);
        Task<CompanyLegalEntity> AddLegalEntityAsync(CompanyLegalEntity legalEntity);
        Task<PrimaryOfficer> AddPrimaryOfficerAsync(PrimaryOfficer primaryOfficer);
        Task<CustomerUserBusinessProfile> GetCustomerUserBPById(int userId, int businessProfileCode);
        Task<Result<DocumentCommentBP>> AddCommentsAsync(DocumentCommentBP comments);
        Task<DocumentUploadBP> DeleteDocumentUploadBP(DocumentUploadBP documentUploadBP);

        Task GetDocumentUploadDataAsync(Guid documentId, long id);
        Task<DocumentUploadBP> AddDocumentUploadAsync(DocumentUploadBP document);
        Task<DocumentCategoryTemplate> UpdateDocumentTemplateAsync(DocumentCategoryTemplate documentTemplate);
        Task<DocumentCategoryTemplate> GetAMLATemplateInfo(long questionnaireCode);
        Task<DocumentCategoryTemplate> AddDocumentTemplateAsync(DocumentCategoryTemplate documentTemplate);
        Task<DocumentUploadBP> GetDocumentUploadByIdAsync(long id);
        Task<DocumentCategoryBP> UpdateDocumentCategoryBP(DocumentCategoryBP checkCategoryBP);
        Task<Result<DocumentCategoryBP>> GetCategoryBPyCategoryCodeAsync(int documentCategoryCode);
        Task<List<DocumentCategoryBP>> AddDocumentCategoryBPs(List<DocumentCategoryBP> documentCategoryBPs);
        Task<DocumentCategoryBP> GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(int businessProfileCode, long documentCategoryCode);

        Task<IReadOnlyList<DocumentUploadBP>> GetDocumentUploadBPProfile(Specification<DocumentUploadBP> documentUploadBPspec);
        Task<Result<DocumentCategoryBP>> AddCategoryBPAsync(DocumentCategoryBP categoryBP);
        Task<Result<DocumentCommentUploadBP>> AddReviewRemarkAsync(DocumentCommentUploadBP commentBP);

        Task<IReadOnlyList<DocumentCategoryBP>> GetCategoryBPProfile(Specification<DocumentCategoryBP> documentCategoryBPspec);
        Task<DocumentCategoryBP> GetDocumentCategoryAsync(int businessProfileCode, int documentCategoryCode);
        Task<DocumentCommentUploadBP> GetReviewRemarkDocumentAsync(int documentCommentBPCode);
        Task<ParentHoldingCompany> AddParentHoldingCompanyAsync(ParentHoldingCompany parentHoldingCompany);
        Task<ReviewResult> GetReviewResultByCodeAsync(int businessProfileCode, long kycCategoryCode);
        Task<IReadOnlyList<DocumentCategoryBP>> GetDocumentCategoryBPAsync(Specification<DocumentCategoryBP> documentCategoryBPspec);
        Task<Shareholder> AddShareholderAsync(Shareholder shareholder);
        Task<LegalEntity> AddLegalEntityAsync(LegalEntity legalEntity);

        Task<PoliticallyExposedPerson> AddPoliticallyExposedPersonAsync(PoliticallyExposedPerson politicallyExposedPerson);

        // Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryAsync(int documentCategoryCode);
        Task<Result<AffiliateAndSubsidiary>> AddAffiliateAndSubsidiaryAsync(AffiliateAndSubsidiary affiliateAndSubsidiary);
        Task<Result<AuthorisedPerson>> AddAuthorisedPersonAsync(AuthorisedPerson authorisedPerson);
        Task<IReadOnlyList<BoardOfDirector>> GetBoardOfDirectorByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<IndividualLegalEntity>> GetIndividualLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<CompanyLegalEntity>> GetCompanyLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IndividualLegalEntity> GetIndividualLegalEntityByTableIDAsync(long tableID);
        Task<CompanyLegalEntity> GetCompanyLegalEntityByTableIDAsync(long tableID);
        Task<IReadOnlyList<PrimaryOfficer>> GetPrimaryOfficerByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryAsync(Specification<DocumentCategory> documentCategorypec);
        Task<DocumentCommentUploadBP> GetReviewRemarkByCommentCode(long categoryBPCode);
        Task<DocumentCategory> GetDocumentCategoriesAsync(long documentCategoryCode);
        Task<DocumentCategory> GetDocumentCategoriesMappingTCAsync(int seqNo);
        Task<DocumentCategory> GetDocumentCategoriesMappingTBAsync(int seqNo);

        Task<IReadOnlyList<ParentHoldingCompany>> GetParentHoldingCompanyByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<Result<DocumentUploadBP>> UploadDocumentsAsync(DocumentUploadBP documentUpload);
        Task UploadDocumentAsync(DocumentUploadBP documentUpload);
        Task<List<EmailRecipient>> GetRecipientEmail(long bccType, long notificationTemplate);
        Task<List<EmailRecipient>> GetRecipientEmailByCollectionTier(long collectionTierCode, long recipientType, long notificationTemplate);
        Task<List<EmailRecipient>> GetRecipientEmailByAuthorityLevel(long authorityLevelCode, long recipientTypeCode, long notificationTemplateCode);
        Task<IReadOnlyList<Shareholder>> GetShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<LegalEntity>> GetLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile);

        Task<IReadOnlyList<IndividualShareholder>> GetIndividualShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<CompanyShareholder>> GetCompanyShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IndividualShareholder> GetIndividualShareholderByTableIDAsync(long tableID);
        Task<CompanyShareholder> GetCompanyShareholderByTableIDAsync(long tableID);
        Task<IReadOnlyList<PoliticallyExposedPerson>> GetPoliticallyExposedPersonByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<AffiliateAndSubsidiary>> GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<AuthorisedPerson>> GetAuthorisedPersonByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<Result<BoardOfDirector>> UpdateBoardOfDirectorAsync(BoardOfDirector boardOfDirector, CancellationToken cancellationToken);
        Task<Result<IndividualLegalEntity>> UpdateLegalEntityAsync(IndividualLegalEntity legalEntity, CancellationToken cancellationToken);
        Task<Result<CompanyLegalEntity>> UpdateLegalEntityAsync(CompanyLegalEntity legalEntity, CancellationToken cancellationToken);
        Task<Result<PrimaryOfficer>> UpdatePrimaryOfficerAsync(PrimaryOfficer primaryOfficer, CancellationToken cancellationToken);
        Task<IReadOnlyList<DocumentCategory>> GetBusinessProfilesAsync(object categoryProfileSpec);
        Task<Result<ParentHoldingCompany>> UpdateParentHoldingCompanyAsync(ParentHoldingCompany parentHoldingCompany, CancellationToken cancellationToken);
        Task<Result<Shareholder>> UpdateShareholderAsync(Shareholder shareholder, CancellationToken cancellationToken);
        Task<Result<LegalEntity>> UpdateLegalEntityAsync(LegalEntity legalEntity, CancellationToken cancellationToken);
        Task<Result<PoliticallyExposedPerson>> UpdatePoliticallyExposedPersonAsync(PoliticallyExposedPerson politicallyExposedPerson, CancellationToken cancellationToken);
        Task<Result<AffiliateAndSubsidiary>> UpdateAffiliateAndSubsidiaryAsync(AffiliateAndSubsidiary affiliateAndSubsidiary, CancellationToken cancellationToken);
        Task<Result<AuthorisedPerson>> UpdateAuthorisedPerson(AuthorisedPerson authorisedPerson, CancellationToken cancellationToken);

        Task<AuthorisedPerson> GetAuthorisedPersonByDefaultAsync(BusinessProfile businessProfile);
        Task DeleteAffiliateAndSubsidiaryAsync(IEnumerable<AffiliateAndSubsidiary> affiliateAndSubsidiary, CancellationToken cancellationToken);
        Task DeleteBoardOfDirectorAsync(IEnumerable<BoardOfDirector> boardOfDirector, CancellationToken cancellationToken);
        Task DeleteIndividualLegalEntityAsync(IEnumerable<IndividualLegalEntity> legalEntity, CancellationToken cancellationToken);
        Task DeleteCompanyLegalEntityAsync(IEnumerable<CompanyLegalEntity> legalEntity, CancellationToken cancellationToken);
        Task DeleteParentHoldingCompanyAsync(IEnumerable<ParentHoldingCompany> parentHoldingCompany, CancellationToken cancellationToken);
        Task DeletePoliticallyExposedPersonAsync(IEnumerable<PoliticallyExposedPerson> politicallyExposedPerson, CancellationToken cancellationToken);
        Task DeletePrimaryOfficerAsync(IEnumerable<PrimaryOfficer> primaryOfficer, CancellationToken cancellationToken);
        Task DeleteAuthorisedPersonAsync(IEnumerable<AuthorisedPerson> authorisedPerson, CancellationToken cancellationToken);

        Task DeleteShareholderAsync(IEnumerable<Shareholder> shareholder, CancellationToken cancellationToken);
        Task DeleteLegalEntityAsync(IEnumerable<LegalEntity> legalEntity, CancellationToken cancellationToken);

        Task<Declaration> GetKYCDeclarationInfoAsync(int BusinessProfileCode);
        Task<BusinessUserDeclaration> GetKYCBusinessDeclarationInfoAsync(int BusinessProfileCode);
        Task<Declaration> InsertKYCDeclarationInfoAsync(Declaration declaration);
        Task<Declaration> UpdateKYCDeclarationInfoAsync(Declaration declaration);
        Task<bool> SubmitKYCAsync(BusinessProfile BusinessProfile, List<DocumentCategoryBP> documentCategoryBPs);
        Task<bool> SubmitBusinessKYCAsync(BusinessProfile BusinessProfile, List<DocumentCategoryBP> documentCategoryBPs);

        Task<DocumentCategoryBP> GetDocumentCategoryBPAMLCFTAsync(BusinessProfile businessProfile, Solution solution, CustomerType customerType);
        Task<AMLCFTQuestionnaireAnswer> GetAMLCFTQuestionnaireAnswerAsync(BusinessProfile businessProfile);
        Task<Questionnaire> GetQuestionnaireByCodeAsync(long questionnaireCode);
        Task<IEnumerable<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswersByQuestionnaireAsync(AMLCFTQuestionnaire aMLCFTQuestionnaire);
        Task<DocumentUploadBP> GetDocumentUploadBPAsync(DocumentCategoryBP documentCategoryBP);
        Task<AMLCFTDisplayRules> AddAMLCFTDisplayRules(AMLCFTDisplayRules aMLCFTDisplayRules);
        Task<AMLCFTDisplayRules> UpdateAMLCFTDisplayRules(AMLCFTDisplayRules aMLCFTDisplayRules);
        Task<long> GetDocumentCategoryGroupIdBySolution(long? solutionCode);
        Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryByGroupId(long documentCategoryGroupId);
        Task<RequisitionRunningNumber> GetRequisitionRunningNumberLatest();
        Task<RequisitionRunningNumber> AddRequisitionRunningNumber(RequisitionRunningNumber requisitionRunningNumber);
        Task<RequisitionRunningNumber> UpdateRequisitionRunningNumber(RequisitionRunningNumber requisitionRunningNumber);
        #region AML/CFT Add and Updates

        Task<AMLCFTQuestionnaire> AddAMLCFTQuestionnaireQuestionsAsync(AMLCFTQuestionnaire amlCFTQuestionnaire, CancellationToken cancellationToken);
        Task<AMLCFTQuestionnaireAnswer> AddAMLCFTQuestionnaireAnswersAsync(AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer, CancellationToken cancellationToken);
        Task<AMLCFTQuestionnaireAnswer> UpdateAMLCFTQuestionnaireAnswersAsync(AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer, CancellationToken cancellationToken);
        Task DeleteAMLCFTQuestionnaireAnswersAsync(IEnumerable<AMLCFTQuestionnaireAnswer> amlCFTQuestionnaireAnswers, CancellationToken cancellationToken);
        Task DeleteAMLCFTQuestionnairesAsync(IEnumerable<AMLCFTQuestionnaire> amlCFTQuestionnaires, CancellationToken cancellationToken);
        Task<IReadOnlyList<AMLCFTQuestionnaire>> GetAMLCFTQuestionnairesByBusinessProfileAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswersByBusinessProfileAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<AMLCFTQuestionnaire>> GetAMLCFTQuestionnairesByBusinessProfileWithIgnoreQuestionnaireAsync(BusinessProfile businessProfile, List<long> ignoreQuestionnaire);
        Task<IReadOnlyList<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswersByBusinessProfileWithIgnoreQuestionnaireAsync(BusinessProfile businessProfile, List<long> ignoreQuestionnaire);

        #endregion

        Task<AMLCFTQuestionnaire> GetAMLCFTQuestionAsync(BusinessProfile businessProfile, Question question);
        Task<AMLCFTQuestionnaireAnswer> GetAMLCFTAnswerAsync(AMLCFTQuestionnaire amlCFTQuestionnaire, AnswerChoice answerChoice, string answerRemark);
        Task<ShareholderCompanyLegalEntity> GetShareholderCompanyLegalEntityByLegalEntityCodeAsync(long id);
        Task<Question> GetAMLCFTQuestionByQuestionCodeAsync(int questionCode);

        Task<ShareholderCompanyLegalEntity> DeleteShareholderLegalEntityAsync(ShareholderCompanyLegalEntity deleteCompanyLegalEntity, CancellationToken cancellationToken);
        Task<AnswerChoice> GetAMLCFTAnswerChoiceAsync(int answerChoiceCode);

        Task<Questionnaire> GetQuestionnaireByQuestionnaireCodeAsync(long questionnaireCode);
        Task<QuestionnaireSolution> GetQuestionnaireSolutionsByQuestionnaireCodeAsync(long questionnaireCode);
        Task<ShareholderCompanyLegalEntity> AddShareholderLegalEntityAsync(ShareholderCompanyLegalEntity shareholderCompanyLegalEntity);
        Task<QuestionnaireSolution> GetQuestionnaireSolutionByQuestionnaireAndSolutionAsync(Questionnaire questionnaireCode, Solution solutionCode);
        Task<List<QuestionSection>> GetQuestionSectionsByQuestionnaireCodeAsync(long questionnaireCode);
        Task<QuestionSection> GetQuestionSectionByQuestionSectionCodeAsync(long questionSectionCode);
        Task<List<Question>> GetQuestionsByQuestionSectionAsync(List<QuestionSection> questionSection);
        Task<Result<Questionnaire>> UpdateQuestionnaireStatusAsync(Questionnaire questionnaire);
        Task<Result<Questionnaire>> AddOrUpdateQuestionnaireAsync(Questionnaire questionnaire, int action);
        Task<IReadOnlyList<ShareholderIndividualLegalEntity>> GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<IReadOnlyList<ShareholderCompanyLegalEntity>> GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile);
        Task<Result<List<QuestionnaireSolution>>> UpdateQuestionnaireSolutionAsync(List<QuestionnaireSolution> questionnaire);
        Task<Result<List<QuestionSection>>> AddOrUpdateQuestionSectionAsync(List<QuestionSection> questionSections, int action);
        Task<Result<List<Question>>> AddOrUpdateQuestionsAsync(List<Question> questions, int action);
        Task<Result<List<AnswerChoice>>> AddOrUpdateAnswerChoicesAsync(List<AnswerChoice> answerChoices, int action);
        Task<Result<List<QuestionSection>>> DeleteQuestionSectionsAsync(List<QuestionSection> questionSections);
        Task<ShareholderIndividualLegalEntity> AddShareholderLegalEntityAsync(ShareholderIndividualLegalEntity shareholderIndividualLegalEntity);
        Task<Result<List<QuestionnaireSolution>>> DeleteQuestionnaireSolutionAsync(List<QuestionnaireSolution> questionnaireSolutions);
        Task<Result<List<QuestionnaireSolution>>> AddQuestionnaireSolutionAsync(List<QuestionnaireSolution> questionnaireSolution);
        Task<Question> GetQuestionByQuestionCodeAsync(long questionCode);
        Task<List<AnswerChoice>> GetAnswerChoicesByQuestionAsync(List<Question> questions);
        Task<AnswerChoice> GetAnswerChoiceByAnswerChoiceCodeAsync(long answerChoiceCode);
        Task<List<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswerByAnswerChoiceAsync(List<AnswerChoice> answerChoices);
        Task<Result<List<AMLCFTQuestionnaireAnswer>>> DeleteAMLCFTQuestionnaireAnswersByAnswerChoiceAsync(List<AnswerChoice> answerChoices);
        Task<List<AMLCFTQuestionnaire>> GetAMLCFTQuestionnaireByQuestionAsync(List<Question> questions);
        Task<Result<List<AMLCFTQuestionnaire>>> DeleteAMLCFTQuestionnairesByQuestionAsync(List<Question> questions);
        Task<bool> DeleteAnswerChoicesAsync(List<AnswerChoice> answerChoices);
        Task<bool> DeleteQuestionsAsync(List<Question> questions);
        Task<bool> DeleteAMLCFTQuestionnaireAnswerAsync(List<AMLCFTQuestionnaireAnswer> amlCFTQuestionnaireAnswers);

        Task<COInformation> GetCOInfoByBusinessCode(int businessProfileCode);
        Task<List<ShareholderCompanyLegalEntity>> GetShareholderCompanyLegalEntity(long id);
        Task<AMLCFTDisplayRules> GetAMLCFTDisplayRulesByCodeAsync(long displayRuleCode);
        Task<Result<AMLCFTDisplayRules>> DeleteAMLCFTDisplayRulesAsync(AMLCFTDisplayRules displayRules);
        Task<List<ShareholderIndividualLegalEntity>> GetShareholderIndividualLegalEntity(long id);
        Task<LicenseInformation> GetLicenseInfoByBusinessCode(int businessProfileCode);

        Task<bool> AddKYCSubmoduleReviews(List<KYCSubModuleReview> kYCSubModuleReviews);

        Task<KYCSubModuleReview> SaveKYCSubModuleReview(KYCSubModuleReview kYCSubModuleReview);
        Task<ShareholderIndividualLegalEntity> DeleteShareholderIndividualLegalEntityAsync(ShareholderIndividualLegalEntity shareholderIndividualLegalEntity, CancellationToken cancellationToken);
        Task<List<KYCSubModuleReview>> GetKYCSubModuleReviewByBusinessProfile(BusinessProfile businessProfile);
        Task<KYCSubModuleReview> GetKYCSubModuleReviewByBusinessProfileCategory(BusinessProfile businessProfile, KYCCategory kYCCategory);
        Task<KYCSubModuleReview> GetKYCSubModuleReviewByBusinessProfileCategoryNoTracking(BusinessProfile businessProfile, KYCCategory kYCCategory);
        Task<bool> SaveKYCSubModuleReviewList(BusinessProfile businessProfile, List<KYCSubModuleReview> kycSubModuleReviewUpdateList);
        Task<Result<KYCSummaryFeedback>> SaveKYCSummaryFeedback(KYCSummaryFeedback kycSummaryFeedback);
        Task<Result<KYCCustomerSummaryFeedback>> SaveKYCCustomerSummaryFeedback(KYCCustomerSummaryFeedback kycCustomerSummaryFeedback);
        Task<Result<CustomerUser>> EditIsTPNUser(CustomerUser customerUser);
        Task<List<Solution>> GetSolutionsByUserAsync(long userId);

        //Phase 3 Changes 
        Task<CustomerType> GetCustomerTypeByCode(long? customerTypeCode);
        Task<ServiceType> GetServiceTypeByCode(long? serviceTypeCode);
        Task<CollectionTier> GetCollectionTierByCode(long? collectionTierCode);
        Task<IDType> GetIDTypeByCode(long? IDTypeCode);
        Task<AuthorisationLevel> GetAuthorisationLevelCodeByCode(long? authorisationLevelCode);

        Task<List<KYCCategory>> GetKYCConnectCategories();
        Task<List<KYCCategory>> GetKYCBusinessCategories();
        Task<RelationshipTieUp> GetRelationshipTieUpByCodeAsync(long? relationshipTieUpCode);
        Task<IncorporationCompanyType> GetIncorporationCompanyTypeByCodeAsync(long? incorporationCompanyTypeCode);
        Task<BusinessNature> GetBusinessNatureByCodeAsync(long? businessNatureCode);
        Task<ServicesOffered> GetServicesOfferedByCodeAsync(long? servicesOfferedTypeCode);
        Task<EntityType> GetEntityTypeByCodeAsync(long? entityTypeCode);

        Task<List<DeclarationQuestion>> GetDeclarationQuestionsByCustomerTypeAsync(long customerTypeCode);
        Task<List<CustomerBusinessDeclarationAnswer>> AddCustomerBusinessDeclarationAnswersAsync(List<CustomerBusinessDeclarationAnswer> customerBusinessDeclarationAnswers);
        Task<CustomerBusinessDeclaration> AddCustomerBusinessDeclarationAsync(CustomerBusinessDeclaration customerBusinessDeclaration);
        Task<CustomerBusinessDeclarationAnswer> GetCustomerBusinessDeclarationAnswerByCodeAsync(long customerBusinessDeclarationAnswerCode);
        Task<CustomerBusinessDeclarationAnswer> UpdateCustomerBusinessDeclarationAnswerAsync(CustomerBusinessDeclarationAnswer customerBusinessDeclarationAnswer);
        Task<List<CustomerBusinessDeclarationAnswer>> GetCustomerBusinessDeclarationAnswersByCodeAsync(long customerBusinessDeclarationCode);
        Task<List<CustomerBusinessDeclarationAnswer>> UpdateCustomerBusinessDeclarationAnswersAsync(List<CustomerBusinessDeclarationAnswer> customerBusinessDeclarationAnswers);
        Task DeleteCustomerBusinessDeclarationAnswersAsync(List<CustomerBusinessDeclarationAnswer> customerBusinessDeclarationAnswers);
        Task<CustomerBusinessDeclaration> GetCustomerBusinessDeclarationByCodeAsync(long customerBusinessDeclarationCode);
        Task<CustomerBusinessDeclaration> UpdateCustomerBusinessDeclarationAsync(CustomerBusinessDeclaration customerBusinessDeclaration);
        Task<ShareholderCompanyLegalEntity> UpdateShareholderCompanyLegalEntityAsync(ShareholderCompanyLegalEntity a);
        Task<List<BusinessDeclarationRejectionMatrix>> GetRejectionMatrixesByCustomerTypeAsync(long customerTypeCode);
        Task<BusinessDeclarationStatus> GetBusinessDeclarationStatus(long businessDeclarationStatusCode);

        Task<List<CustomerBusinessTransactionEvaluationAnswer>> GetCustomerBusinessTransactionEvaluationAnswersAsync(int businessProfileCode);
        Task<CustomerBusinessTransactionEvaluationAnswer> GetCustomerBusinessTransactionEvaluationAnswersByIdAsync(int id);
        Task<List<CustomerBusinessTransactionEvaluationAnswer>> AddCustomerBusinessTransactionEvaluationAnswer(List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswers);
        Task<TransactionEvaluationAnswerChoice> GetTransactionEvaluationAnswerChoiceAsync(int transactionEvaluationAnswerChoiceCode);
        void DeleteCustomerBusinessTransactionEvaluationAnswersByBusinessProfileCodeAsync(List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswers);
        Task<TransactionEvaluationQuestion> GetTransactionEvaluationQuestionAsync(int transactionEvaluationQuestionCode);
        void DeleteCustomerBusinessTransactionEvaluationAnswer(List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswers);

        Task<Gender> GetGenderTypeByCode(long? genderTypeCode);
        Task<ShareholderIndividualLegalEntity> UpdateShareholderIndividualLegalEntityAsync(ShareholderIndividualLegalEntity b);
        Task<CountryMeta> GetCountryMetaByISO2CodeAsync(string countryCodeISO2);
        Task<KYCStatus> GetKYCStatusByCodeAsync(long? kycStatusCode);
        Task<Solution> GetSolutionByCodeAsync(long? solutionCode);
        Task<long?> GetSolutionByNameAsync(string solution);

        Task<List<KYCCategory>> GetKYCCategoriesByCustomerTypeGroupCodeAsync(int customerTypeGroupCode);

        Task<BoardOfDirector> GetBoardOfDirectorByCodeAsync(long boardOfDirectorCode);
        Task<PrimaryOfficer> GetPrimaryOfficerByCodeAsync(long primaryOfficerCode);
        Task<AuthorisedPerson> GetAuthorisedPersonByCodeAsync(long authorisedPersonCode);

        //Verification Changes
        Task<IEnumerable<VerificationIDType>> GetAllVerificationIDTypes();
        Task<IEnumerable<VerificationIDTypeSection>> GetAllVerificationIDTypeSections();
        Task<IEnumerable<VerificationStatus>> GetAllVerificationStatus();
        Task<IEnumerable<SubmissionResult>> GetAllSubmissionResult();
        Task<IEnumerable<RiskScore>> GetAllRiskScores();
        Task<IEnumerable<RiskType>> GetAllRiskTypes();
        Task<CustomerVerification> GetCustomerVerificationbyBusinessProfileCodeAsync(int businessProfileCode);
        Task<VerificationStatus> GetVerificationStatusByCodeAsync(long? verificationStatusCode);
        Task<VerificationIDType> GetVerificationIDByCodeAsync(long? verificationIDCode);
        Task<RiskScore> GetRiskScoreByCodeAsync(long? riskScoreCode);
        Task<RiskType> GetRiskTypeByCodeAsync(long? riskTypeCode);
        Task<CustomerVerification> AddCustomerVerificationAsync(CustomerVerification customerVerification);
        Task<CustomerVerification> UpdateCustomerVerificationAsync(CustomerVerification customerVerification);
        Task<List<CustomerVerificationDocuments>> DeleteCustomerVerificationDocumentsByCustomerVerificationCodeAsync(long? customerVerificationCode);
        Task<CustomerVerification> GetCustomerVerificationbyCustomerVerificationCodeAsync(long? customerVerification);
        Task<List<CustomerVerificationDocuments>> GetCustomerVerificationDocumentbyCustomerVerificationCodeAsync(long? customerVerification);
        Task<VerificationIDTypeSection> GetVerificationIDTypeSectionsByCode(long? verificationIDTypeCode);
        Task<CustomerVerificationDocuments> AddCustomerVerificationDocumentsAsync(CustomerVerificationDocuments customerVerificationDocuments);
        Task<List<CustomerVerificationDocuments>> GetCustomerVerificationDocumentsByVerificationCodeAsync(long? customerVerificationCode);
        Task<CustomerVerificationDocuments> GetCustomerVerificationDocumentByDocumentIdAsync(Guid? documentId);
        Task<CustomerVerificationDocuments> DeleteCustomerVerificationDocumentAsync(long? customerVerificationDocumentCode);
        Task<CustomerVerification> DeleteCustomerVerificationTemplateAsync(long? customerVerificationCode);
        Task<CustomerVerificationDocuments> GetCustomerVerificationDocumentsByCustomerVerificationDocumentCode(long? customerVerificationDocumentCode);
        Task<CustomerVerificationDocuments> UpdateCustomerVerificationDocumentsAsync(CustomerVerificationDocuments customerVerificationDocuments);
        Task<CustomerVerificationDocuments> GetCustomerVerificationDocumentUploadByVerificationCodeAsync(long? customerVerificationCode);

        //Sprint 6
        Task<BusinessUserDeclaration> GetBusinessUserDeclarationByBusinessProfileCodeAsync(long? businessProfileCode);
        Task<BusinessUserDeclaration> GetBusinessUserDeclarationByBusinessUserDeclarationCode(long? businessUserDeclarationCode);
        Task<Declaration> GetCustomerConnectDeclarationByDeclarationCode(long? declarationCode);
        Task<BusinessUserDeclaration> AddBusinessUserDeclarationAsync(BusinessUserDeclaration businessUserDeclaration);
        Task<BusinessUserDeclaration> UpdateBusinessUserDeclarationAsync(BusinessUserDeclaration businessUserDeclaration);
        Task<BusinessUserDeclaration> DeleteBusinessUserDeclarationSignatureAsync(Guid? businessUserDeclaration);
        Task<Declaration> DeleteConnectUserDeclarationSignatureAsync(Guid? businessUserDeclaration);
        Task<CustomerBusinessDeclaration> GetCustomerBusinessDeclarationByBusinessProfileCode(long? businessProfileCode);
        Task<SubmissionResult> GetSubmissionResultByCode(long submissionResultCode);
        Task<Result<List<CustomerVerificationDocuments>>> UpdateCustomerVerificationDocumentsListAsync(List<CustomerVerificationDocuments> customerVerificationDocuments);

        Task<KYCSubmissionStatus> GetBusinessKYCSubmissionStatusBySubmissionStatusCode(long? submissionStatusCode);
        Task<List<DocumentCategoryBP>> GetDocumentCategoryBPsByBusinessProfileCodeAsync(int businessProfileCode);
        Task<List<DocumentUploadBP>> GetDocumentUploadBPsAsync(long documentCategoryBPCode);
        Task<List<DocumentUploadBP>> DeleteDocumentUploadBPs(List<DocumentUploadBP> documentUploadBPs);
        Task<List<DocumentCategoryBP>> DeleteDocumentCategoryBPs(List<DocumentCategoryBP> documentCategoryBPs);
        Task<List<ParentHoldingCompany>> GetParentHoldingCompanyListAsync(int businessProfileCode);
        Task<List<PrimaryOfficer>> GetPrimaryOfficerListAsync(int businessProfileCode);
        Task<List<AffiliateAndSubsidiary>> GetAffiliateAndSubsidiaryListAsync(int businessProfileCode);
        Task<List<LegalEntity>> GetLegalEntityListAsync(int businessProfileCode);
        Task<LegalEntity> GetLegalEntityAsync(int businessProfileCode);
        Task<List<KYCSubModuleReview>> GetKYCSubModuleReviewList(int businessProfileCode);
        Task<List<KYCSubModuleReview>> DeleteKYCSubModuleReview(List<KYCSubModuleReview> kYCSubModuleReviews, int businessProfileCode);
        Task<List<KYCSubModuleReview>> AddKYCSubModuleReviewAsync(List<KYCSubModuleReview> kYCSubModuleReview);
        Task<ReviewResult> GetReviewResultAsync(long reviewResultCode);

        //Screening List 
        Task<IEnumerable<ScreeningInput>> GetScreeningInputsByBusinessProfileIdAsync(long? businessProfileCode);
        Task<Result<ScreeningInput>> UpdateSingleScreeningInputAsync(ScreeningInput screeningInput);
        Task<Result<List<ScreeningInput>>> UpdateScreeningInputAsync(List<ScreeningInput> screeningInputs);
        Task<Result<List<ScreeningInput>>> AddScreeningInputAsync(List<ScreeningInput> screeningInputs);
        Task<IEnumerable<ScreeningDetail>> GetScreeningDetailsByScreeningInputCode(long? screeningInputCode);
        Task<Result<List<Screening>>> SaveScreening(List<Screening> screening);
        Task<ScreeningInput> GetScreeningInputByClientReferenceAsync(string clientReference);
        Task<WatchlistStatus> GetWatchlistStatusByWatchlistStatusCode(long? watchlistStatusCode);
        Task<Result<List<ScreeningDetail>>> SaveScreeningDetails(List<ScreeningDetail> screeningDetails);
        Task<Screening> GetScreeningByReferenceCodeAsync(Guid referenceCode);
        Task<ScreeningDetail> GetScreeningDetailByEntityIdAsync(long screeningId, long entityId);
        Task<ScreeningDetail> UpdateScreeningDetails(ScreeningDetail screeningDetails);
        Task<OwnershipStrucureType> GetOwnershipStructureTypeByCode(long? ownershipStructureTypeCode);
        Task<ScreeningEntityType> GetScreeningEntityTypeByCode(long? screeningEntityTypeCode);
        Task<List<Screening>> GetLatestScreeningByScreeningInputCodes(List<int> screeningInputCodes);
        Task<CompanyUserAccountStatus> GetCompanyUserAccountStatus(long? companyUserAccountStatusCode);
        Task<CompanyUserBlockStatus> GetCompanyUserBlockStatus(long? companyUserBlockStatusCode);
        Task<List<ChangeCustomerTypeDocumentUploadBP>> AddChangeCustomerTypeDocumentUploadBPs(List<ChangeCustomerTypeDocumentUploadBP> changeCustomerTypeDocumentUploadBPs);
        Task<ChangeCustomerTypeLicenseInformation> AddChangeCustomerTypeLicenseInformation(ChangeCustomerTypeLicenseInformation changeCustomerTypeLicenseInformation);
        Task<ChangeCustomerTypeCOInformation> AddChangeCustomerTypeCOInformation(ChangeCustomerTypeCOInformation changeCustomerTypeCOInformation);
        Task<DefaultTemplateDocument> GetDefaultTemplateDocumentAsync(long defaultTemplateCode);
        Task<DefaultTemplateDocument> AddDefaultTemplateDocumentAsync(DefaultTemplateDocument defaultTemplateDocument);
        Task<DefaultTemplateDocument> UpdateDefaultTemplateDocumentAsync(DefaultTemplateDocument defaultTemplateDocument);
        //Concurrency
        Task<DateTime?> GetLastReviewConcurrentModifiedTimestamp(long businessProfileCode);
        Task<Shareholder> UpdateShareholdersPrimaryOfficerReferenceAsync(long? primaryOfficerCode);
        Task<QuestionManagement> SaveAdminTemplateManagement(QuestionManagement questionManagement);
        Task<QuestionManagement> GetAdminTemplateManagement(QuestionManagement questionManagement);
        Task<QuestionManagement> UpdateAdminTemplateManagement(QuestionManagement questionManagement);
        Task UpdateKYCCustomerSummaryFeedbackNotificationsAsReadAsync(Specification<KYCCustomerSummaryFeedbackNotification> specification, CancellationToken cancellationToken);
        Task<Result<KYCCustomerSummaryFeedbackNotification>> InsertKYCCustomerSummaryFeedbackNotificationAsync(KYCCustomerSummaryFeedbackNotification notification, CancellationToken cancellationToken);
        Task UpdateKYCSummaryFeedbackNotificationsAsReadByCategoryAsync(Specification<KYCSummaryFeedbackNotification> specification, CancellationToken cancellationToken);
        Task<Result<KYCSummaryFeedbackNotification>> InsertKYCSummaryFeedbackNotificationAsync(KYCSummaryFeedbackNotification notification, CancellationToken cancellationToken);
        Task<List<KYCSummaryFeedback>> GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(int businessProfileCode);
        Task<Title> GetTitleTypeByCode(long? titleTypeCode);
        Task SaveChangesAsync();
    }
}