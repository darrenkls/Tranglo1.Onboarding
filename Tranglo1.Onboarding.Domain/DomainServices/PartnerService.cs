using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Domain.DomainServices
{
    public class PartnerService
    {
        private readonly IPartnerRepository partnerRepository;
        private readonly IBusinessProfileRepository businessProfileRepository;
        private readonly IStaffEntityQueryService staffEntityQueryService;

        protected IPartnerRepository Repository => partnerRepository;

		public PartnerService(
            IPartnerRepository partnerRepository,
             IBusinessProfileRepository businessProfileRepository,
             IStaffEntityQueryService staffEntityQueryService)
        {
            this.partnerRepository = partnerRepository;
            this.businessProfileRepository = businessProfileRepository;
            this.staffEntityQueryService = staffEntityQueryService;
        }

        public async Task<PartnerAgreementTemplate> AddPartnerAgreementTemplateUploadAsync(PartnerAgreementTemplate partnerAgreementTemplate)
        {
            return await partnerRepository.AddPartnerAgreementTemplateUploadAsync(partnerAgreementTemplate);
        }

        public async Task<PartnerAgreementTemplate> RemovePartnerAgreementTemplateAsync(PartnerAgreementTemplate partnerAgreementTemplate)
        {
            return await partnerRepository.RemovePartnerAgreementTemplateAsync(partnerAgreementTemplate);
        }

        public async Task<List<PartnerAgreementTemplate>> GetPartnerAgreementTemplatesAsync(long partnerCode)
        {
            return await partnerRepository.GetPartnerAgreementTemplatesAsync(partnerCode);
        }

        public async Task<PartnerAgreementTemplate> GetPartnerAgreementTemplateByTemplateIdAsync(PartnerAgreementTemplate partnerAgreementTemplate)
        {
            return await partnerRepository.GetPartnerAgreementTemplateByTemplateIdAsync(partnerAgreementTemplate);
        }

        public async Task<SignedPartnerAgreement> AddSignedPartnerAgreementUploadAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            return await partnerRepository.AddSignedPartnerAgreementUploadAsync(signedPartnerAgreement);
        }

        public async Task<SignedPartnerAgreement> RemoveSignedPartnerAgreementAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            return await partnerRepository.RemoveSignedPartnerAgreementAsync(signedPartnerAgreement);
        }

        public async Task<SignedPartnerAgreement> UpdateSignedPartnerAgreementAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            return await partnerRepository.UpdateSignedPartnerAgreementAsync(signedPartnerAgreement);
        }

        public async Task<List<SignedPartnerAgreement>> GetSignedPartnerAgreementsAsync(long partnerCode)
        {
            return await partnerRepository.GetSignedPartnerAgreementsAsync(partnerCode);
        }

        public async Task<SignedPartnerAgreement> GetSignedPartnerAgreementBySignedDocumentIdAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            return await partnerRepository.GetSignedPartnerAgreementBySignedDocumentIdAsync(signedPartnerAgreement);
        }

        public async Task<Result<PartnerRegistration>> AddPartnerRegistrationAsync(PartnerRegistration partnerRegistration)
        {
            return await partnerRepository.AddPartnerRegistrationAsync(partnerRegistration);
        }

        public async Task<HelloSignDocument> AddHelloSignDocumentAsync(HelloSignDocument helloSignDocument)
        {
            return await partnerRepository.AddHelloSignDocumentAsync(helloSignDocument);
        }

        public async Task<HelloSignDocument> RemoveHelloSignDocumentAsync(HelloSignDocument helloSignDocument)
        {
            return await partnerRepository.RemoveHelloSignDocumentAsync(helloSignDocument);
        }

        public async Task<List<HelloSignDocument>> GetHelloSignDocumentsAsync(long partnerCode)
        {
            return await partnerRepository.GetHelloSignDocumentsAsync(partnerCode);
        }

        public async Task<HelloSignDocument> GetHelloSignDocumentByHelloSignDocumentIdAsync(long helloSignDocumentId)
        {
            return await partnerRepository.GetHelloSignDocumentByHelloSignDocumentIdAsync(helloSignDocumentId);
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationCodeByBusinessProfileCodeAsync(int businessProfileCode)
        {
            return await partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(businessProfileCode);
        }

        public async Task<PartnerRegistration> GetPartnerAgreementDetailsAsync(long partnerCode)
        {
            return await partnerRepository.GetPartnerAgreementDetailsByPartnerCodeAsync(partnerCode);
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationByEmail(string loginId)
        {
            return await partnerRepository.GetPartnerRegistrationByEmail(loginId);
        }

        public async Task<PartnerRegistration> UpdatePartnerAgreementDetailsAsync(PartnerRegistration partnerRegistration)
        {
            return await partnerRepository.UpdatePartnerAgreementDetailsAsync(partnerRegistration);
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationByCodeAsync(long partnerCode)
        {
            return await partnerRepository.GetPartnerRegistrationByCodeAsync(partnerCode);
        }
        public async Task<List<PartnerAPISetting>> GetPartnerAPISettingByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode)
        {
            return await partnerRepository.GetPartnerAPISettingByPartnerSubscriptionCodeAsync(partnerSubscriptionCode);
        }
        public async Task<PartnerAPISetting> GetPartnerAPISettingByCodeAsync(long partnerAPISettingCode)
        {
            return await partnerRepository.GetPartnerAPISettingAsync(partnerAPISettingCode);
        }

        public async Task<Result<PartnerRegistration>> UpdatePartnerRegistrationAsync(PartnerRegistration partnerRegistration)
        {
            return await partnerRepository.UpdatePartnerRegistrationAsync(partnerRegistration);
        }

        public async Task<PartnerAPISetting> AddPartnerAPISettingAsync(PartnerAPISetting partnerAPISetting)
        {
            return await partnerRepository.AddPartnerAPISettingAsync(partnerAPISetting);
        }

        public async Task<Result<PartnerProfileChangedEvent>> AddPartnerOnboardingCreationEventAsync(PartnerProfileChangedEvent onboardingEvent)
        {
            return await partnerRepository.AddPartnerOnboardingCreationEventAsync(onboardingEvent);
        }


        public async Task<WhitelistIP> AddWhiteListAsync(WhitelistIP whitelistIP)
        {
            return await partnerRepository.AddWhitelistIPAsync(whitelistIP);
        }
        public async Task<PartnerAPISetting> UpdatePartnerAPISettingAsync(PartnerAPISetting partnerAPISetting)
        {
            return await partnerRepository.UpdatePartnerAPISettingAsync(partnerAPISetting);
        }
        public async Task<APIURL> GetApiUrlAsync(int env, APIType aPIType)
        {
            return await partnerRepository.GetApiUrlAsync(env, aPIType);
        }
        public async Task<Result<PartnerAPISetting>> UpdateIsConfiguredAsync(PartnerAPISetting partnerAPISetting)
        {
            return await partnerRepository.UpdatePartnerAPISettingAsync(partnerAPISetting);
        }

        public async Task<WhitelistIP> GetWhitelistIPByIPAddressAsync(long partnerSubscriptionCode, string iPAddressStart, string iPAddressEnd)
        {
            if (string.IsNullOrEmpty(iPAddressEnd))
            {
                return await partnerRepository.GetWhitelistIPByIPAddressAsync(partnerSubscriptionCode, iPAddressStart);
            }

            return await partnerRepository.GetWhitelistIPByIPAddressRangedAsync(partnerSubscriptionCode, iPAddressStart, iPAddressEnd);
        }

        public async Task<Result<WhitelistIP>> UpdateWhiteListIPAsync(WhitelistIP whitelistIP)
        {
            return await partnerRepository.UpdateWhitelistIPAsync(whitelistIP);
        }

        public async Task<Result<List<PartnerRegistration>>> GetPartnerRegistrationAsync(
            CustomerUser customer, long partnerCode)
        {
            var customerUserBusinessProfiles = await businessProfileRepository.GetCustomerUserBusinessProfilesByIdAsync(customer.Id);
            var partnerRegistration = await Repository.GetPartnerRegistrationAsync(customerUserBusinessProfiles);
            var partnerRegistrationByPartnerCode = await Repository.GetPartnerRegistrationByCodeAsync(partnerCode);

            foreach (var item in partnerRegistration)
            {
                if (item.Id == partnerRegistrationByPartnerCode.Id)
                {
                    return Result.Success(partnerRegistration);

                }
            }
            return Result.Failure<List<PartnerRegistration>>("User list is empty.");
        }

        public async Task<bool> UserHasTrangloEntity(TrangloStaff trangloStaff, long partnerCode)
        {
            var trangloStaffEntity = await this.staffEntityQueryService.GetTrangloStaffEntityAssignmentById(trangloStaff.LoginId);

            var trangloEntityByPartner = await this.partnerRepository.GetTrangloEntitiesByPartnerAsync(partnerCode);

            if (trangloEntityByPartner != null)
            {
                foreach (var item in trangloStaffEntity)
                {
                    if (trangloEntityByPartner.Exists(x => (x == item.TrangloEntity || x == null)))
                    {
                        return true;
                    }
                }
            }
            else if (trangloEntityByPartner.Any())
            {
                return true;
            }
            return false;
        }

        //CHECK FIELDS
        private async Task<bool> IsPartnerKYCNotStarted(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (businessProfile.WorkflowStatus == null && businessProfile.KYCSubmissionStatusCode == KYCSubmissionStatus.Draft.Id)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsPartnerKYCPendingReview(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (businessProfile.WorkflowStatus != null && businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Pending_Review)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsPartnerKYCInProgress(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (businessProfile.WorkflowStatus != null && businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Review_In_Progress)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsPartnerKYCCompleted(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (businessProfile.WorkflowStatus != null && businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Approved)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsPartnerKYCRejected(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (businessProfile.WorkflowStatus != null && businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Reject)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsPartnerSubscriptionProfileCompleted(long partnerCode, long partnerSubscriptionCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);
            var partnerSubscription = await partnerRepository.GetSubscriptionAsync(partnerSubscriptionCode);

            if (partnerSubscription.Solution == Solution.Connect)
            {
                if (!string.IsNullOrEmpty(businessProfile.CompanyRegistrationName)
                    && (!string.IsNullOrEmpty(businessProfile.TradeName)
                    && (!string.IsNullOrEmpty(businessProfile.CompanyRegistrationNo)
                    && partnerProfile.Email != null
                    && Email.Create(partnerProfile.Email.Value).IsSuccess)
                    && (businessProfile.ContactNumber != null
                    && ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsSuccess)
                    && businessProfile.CompanyRegisteredCountryCode != null
                    && (partnerSubscription.Solution != null)
                    && (partnerSubscription.PartnerType != null)
                    && (partnerSubscription.IsCurrencyCodeAssigned is true)
                    && ((//partnerSubscription.IsPricePackageAssigned is true && 
                    partnerSubscription.PartnerType == PartnerType.Sales_Partner)
                    || (partnerSubscription.PartnerType == PartnerType.Supply_Partner))))
                //&& partnerSubscription.PartnerType.Id == PartnerType.Sales_Partner.Id) || partnerSubscription.PartnerType.Id == PartnerType.Supply_Partner.Id)) //check

                {
                    return true;
                }
            }
            else if (partnerSubscription.Solution == Solution.Business) // Tranglo Business do not have Pricing Package and Trade Name is Optional
            {
                if (partnerProfile.CustomerTypeCode == CustomerType.Individual.Id) //Individual do not need CompanyRegistrationNo
                {
                    if (!string.IsNullOrEmpty(businessProfile.CompanyRegistrationName)
                    && partnerProfile.Email != null
                    && Email.Create(partnerProfile.Email.Value).IsSuccess
                    && (businessProfile.ContactNumber != null
                    && ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsSuccess)
                    && businessProfile.CompanyRegisteredCountryCode != null
                    && (partnerSubscription.Solution != null)
                    && (partnerSubscription.PartnerType != null)
                    && (partnerSubscription.IsCurrencyCodeAssigned is true)
                    && (partnerSubscription.PartnerType == PartnerType.Sales_Partner)
                    || (partnerSubscription.PartnerType == PartnerType.Supply_Partner))
                    //&& partnerSubscription.PartnerType.Id == PartnerType.Sales_Partner.Id) || partnerSubscription.PartnerType.Id == PartnerType.Supply_Partner.Id)) //check

                    {
                        return true;
                    }
                }
                else if (!string.IsNullOrEmpty(businessProfile.CompanyRegistrationName)
                    && (!string.IsNullOrEmpty(businessProfile.CompanyRegistrationNo)
                    && partnerProfile.Email != null
                    && Email.Create(partnerProfile.Email.Value).IsSuccess)
                    && (businessProfile.ContactNumber != null
                    && ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsSuccess)
                    && businessProfile.CompanyRegisteredCountryCode != null
                    && (partnerSubscription.Solution != null)
                    && (partnerSubscription.PartnerType != null)
                    && (partnerSubscription.IsCurrencyCodeAssigned is true)
                    && (partnerSubscription.PartnerType == PartnerType.Sales_Partner)
                    || (partnerSubscription.PartnerType == PartnerType.Supply_Partner))
                //&& partnerSubscription.PartnerType.Id == PartnerType.Sales_Partner.Id) || partnerSubscription.PartnerType.Id == PartnerType.Supply_Partner.Id)) //check

                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsPartnerSubscriptionProfileInProgress(long partnerCode, long partnerSubscriptionCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var subscription = await partnerRepository.GetSubscriptionAsync(partnerSubscriptionCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (string.IsNullOrEmpty(businessProfile.CompanyRegistrationName)
                || (string.IsNullOrEmpty(businessProfile.TradeName)
                || (string.IsNullOrEmpty(businessProfile.CompanyRegistrationNo)
                || partnerProfile.Email == null
                || Email.Create(partnerProfile.Email.Value).IsFailure)
                || (businessProfile.ContactNumber == null
                || ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsFailure)
                && businessProfile.CompanyRegisteredCountryCode == null
                || (subscription.Solution != null)
                && subscription.PartnerType == null
                || (!subscription.IsPricePackageAssigned is true && subscription.PartnerType == PartnerType.Sales_Partner)))
            {
                return true;
            }

            return false;
        }

        private async Task<Result<bool>> IsPartnerProfileNotStarted(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            if (partnerProfile == null)
            {
                return Result.Failure<bool>($"Failed to retrieve Partner Profile");
            }
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);
            if (businessProfile == null)
            {
                return Result.Failure<bool>($"Failed to retrieve Business Profile");
            }

            return businessProfile.KYCSubmissionStatusCode == KYCSubmissionStatus.Draft.Id;


        }

        private async Task<bool> IsPartnerProfileRejected(long partnerCode)
        {
            var KYC = await IsPartnerKYCRejected(partnerCode);

            if (KYC)
                return true;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="userBearerToken"></param>
        /// <param name="queryStatusOnly">Make this method read the status only, wont call any domain event or make changes</param>
        /// <returns></returns>
        public async Task<bool> IsPartnerReadyGoLive(long partnerCode, long partnerSubscriptionCode, bool queryStatusOnly = false)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var subscription = await partnerRepository.GetSubscriptionAsync(partnerSubscriptionCode);

            var isParterCompleted = await IsPartnerKYCCompleted(partnerCode);
            var isPartnerProfileComplete = await IsPartnerSubscriptionProfileCompleted(partnerCode, partnerSubscriptionCode); // check

            if (isParterCompleted
                && isPartnerProfileComplete
                && subscription.APIIntegrationOnboardWorkflowStatusCode == OnboardWorkflowStatus.Approve_Complete.Id
                && partnerProfile.AgreementOnboardWorkflowStatusCode == OnboardWorkflowStatus.Approve_Complete.Id
                && partnerProfile.AgreementStatus != null
                && partnerProfile.AgreementStartDate != null
                && partnerProfile.AgreementEndDate != null
                )
            {
                if (!queryStatusOnly)
                {
                    subscription.SetAPIIntegrationOnboardWorkflowStatus();

                    var updatePartnerSubcription = await partnerRepository.UpdateSubcriptionAsync(subscription);
                    if (updatePartnerSubcription.IsFailure)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                if (!queryStatusOnly)
                {
                    subscription.SetOverallOnboardInComplete();
                }
            }

            return false;


        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="partnerCode"></param>
		/// <param name="partnerSubscriptionCode"></param>
		/// <returns></returns>
		public async Task<Result<OnboardWorkflowStatus>> GetPartnerProfileStatus(long partnerCode, long partnerSubscriptionCode)
        {
            var partnerCompleted = await IsPartnerSubscriptionProfileCompleted(partnerCode, partnerSubscriptionCode);
            var partnerNotStarted = await IsPartnerProfileNotStarted(partnerCode);
            var partnerInProgress = await IsPartnerSubscriptionProfileInProgress(partnerCode, partnerSubscriptionCode);
            var partnerRejected = await IsPartnerProfileRejected(partnerCode);

            if (partnerCompleted && !partnerRejected)
            {
                return OnboardWorkflowStatus.Approve_Complete;
            }
            else if (partnerNotStarted.IsSuccess)
            {
                return OnboardWorkflowStatus.Pending;
            }
            else if (partnerNotStarted.IsFailure)
            {
                return Result.Failure<OnboardWorkflowStatus>(partnerNotStarted.Error);
            }
            else if (partnerInProgress)
            {
                return OnboardWorkflowStatus.In_Progress;
            }
            else if (partnerRejected)
            {
                return OnboardWorkflowStatus.Reject;
            }
            else
            {
                return OnboardWorkflowStatus.In_Progress;
            }
        }


        public async Task<WorkflowStatus> GetPartnerKYCStatus(long partnerCode)
        {
            var partnerKYCCompleted = await IsPartnerKYCCompleted(partnerCode);
            var partnerKYCInProgress = await IsPartnerKYCInProgress(partnerCode);
            var partnerKYCNotStarted = await IsPartnerKYCNotStarted(partnerCode);
            var partnerKYCPendingReview = await IsPartnerKYCPendingReview(partnerCode);
            var partnerKYCRejected = await IsPartnerKYCRejected(partnerCode);

            if (partnerKYCCompleted)
            {
                return WorkflowStatus.Compliance_Approved;
            }
            else if (partnerKYCInProgress)
            {
                return WorkflowStatus.Compliance_Review_In_Progress;
            }
            else if (partnerKYCNotStarted)
            {
                return WorkflowStatus.Compliance_Pending_Review;
            }
            else if (partnerKYCPendingReview)
            {
                return WorkflowStatus.Compliance_Pending_Review;
            }
            else if (partnerKYCRejected)
            {
                return WorkflowStatus.Compliance_Reject;
            }
            else
            {
                return WorkflowStatus.Compliance_Pending_Review;
            }
        }

        public async Task<dynamic> GetPartnerFlow(long partnerCode)
        {
            //  If a partner has bilateral (partner type = sales & supply partner) subscriptions added at the same time,
            //  the KYC edit restriction and review shall follow the sales partner flow

            var subscriptions = await partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerCode);

            if (subscriptions.All(x => x.PartnerType == PartnerType.Supply_Partner))
            {
                return PartnerType.Supply_Partner;
            }
            else if (subscriptions.Exists(x => x.PartnerType == PartnerType.Sales_Partner))
            {
                return PartnerType.Sales_Partner;
            }
            else if (subscriptions.All(x => x.PartnerType is null))
            {
                return null;
            }
            else
                return Result.Failure("Failed to get partner flow");
        }

        // TB Onboard summary checking
        private async Task<bool> IsBusinessKYCIncomplete(int businessProfileCode)
        {
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            if (businessProfile.BusinessWorkflowStatus == null && businessProfile.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Draft)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsBusinessKYCPendingReview(int businessProfileCode)
        {
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            var isKYCPendingReview = businessProfile.BusinessWorkflowStatus != null && (
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.Compliance_Pending_Review ||
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.Compliance_Review_In_Progress ||
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Pending_Review ||
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_In_Progress
                                            );

            if (isKYCPendingReview)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsBusinessKYCCompleted(int businessProfileCode)
        {
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            var isKYCCompleted = businessProfile.BusinessWorkflowStatus != null && (
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.Compliance_Approved ||
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Approved
                                            );

            if (isKYCCompleted)
            {
                return true;
            }

            return false;
        }

        // No requirement currently on reject (Checked with BAs to leave as it is)
        private async Task<bool> IsBusinessKYCRejected(int businessProfileCode)
        {
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            var isKYCRejected = businessProfile.BusinessWorkflowStatus != null && (
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.Compliance_Reject ||
                                            businessProfile.BusinessWorkflowStatus == WorkflowStatus.KYC_Operations_Reject
                                            );

            if (isKYCRejected)
            {
                return true;
            }

            return false;
        }

        public async Task<string> GetCustomerBusinessKYCStatus(int businessProfileCode)
        {
            var isBusinessKYCCompleted = await IsBusinessKYCCompleted(businessProfileCode);
            var isBusinessKYCIncomplete = await IsBusinessKYCIncomplete(businessProfileCode);
            var isBusinessKYCPendingReview = await IsBusinessKYCPendingReview(businessProfileCode);
            var isBusinessKYCNotApproved = await IsBusinessKYCRejected(businessProfileCode);
            
            if (isBusinessKYCCompleted)
            {
                return BusinessOnboardSummaryStatus.Complete;
            }
            else if (isBusinessKYCIncomplete)
            {
                return BusinessOnboardSummaryStatus.Incomplete;
            }
            else if (isBusinessKYCPendingReview)
            {
                return BusinessOnboardSummaryStatus.Pending_Review;
            }
            else if (isBusinessKYCNotApproved)
            {
                return BusinessOnboardSummaryStatus.NotApprove;
            }
            else
                return BusinessOnboardSummaryStatus.Incomplete;
        }

        public async Task<Result<TCPortalOnboardingStatus>> GetCustomerConnectKYCStatus(int businessProfileCode)
        {
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);

            if(businessProfile != null)
            {
                if (businessProfile.WorkflowStatus == null && businessProfile.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Draft)
                {
                    return TCPortalOnboardingStatus.Incomplete;
                }

                if(businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Pending_Review ||
                                          businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Review_In_Progress)
                {
                    return TCPortalOnboardingStatus.PendingReview;
                }

                if(businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Approved)
                {
                    return TCPortalOnboardingStatus.Approved;
                }

                if(businessProfile.WorkflowStatus == WorkflowStatus.Compliance_Reject)
                {
                    return TCPortalOnboardingStatus.NotApproved;
                }
            }

            return Result.Failure<TCPortalOnboardingStatus>("Invalid Business Profile Code.");
        }

        public async Task<Result<OnboardWorkflowStatus>> GetBusinessPartnerProfileStatus(long partnerCode, long partnerSubscriptionCode)
        {
            var partnerCompleted = await IsBusinessPartnerProfileCompleted(partnerCode, partnerSubscriptionCode);
            var partnerNotStarted = await IsBusinessPartnerProfileNotStarted(partnerCode);
            var partnerInProgress = await IsPartnerSubscriptionProfileInProgress(partnerCode, partnerSubscriptionCode);
            var partnerRejected = await IsBusinessPartnerKYCRejected(partnerCode);

            if (partnerCompleted && !partnerRejected)
            {
                return OnboardWorkflowStatus.Approve_Complete;
            }
            else if (partnerRejected)
            {
                return OnboardWorkflowStatus.Reject;
            }
            else if (partnerInProgress)
            {
                return OnboardWorkflowStatus.In_Progress;
            }
            else if (partnerNotStarted.IsSuccess)
            {
                return OnboardWorkflowStatus.Pending;
            }
            else if (partnerNotStarted.IsFailure)
            {
                return Result.Failure<OnboardWorkflowStatus>(partnerNotStarted.Error);
            }
            else
            {
                return OnboardWorkflowStatus.In_Progress;
            }
        }

        private async Task<Result<bool>> IsBusinessPartnerProfileNotStarted(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            if (partnerProfile == null)
            {
                return Result.Failure<bool>($"Failed to retrieve Partner Profile");
            }
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);
            if (businessProfile == null)
            {
                return Result.Failure<bool>($"Failed to retrieve Business Profile");
            }

            return businessProfile.BusinessKYCSubmissionStatus.Id == KYCSubmissionStatus.Draft.Id;
        }

        private async Task<bool> IsBusinessPartnerKYCRejected(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);
            bool businessWorkflowStatusRejected = false;
            if (businessProfile.BusinessWorkflowStatus != null)
            {

                businessWorkflowStatusRejected = businessProfile.BusinessWorkflowStatus.Id == WorkflowStatus.Compliance_Reject.Id ||
                                                     businessProfile.BusinessWorkflowStatus.Id == WorkflowStatus.KYC_Operations_Reject.Id;
            }
            if ((businessProfile.BusinessWorkflowStatus != null && businessWorkflowStatusRejected) ||
                businessProfile.BusinessKYCStatus == KYCStatus.Rejected)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsBusinessKYCInProgress(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            if (businessProfile.BusinessWorkflowStatus != null && businessProfile.BusinessWorkflowStatus.Id == WorkflowStatus.Compliance_Review_In_Progress.Id)
            {
                return true;
            }

            return false;
        }

        public async Task<WorkflowStatus> GetAdminBusinessKYCStatus(long partnerCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);

            var businessKYCCompleted = await IsBusinessKYCCompleted(businessProfile.Id);
            var businessKYCInProgress = await IsBusinessKYCInProgress(partnerCode);
            var businessKYCNotStarted = await IsBusinessKYCIncomplete(businessProfile.Id);
            var businessKYCPendingReview = await IsBusinessKYCPendingReview(businessProfile.Id);
            var businessKYCRejected = await IsBusinessKYCRejected(businessProfile.Id);

            // Remain as compliance workflow statuses for FE to display TB partner progress
            if (businessKYCCompleted)
            {
                WorkflowStatus workflowStatus = new WorkflowStatus();
                if (businessProfile.CollectionTier == CollectionTier.Tier_1)
                {
                    workflowStatus = WorkflowStatus.KYC_Operations_Approved;
                }

                if (businessProfile.CollectionTier == CollectionTier.Tier_2 || businessProfile.CollectionTier == CollectionTier.Tier_3)
                {
                    workflowStatus = WorkflowStatus.Compliance_Approved;
                }

                return workflowStatus;
            }
            else if (businessKYCInProgress)
            {
                WorkflowStatus workflowStatus = new WorkflowStatus();
                if (businessProfile.CollectionTier == CollectionTier.Tier_1)
                {
                    workflowStatus = WorkflowStatus.KYC_Operations_In_Progress;
                }

                if (businessProfile.CollectionTier == CollectionTier.Tier_2 || businessProfile.CollectionTier == CollectionTier.Tier_3)
                {
                    workflowStatus = WorkflowStatus.Compliance_Review_In_Progress;
                }

                return workflowStatus;
            }
            else if (businessKYCNotStarted || businessKYCPendingReview)
            {
                WorkflowStatus workflowStatus = new WorkflowStatus();
                if (businessProfile.CollectionTier == CollectionTier.Tier_1)
                {
                    workflowStatus = WorkflowStatus.KYC_Operations_Pending_Review;
                }

                if (businessProfile.CollectionTier == CollectionTier.Tier_2 || businessProfile.CollectionTier == CollectionTier.Tier_3)
                {
                    workflowStatus = WorkflowStatus.Compliance_Pending_Review;
                }

                return workflowStatus;
            }
            else if (businessKYCRejected)
            {
                return WorkflowStatus.Compliance_Reject;
            }
            else
            {
                return WorkflowStatus.Compliance_Pending_Review;
            }
        }

        private async Task<bool> IsBusinessPartnerProfileCompleted(long partnerCode, long partnerSubscriptionCode)
        {
            var partnerProfile = await GetPartnerRegistrationByCodeAsync(partnerCode);
            var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(partnerProfile.BusinessProfileCode);
            var partnerSubscription = await partnerRepository.GetSubscriptionAsync(partnerSubscriptionCode);

            if (partnerProfile.CustomerType == CustomerType.Individual)
            {
                if (!string.IsNullOrEmpty(businessProfile.CompanyName)
                && !(partnerProfile.CustomerType is null)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegistrationName) //Fullname
                && !string.IsNullOrEmpty(businessProfile.ContactPersonName) //Name P-I-C
                && ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsSuccess
                && Email.Create(partnerProfile.Email.Value).IsSuccess
                && !(businessProfile.BusinessNature is null)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegisteredAddress)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegisteredZipCodePostCode)
                && !(businessProfile.CompanyRegisteredCountryCode is null)
                && !(businessProfile.NationalityCode is null)
                && (partnerSubscription.Solution != null)
                && (partnerSubscription.PartnerType != null)
                && (partnerSubscription.IsCurrencyCodeAssigned is true)
                && ((partnerSubscription.PartnerType == PartnerType.Sales_Partner)
                || (partnerSubscription.PartnerType == PartnerType.Supply_Partner)))
                {
                    return true;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(businessProfile.CompanyName)
                && !(partnerProfile.CustomerType is null)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegistrationName)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegistrationNo)
                && !(businessProfile.TelephoneNumber is null)
                && !string.IsNullOrEmpty(businessProfile.ContactPersonName)
                //&& ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsSuccess
                && Email.Create(partnerProfile.Email.Value).IsSuccess
                && !(businessProfile.BusinessNature is null)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegisteredAddress)
                && !string.IsNullOrEmpty(businessProfile.CompanyRegisteredZipCodePostCode)
                && !(businessProfile.CompanyRegisteredCountryCode is null)
                && !string.IsNullOrEmpty(businessProfile.MailingAddress)
                && !string.IsNullOrEmpty(businessProfile.MailingZipCodePostCode)
                && !(businessProfile.MailingCountryCode is null)
                //&& !(businessProfile.RelationshipTieUpCode is null)
                && (partnerSubscription.Solution != null)
                && (partnerSubscription.PartnerType != null)
                && (partnerSubscription.IsCurrencyCodeAssigned is true)
                && ((partnerSubscription.PartnerType == PartnerType.Sales_Partner)
                || (partnerSubscription.PartnerType == PartnerType.Supply_Partner)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}