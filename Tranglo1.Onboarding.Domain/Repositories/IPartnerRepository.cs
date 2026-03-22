using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Events;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IPartnerRepository
    {

        Task<Result<PartnerRegistration>> AddPartnerRegistrationAsync(PartnerRegistration partnerRegistration);
        Task<List<PartnerRegistration>> GetPartnerRegistrationAsync(List<CustomerUserBusinessProfile> customerUserBusinessProfiles);
        Task<PartnerAgreementTemplate> AddPartnerAgreementTemplateUploadAsync(PartnerAgreementTemplate partnerAgreementTemplate);
        Task<PartnerAgreementTemplate> RemovePartnerAgreementTemplateAsync(PartnerAgreementTemplate partnerAgreementTemplate);
        Task<List<PartnerAgreementTemplate>> GetPartnerAgreementTemplatesAsync(long partnerCode);
        Task<PartnerAgreementTemplate> GetPartnerAgreementTemplateByTemplateIdAsync(PartnerAgreementTemplate partnerAgreementTemplate);
        Task<SignedPartnerAgreement> AddSignedPartnerAgreementUploadAsync(SignedPartnerAgreement signedPartnerAgreement);
        Task<SignedPartnerAgreement> RemoveSignedPartnerAgreementAsync(SignedPartnerAgreement signedPartnerAgreement);
        Task<SignedPartnerAgreement> UpdateSignedPartnerAgreementAsync(SignedPartnerAgreement signedPartnerAgreement);
        Task<List<SignedPartnerAgreement>> GetSignedPartnerAgreementsAsync(long partnerCode);
        Task<SignedPartnerAgreement> GetSignedPartnerAgreementBySignedDocumentIdAsync(SignedPartnerAgreement signedPartnerAgreement);
        Task<PartnerRegistration> GetPartnerDetailsByCodeAsync(long partnerCode);
        Task<IEnumerable<PartnerType>> GetPartnerTypes(Specification<PartnerType> spec);
        Task<PartnerRegistration> GetPartnerAgreementDetailsByPartnerCodeAsync(long partnerCode);
        Task<List<KYCCategoryCustomerType>> GetKYCBusinessCategoryByCustomerTypeGroupCodeAsync(int customerTypeGroupCode);
        Task<PartnerRegistration> UpdatePartnerAgreementDetailsAsync(PartnerRegistration partnerRegistration);
        Task<PartnerRegistration> GetPartnerRegistrationByCodeAsync(long partnerCode);
        Task<Result<PartnerRegistration>> UpdatePartnerRegistrationAsync(PartnerRegistration partnerRegistration);
        Task<Result<PartnerSubscription>> UpdatePartnerSubscriptionsAsync(PartnerSubscription partnerSubscription);
        Task<Result<PartnerSubscription>> DeleteSubcriptionAsync(PartnerSubscription partnerSubscription);
        Task<HelloSignDocument> AddHelloSignDocumentAsync(HelloSignDocument helloSignDocument);
        Task<HelloSignDocument> RemoveHelloSignDocumentAsync(HelloSignDocument helloSignDocument);
        Task<List<HelloSignDocument>> GetHelloSignDocumentsAsync(long partnerCode);
        Task<PartnerRegistration> GetPartnerInfoByCodeForSignUpCode(long partnerCode);
        Task<HelloSignDocument> GetHelloSignDocumentByHelloSignDocumentIdAsync(long helloSignDocumentId);
        Task<PartnerRegistration> GetPartnerRegistrationByBusinessProfileCodeAsync(int businessProfileCode);
        Task<PartnerRegistration> GetPartnerRegistrationCodeByBusinessProfileCodeAsync(int businessProfileCode);

        Task<IEnumerable<ChangeType>> GetPartnerAccountStatusChangeTypeAsync();
        Task<IEnumerable<PartnerAccountStatusType>> GetPartnerAccountStatusTypeAsync();
        Task<IEnumerable<OnboardWorkflowStatus>> GetOnboardWorkflowStatusAsync();
        Task<PartnerAPISetting> AddPartnerAPISettingAsync(PartnerAPISetting partnerAPISetting);
        Task<WhitelistIP> AddWhitelistIPAsync(WhitelistIP whitelistIP);
        Task<PartnerAPISetting> UpdatePartnerAPISettingAsync(PartnerAPISetting partnerAPISetting);
        Task<APIURL> GetApiUrlAsync(int env, APIType aPIType);
        Task<WhitelistIP> UpdateWhitelistIPAsync(WhitelistIP whitelistIP);
        Task<WhitelistIP> GetWhitelistIPAsync(long whitelistIPId);
        Task<PartnerAPISetting> GetPartnerAPISettingAsync(long APISettingId);
        Task<List<PartnerAPISetting>> GetPartnerAPISettingByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode);
        Task<List<PartnerAPISetting>> GetPartnerAPISettingByAPIUserIdAsync(long partnerCode, string apiUserId);
        Task<List<PartnerAPISetting>> GetPartnerAPISettingBySecretKeyAsync(long partnerCode, string secretKey);
        Task<WhitelistIP> GetWhitelistIPByIPAddressRangedAsync(long partnerSubscriptionCode, string iPAddressStart, string iPAddressEnd);
        Task<WhitelistIP> GetWhitelistIPByIPAddressAsync(long partnerSubscriptionCode, string iPAddressStart);
        Task<PartnerCMSIntegrationDetail> GetPartnerCMSIntegrationByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode);
        Task<PartnerCMSIntegrationDetail> AddPartnerCMSIntegration(PartnerCMSIntegrationDetail partnerCMSIntegrationDetail);
        Task<PartnerCMSIntegrationDetail> UpdatePartnerCMSIntegration(PartnerCMSIntegrationDetail partnerCMSIntegrationDetail);
        Task<PartnerWalletCMSIntegrationDetail> GetPartnerWalletIntegrationByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode);
        Task<PartnerWalletCMSIntegrationDetail> AddPartnerWalletIntegration(PartnerWalletCMSIntegrationDetail partnerWalletDetail);
        Task<PartnerWalletCMSIntegrationDetail> UpdatePartnerWalletIntegration(PartnerWalletCMSIntegrationDetail partnerWalletDetail);
        Task<IEnumerable<PartnerWalletCMSIntegrationDetail>> GetPartnerWalletListByStatus(string status);
        Task<PartnerRegistration> GetPartnerRegistrationByEmail(string loginId);
        //Task<string> GetTrangloEntityByPartnerAsync(long partnerCode);
        Task<List<string>> GetTrangloEntitiesByPartnerAsync(long partnerCode);
        Task<PartnerRegistration> GetPartnerRegistrationByPartnerId(Guid partnerId);
        Task<Result<PartnerProfileChangedEvent>> AddPartnerOnboardingCreationEventAsync(PartnerProfileChangedEvent partnerOnboarding);
        Task<PartnerProfileChangedEvent> GetPartnerOnboardingEventAsync(long partnerSubscriptionCode);
        Task<Result<PartnerSubscription>> AddSubcriptionAsync(PartnerSubscription partnerSubscription);
        Task<Result<PartnerSubscription>> UpdateSubcriptionAsync(PartnerSubscription partnerSubscription);
        Task<PartnerSubscription> GetSubscriptionAsync(long partnerSubscriptionCode);
        Task<List<PartnerSubscription>> GetSubscriptionsByPartnerCodeAsync(long partnerCode);
        Task<List<PartnerSubscription>> GetSubscriptionAsync(long partnerCode, string trangloEntity);
		Task<List<PartnerSubscription>> GetSubscriptionWithNullEntityAsync(long partnerCode, string trangloEntity);
		Task<CountryMeta> GetCountryAsync(string countryISO2);
        Task<PartnerType> GetPartnerTypeAsync(long partnerTypeCode);
        Task<Solution> GetSolutionAsync(long solutionCode);
        Task<PartnerAccountStatusType> GetPartnerAccountStatusTypeAsync(long partnerAccountStatusTypeCode);
        Task<ChangeType> GetChangeTypeAsync(long changeTypeCode);
        Task<List<PartnerSubscription>> GetSalesPartnerSubscriptionListAsync(long partnerCode);
        Task<TrangloEntity> GetTrangloEntityMeta(string trangloEntity);
        Task<List<PartnerSubscription>> GetPartnerSubscriptionListAsync(long partnerCode);
        Task<PartnerSubscription> GetPartnerSubscriptionByCodeAsync(long partnerSubscriptionCode);
        Task<PartnerSubscription> GetPartnerSubscriptionByPartnerCodeAsync(long? partnerCode);
        Task<PartnerSubscription> GetPartnerSubcriptionByPartnerCodeAndSolutionCodeAsync(long partnerCode, int solutionCode);

        //Meta
        Task<CustomerType> GetCustomerTypeByCodeAsync(long? customerTypeCode);
        Task<RelationshipTieUp> GetRelationshipTieUpByCodeAsync(long? relationshipTieUpCode);
        Task<List<Solution>> GetSolutionsByPartnerAsync(long partnerCode);
        Task<CustomerType> GetCustomerTypeByBusinessProfileAsync(int businessProfileCode);
        Task<PartnerSubscription> GetPartnerUserTrangloEntityByCodeAsync(long? partnerCode, string customerSolution);
        Task<PartnerRegistration> GetPartnerRegistrationCodeByCodeAsync(long? partnerCode);
        Task<IEnumerable<KYCReminderStatus>> GetKYCReminderStatusesAsync();
        Task<PartnerAccountStatus> GetLatestPartnerAccountStatusByPartnerSubscription(long partnerSubscriptionCode);
        Task<List<PartnerSubscription>> GetPartnerSubscriptionsByBusinessProfileIdsAsync(HashSet<int> businessProfileIds);
    }
}