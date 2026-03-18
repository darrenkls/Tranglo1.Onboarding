using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class PartnerRepository : IPartnerRepository
    {
        private readonly PartnerDBContext dbContext;
        public PartnerRepository(PartnerDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PartnerAgreementTemplate> AddPartnerAgreementTemplateUploadAsync(PartnerAgreementTemplate partnerAgreementTemplate)
        {
            this.dbContext.PartnerAgreementTemplate.Add(partnerAgreementTemplate);
            await this.dbContext.SaveChangesAsync();
            return partnerAgreementTemplate;
        }

        public async Task<PartnerAgreementTemplate> RemovePartnerAgreementTemplateAsync(PartnerAgreementTemplate partnerAgreementTemplate)
        {
            this.dbContext.PartnerAgreementTemplate.Update(partnerAgreementTemplate);
            await this.dbContext.SaveChangesAsync();
            return partnerAgreementTemplate;
        }

        public async Task<List<PartnerAgreementTemplate>> GetPartnerAgreementTemplatesAsync(long partnerCode)
        {
            var partnerAgreementTemplates = dbContext.PartnerAgreementTemplate.Where(x => x.PartnerRegistration.Id == partnerCode);
            return await partnerAgreementTemplates.ToListAsync();
        }

        public async Task<PartnerAgreementTemplate> GetPartnerAgreementTemplateByTemplateIdAsync(PartnerAgreementTemplate partnerAgreementTemplate)
        {
            return await dbContext.PartnerAgreementTemplate.Where(x => x.TemplateId == partnerAgreementTemplate.TemplateId).FirstOrDefaultAsync();
        }

        public async Task<SignedPartnerAgreement> AddSignedPartnerAgreementUploadAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            this.dbContext.SignedPartnerAgreement.Add(signedPartnerAgreement);
            await this.dbContext.SaveChangesAsync();
            return signedPartnerAgreement;
        }

        public async Task<SignedPartnerAgreement> RemoveSignedPartnerAgreementAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            this.dbContext.SignedPartnerAgreement.Update(signedPartnerAgreement);
            await this.dbContext.SaveChangesAsync();
            return signedPartnerAgreement;
        }

        public async Task<SignedPartnerAgreement> UpdateSignedPartnerAgreementAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            this.dbContext.SignedPartnerAgreement.Update(signedPartnerAgreement);
            await this.dbContext.SaveChangesAsync();
            return signedPartnerAgreement;
        }

        public async Task<List<SignedPartnerAgreement>> GetSignedPartnerAgreementsAsync(long partnerCode)
        {
            var signedpartnerAgreements = dbContext.SignedPartnerAgreement.Where(x => x.PartnerRegistration.Id == partnerCode);
            return await signedpartnerAgreements.ToListAsync();
        }

        public async Task<SignedPartnerAgreement> GetSignedPartnerAgreementBySignedDocumentIdAsync(SignedPartnerAgreement signedPartnerAgreement)
        {
            return await dbContext.SignedPartnerAgreement.Where(x => x.SignedDocumentId == signedPartnerAgreement.SignedDocumentId).FirstOrDefaultAsync();
        }

        public async Task<PartnerRegistration> GetPartnerAgreementDetailsByPartnerCodeAsync(long partnerCode)
        {
            return await dbContext.PartnerRegistration.Where(x => x.Id == partnerCode).FirstOrDefaultAsync();
        }

        public async Task<PartnerRegistration> UpdatePartnerAgreementDetailsAsync(PartnerRegistration partnerRegistration)
        {
            this.dbContext.PartnerRegistration.Update(partnerRegistration);
            await this.dbContext.SaveChangesAsync();
            return partnerRegistration;
        }

        public async Task<IEnumerable<PartnerType>> GetPartnerTypes(Specification<PartnerType> spec)
        {
            var query = this.dbContext.PartnerTypes
              .Where(spec.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<Result<PartnerRegistration>> AddPartnerRegistrationAsync(PartnerRegistration partnerRegistration)
        {
            this.dbContext.PartnerRegistrations.Add(partnerRegistration);
            await this.dbContext.SaveChangesAsync();
            return partnerRegistration;
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationByCodeAsync(long partnerCode)
        {
            var a = dbContext.PartnerRegistrations.Where(x => x.Id == partnerCode);
            var partner = dbContext.PartnerRegistrations
                                .Include(x => x.CustomerType)
                                .Where(x => x.Id == partnerCode).FirstOrDefaultAsync();
            return await partner;
        }

        public async Task<Result<PartnerRegistration>> UpdatePartnerRegistrationAsync(PartnerRegistration partnerRegistration)
        {
            this.dbContext.PartnerRegistrations.Update(partnerRegistration);
            await this.dbContext.SaveChangesAsync();
            return partnerRegistration;
        }

        public async Task<Result<PartnerSubscription>> UpdatePartnerSubscriptionsAsync(PartnerSubscription partnerSubscription)
        {
            this.dbContext.PartnerSubscriptions.Update(partnerSubscription);
            await this.dbContext.SaveChangesAsync();
            return partnerSubscription;
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationByBusinessProfileCodeAsync(int businessProfileCode)
        {
            var partner = dbContext.PartnerRegistrations
                                   .Include(x => x.CustomerType)
                                   .FirstOrDefaultAsync(x => x.BusinessProfileCode == businessProfileCode);
            return await partner;
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationCodeByBusinessProfileCodeAsync(int businessProfileCode)
        {
            var partner = dbContext.PartnerRegistrations
                .Include(x => x.CustomerType)
                .FirstOrDefaultAsync(x => x.BusinessProfileCode == businessProfileCode);
            return await partner;
        }

        public async Task<PartnerRegistration> GetPartnerDetailsByCodeAsync(long partnerCode)
        {
            var partner = dbContext.PartnerRegistrations
                .Include(x => x.CustomerType)
                .FirstOrDefaultAsync(x => x.Id == partnerCode);
            return await partner;
        }
        public async Task<IEnumerable<ChangeType>> GetPartnerAccountStatusChangeTypeAsync()
        {
            var query = this.dbContext.ChangeTypes;

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<PartnerAccountStatusType>> GetPartnerAccountStatusTypeAsync()
        {
            var query = this.dbContext.PartnerAccountStatusTypes;

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<OnboardWorkflowStatus>> GetOnboardWorkflowStatusAsync()
        {
            var query = this.dbContext.OnboardWorkflowStatuses;

            return await query.ToListAsync();
        }

        public async Task<HelloSignDocument> AddHelloSignDocumentAsync(HelloSignDocument helloSignDocument)
        {
            this.dbContext.HelloSignDocument.Add(helloSignDocument);
            await this.dbContext.SaveChangesAsync();
            return helloSignDocument;
        }

        public async Task<HelloSignDocument> RemoveHelloSignDocumentAsync(HelloSignDocument helloSignDocument)
        {
            this.dbContext.HelloSignDocument.Update(helloSignDocument);
            await this.dbContext.SaveChangesAsync();
            return helloSignDocument;
        }

        public async Task<List<HelloSignDocument>> GetHelloSignDocumentsAsync(long partnerCode)
        {
            var helloSignDocuments = dbContext.HelloSignDocument.Where(x => x.PartnerRegistration.Id == partnerCode);
            return await helloSignDocuments.ToListAsync();
        }

        public async Task<HelloSignDocument> GetHelloSignDocumentByHelloSignDocumentIdAsync(long helloSignDocumentId)
        {
            return await dbContext.HelloSignDocument.Where(x => x.Id == helloSignDocumentId).FirstOrDefaultAsync();
        }

        public async Task<PartnerAPISetting> AddPartnerAPISettingAsync(PartnerAPISetting partnerAPISetting)
        {
            this.dbContext.PartnerAPISettings.Add(partnerAPISetting);
            await this.dbContext.SaveChangesAsync();
            return partnerAPISetting;
        }
        public async Task<Result<PartnerProfileChangedEvent>> AddPartnerOnboardingCreationEventAsync(PartnerProfileChangedEvent onboardingEvent)
        {

            if (onboardingEvent.TelephoneNumber != null)
            {
                var contactNumberResult = ContactNumber.Create(
                    onboardingEvent.TelephoneNumber.DialCode,
                    onboardingEvent.TelephoneNumber.CountryISO2Code,
                    onboardingEvent.TelephoneNumber.Value);

                if (contactNumberResult.IsFailure)
                {
                    return Result.Failure<PartnerProfileChangedEvent>(contactNumberResult.Error);
                }

                onboardingEvent.TelephoneNumber = contactNumberResult.Value;
            }

            if (onboardingEvent.Email != null)
            {
                var emailResult = Email.Create(
                    onboardingEvent.Email.Value);

                if (emailResult.IsFailure)
                {
                    return Result.Failure<PartnerProfileChangedEvent>(emailResult.Error);
                }

                onboardingEvent.Email = emailResult.Value;
            }

            this.dbContext.PartnerProfileChangedEvent.Add(onboardingEvent);
            await this.dbContext.SaveChangesAsync();
            return onboardingEvent;
        }

        public async Task<WhitelistIP> AddWhitelistIPAsync(WhitelistIP whitelistIP)
        {
            this.dbContext.WhitelistIP.Add(whitelistIP);
            await this.dbContext.SaveChangesAsync();
            return whitelistIP;
        }

        public async Task<PartnerRegistration> GetPartnerInfoByCodeForSignUpCode(long partnerCode)
        {
            var partner = dbContext.PartnerRegistrations.Where(x => x.Id == partnerCode).FirstOrDefaultAsync();
            return await partner;
        }

        public async Task<List<PartnerAPISetting>> GetPartnerAPISettingByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode)
        {
            var partnerAPISetting = dbContext.PartnerAPISettings.Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode);
            return await partnerAPISetting.ToListAsync();
        }
        public async Task<List<PartnerAPISetting>> GetPartnerAPISettingByAPIUserIdAsync(long partnerCode, string apiUserId)
        {
            var partnerAPISetting = dbContext.PartnerAPISettings.Where(x => x.PartnerCode == partnerCode && x.APIUserId == apiUserId);
            return await partnerAPISetting.ToListAsync();
        }
        public async Task<List<PartnerAPISetting>> GetPartnerAPISettingBySecretKeyAsync(long partnerCode, string secretKey)
        {
            var partnerAPISetting = dbContext.PartnerAPISettings.Where(x => x.PartnerCode == partnerCode && x.SecretKey == secretKey);
            return await partnerAPISetting.ToListAsync();
        }
        public async Task<PartnerAPISetting> GetPartnerAPISettingAsync(long APISettingId)
        {
            var partnerAPISetting = dbContext.PartnerAPISettings.Where(x => x.Id == APISettingId);
            return await partnerAPISetting.FirstOrDefaultAsync();
        }
        public async Task<WhitelistIP> GetWhitelistIPAsync(long whitelistIPId)
        {
            var whitelistIP = dbContext.WhitelistIP.Where(x => x.Id == whitelistIPId);
            return await whitelistIP.FirstOrDefaultAsync();
        }
        public async Task<PartnerAPISetting> UpdatePartnerAPISettingAsync(PartnerAPISetting partnerAPISetting)
        {
            this.dbContext.PartnerAPISettings.Update(partnerAPISetting);
            await this.dbContext.SaveChangesAsync();
            return partnerAPISetting;
        }
        public async Task<APIURL> GetApiUrlAsync(int env, APIType aPIType)
        {
            var stringDomain = dbContext.APIURL.Where(x => x.Environment == env && x.APIType == aPIType.Id);
            return await stringDomain.FirstOrDefaultAsync();
        }
        public async Task<WhitelistIP> UpdateWhitelistIPAsync(WhitelistIP whitelistIP)
        {
            this.dbContext.WhitelistIP.Update(whitelistIP);
            await this.dbContext.SaveChangesAsync();
            return whitelistIP;
        }

        public async Task<WhitelistIP> GetWhitelistIPByIPAddressRangedAsync(long partnerSubscriptionCode, string iPAddressStart, string iPAddressEnd)
        {
            var whitelistIPs = dbContext.WhitelistIP.Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode && x.IPAddressStart == iPAddressStart && x.IPAddressEnd == iPAddressEnd).FirstOrDefaultAsync();
            return await whitelistIPs;
        }

        public async Task<WhitelistIP> GetWhitelistIPByIPAddressAsync(long partnerSubscriptionCode, string iPAddressStart)
        {
            var whitelistIPs = dbContext.WhitelistIP.Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode && x.IPAddressStart == iPAddressStart && string.IsNullOrEmpty(x.IPAddressEnd)).FirstOrDefaultAsync();
            return await whitelistIPs;
        }
        public async Task<List<PartnerRegistration>> GetPartnerRegistrationAsync(List<CustomerUserBusinessProfile> customerUserBusinessProfiles)
        {
            List<PartnerRegistration> partnerRegistrations = new List<PartnerRegistration>();
            foreach (var item in customerUserBusinessProfiles)
            {
                var query = await dbContext.PartnerRegistration.Where(x => x.BusinessProfileCode == item.BusinessProfileCode).FirstOrDefaultAsync();
                partnerRegistrations.Add(query);
            }
            /* No tracking queries are useful when the results are used in a read-only scenario.
               They're quicker to execute because there's no need to set up the change tracking information.*/
            return partnerRegistrations;
        }

        #region CMS Integration
        public async Task<PartnerCMSIntegrationDetail> GetPartnerCMSIntegrationByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode)
        {
            return await dbContext.PartnerCMSIntegrationDetails.Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode).FirstOrDefaultAsync();
        }

        public async Task<PartnerCMSIntegrationDetail> AddPartnerCMSIntegration(PartnerCMSIntegrationDetail partnerCMSIntegrationDetail)
        {
            dbContext.PartnerCMSIntegrationDetails.Add(partnerCMSIntegrationDetail);
            await dbContext.SaveChangesAsync();
            return partnerCMSIntegrationDetail;
        }
        public async Task<PartnerCMSIntegrationDetail> UpdatePartnerCMSIntegration(PartnerCMSIntegrationDetail partnerCMSIntegrationDetail)
        {
            dbContext.PartnerCMSIntegrationDetails.Update(partnerCMSIntegrationDetail);
            await dbContext.SaveChangesAsync();
            return partnerCMSIntegrationDetail;
        }

        public async Task<PartnerWalletCMSIntegrationDetail> GetPartnerWalletIntegrationByPartnerSubscriptionCodeAsync(long partnerSubscriptionCode)
        {
            return await dbContext.PartnerWalletCMSIntegrationDetails.Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode).FirstOrDefaultAsync();
        }

        public async Task<PartnerWalletCMSIntegrationDetail> AddPartnerWalletIntegration(PartnerWalletCMSIntegrationDetail partnerWalletDetail)
        {
            dbContext.PartnerWalletCMSIntegrationDetails.Add(partnerWalletDetail);
            await dbContext.SaveChangesAsync();
            return partnerWalletDetail;
        }
        public async Task<PartnerWalletCMSIntegrationDetail> UpdatePartnerWalletIntegration(PartnerWalletCMSIntegrationDetail partnerWalletDetail)
        {
            dbContext.PartnerWalletCMSIntegrationDetails.Update(partnerWalletDetail);
            await dbContext.SaveChangesAsync();
            return partnerWalletDetail;
        }

        public async Task<IEnumerable<PartnerWalletCMSIntegrationDetail>> GetPartnerWalletListByStatus(string status)
        {
            return await dbContext.PartnerWalletCMSIntegrationDetails.Where(x => x.CMSStatus == status).ToListAsync();
        }
        #endregion
        public async Task<PartnerRegistration> GetPartnerRegistrationByEmail(string loginId)
        {
            var partner = dbContext.PartnerRegistrations.Where(x => x.Email.Value == loginId).FirstOrDefaultAsync();
            return await partner;
        }

        /*		public async Task<string> GetTrangloEntityByPartnerAsync(long partnerCode)
                {
                    var partner = await dbContext.PartnerRegistrations.Where(x => x.Id == partnerCode).FirstOrDefaultAsync();

                    var entity = partner?.TrangloEntity;
                    return entity;
                }*/

        public async Task<List<string>> GetTrangloEntitiesByPartnerAsync(long partnerCode)
        {
            var subscriptions = await dbContext.PartnerSubscriptions.Where(x => x.PartnerCode == partnerCode).ToListAsync();

            List<string> entities = new List<string>();
            entities.AddRange(subscriptions?.Select(x => x.TrangloEntity));

            return entities;
        }

        public async Task<Result<PartnerSubscription>> AddSubcriptionAsync(PartnerSubscription partnerSubscription)
        {
            //this.dbContext.PartnerSubscriptions.AddRange(partnerSubscription);
            this.dbContext.Entry(partnerSubscription).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync();
            return partnerSubscription;
        }

        public async Task<Result<PartnerSubscription>> UpdateSubcriptionAsync(PartnerSubscription partnerSubscription)
        {
            //this.dbContext.PartnerSubscriptions.Update(partnerSubscription);
            this.dbContext.Entry(partnerSubscription).State = EntityState.Modified;
            await this.dbContext.SaveChangesAsync();
            return partnerSubscription;
        }

        public async Task<PartnerSubscription> GetSubscriptionAsync(long partnerSubscriptionCode)
        {
            var partnerSubscription = dbContext.PartnerSubscriptions.Where(x => x.Id == partnerSubscriptionCode)
                .Include(x => x.PartnerType)
                .Include(x => x.Solution)
                .Include(x => x.Environment);
            return await partnerSubscription.FirstOrDefaultAsync();
        }

        public async Task<List<PartnerSubscription>> GetSubscriptionsByPartnerCodeAsync(long partnerCode)
        {
            var partnerSubscription = dbContext.PartnerSubscriptions.Where(x => x.PartnerCode == partnerCode)
                .Include(x => x.PartnerType)
                .Include(x => x.Solution)
                .Include(x => x.Environment)
                .Include(x => x.PartnerAccountStatusType);
            return await partnerSubscription.ToListAsync();
        }

        public async Task<List<PartnerSubscription>> GetSubscriptionAsync(long partnerCode, string trangloEntity)
        {
            var partnerSubscription = dbContext.PartnerSubscriptions
                .Include(x => x.PartnerType)
                .Include(x => x.Solution)
                .Include(x => x.Environment)
                .Include(x => x.PartnerAccountStatusType)
                .Where(x => x.PartnerCode == partnerCode && x.TrangloEntity == trangloEntity);
            return await partnerSubscription.ToListAsync();
        }

		public async Task<List<PartnerSubscription>> GetSubscriptionWithNullEntityAsync(long partnerCode, string trangloEntity)
		{
			var partnerSubscription = dbContext.PartnerSubscriptions
				.Include(x => x.PartnerType)
				.Include(x => x.Solution)
				.Include(x => x.Environment)
				.Include(x => x.PartnerAccountStatusType)
				.Where(x => x.PartnerCode == partnerCode && (x.TrangloEntity == trangloEntity || x.TrangloEntity == null));
			return await partnerSubscription.ToListAsync();
		}

		public async Task<Result<PartnerSubscription>> DeleteSubcriptionAsync(PartnerSubscription partnerSubscription)
        {
            this.dbContext.Entry(partnerSubscription).State = EntityState.Deleted;
            await this.dbContext.SaveChangesAsync();
            return partnerSubscription;
        }

        public async Task<CountryMeta> GetCountryAsync(string countryISO2)
        {
            return await dbContext.Countries.Where(x => x.CountryISO2 == countryISO2).FirstOrDefaultAsync();
        }

        public async Task<PartnerType> GetPartnerTypeAsync(long partnerTypeCode)
        {
            return await dbContext.PartnerTypes.Where(x => x.Id == partnerTypeCode).FirstOrDefaultAsync();
        }

        public async Task<Solution> GetSolutionAsync(long solutionCode)
        {
            return await dbContext.Solutions.Where(x => x.Id == solutionCode).FirstOrDefaultAsync();
        }

        public async Task<PartnerAccountStatusType> GetPartnerAccountStatusTypeAsync(long partnerAccountStatusTypeCode)
        {
            return await dbContext.PartnerAccountStatusTypes.Where(x => x.Id == partnerAccountStatusTypeCode).FirstOrDefaultAsync();
        }
        public async Task<ChangeType> GetChangeTypeAsync(long changeTypeCode)
        {
            return await dbContext.ChangeTypes.Where(x => x.Id == changeTypeCode).FirstOrDefaultAsync();
        }

        public async Task<List<PartnerSubscription>> GetPartnerSubscriptionListAsync(long partnerCode)
        {
            var test = dbContext.PartnerSubscriptions.Where(x => x.PartnerCode == partnerCode)
                .Include(x => x.PartnerType)
                .Include(x => x.Solution)
                .Include(x => x.PartnerAccountStatusType);

            return await test.ToListAsync();
        }

        public async Task<TrangloEntity> GetTrangloEntityMeta(string trangloEntity)
        {
            return await dbContext.Entities.Where(x => x.TrangloEntityCode == trangloEntity).FirstOrDefaultAsync();
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationByPartnerId(Guid partnerId)
        {
            var partner = await dbContext.PartnerRegistrations.Where(x => x.PartnerId == partnerId).FirstOrDefaultAsync();
            return partner;
        }

        public async Task<PartnerProfileChangedEvent> GetPartnerOnboardingEventAsync(long partnerSubscriptionCode)
        {
            var partner = await dbContext.PartnerProfileChangedEvent.Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode).FirstOrDefaultAsync();
            return partner;
        }

        public async Task<PartnerSubscription> GetPartnerSubscriptionByCodeAsync(long partnerSubscriptionCode)
        {
            var partnerSubscription = dbContext.PartnerSubscriptions
                                        .Include(x => x.PartnerType)
                                        .Include(x => x.Solution)
                                        .Include(x => x.Environment)
										.FirstOrDefaultAsync(x => x.Id == partnerSubscriptionCode);
            return await partnerSubscription;
        }
        public async Task<PartnerSubscription> GetPartnerSubscriptionByPartnerCodeAsync(long? partnerCode)
        {
            var partnerSubscription = dbContext.PartnerSubscriptions
                                        .Include(x => x.PartnerType)
                                        .Include(x => x.Solution)
                                        .FirstOrDefaultAsync(x => x.PartnerCode == partnerCode);
            return await partnerSubscription;
        }

        public async Task<PartnerSubscription> GetPartnerSubcriptionByPartnerCodeAndSolutionCodeAsync(long partnerCode, int solutionCode)
        {
            var partnerSubscription = dbContext.PartnerSubscriptions
                                        .Include(x => x.PartnerType)
                                        .Include(x => x.Solution)
                                        .Include(x => x.Environment)
                                        .FirstOrDefaultAsync(x => x.PartnerCode == partnerCode && x.Solution.Id == solutionCode);
            return await partnerSubscription;
        }

        public async Task<List<PartnerSubscription>> GetSalesPartnerSubscriptionListAsync(long partnerCode)
        {
            var test = dbContext.PartnerSubscriptions.Where(x => x.PartnerCode == partnerCode)
                .Include(x => x.PartnerType)
                .Include(x => x.Solution);

            return await test.ToListAsync();
        }

        public async Task<CustomerType> GetCustomerTypeByCodeAsync(long? customerTypeCode)
        {
            return await this.dbContext.CustomerTypes.Where(x => x.Id == customerTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<RelationshipTieUp> GetRelationshipTieUpByCodeAsync(long? relationshipTieUpCode)
        {
            return await this.dbContext.RelationshipTieUps.Where(x => x.Id == relationshipTieUpCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Solution>> GetSolutionsByPartnerAsync(long partnerCode)
        {
            var query = dbContext.PartnerSubscriptions.Where(x => x.PartnerCode == partnerCode)
                .Include(x => x.PartnerType)
                .Include(x => x.Solution)
                .Select(x => x.Solution)
                .Distinct();

            return await query.ToListAsync();
        }

        public async Task<CustomerType> GetCustomerTypeByBusinessProfileAsync(int businessProfileCode)
        {
            var query = await dbContext.PartnerRegistrations
                .Where(x => x.BusinessProfileCode == businessProfileCode)
                .Include(x => x.CustomerType)
                .Select(x => x.CustomerType)
                .FirstOrDefaultAsync();

            return query;
        }

        public async Task<PartnerSubscription> GetPartnerUserTrangloEntityByCodeAsync(long? partnerCode, string customerSolution)
        {
            var partnerSubscription = await dbContext.PartnerSubscriptions
                .Include(x => x.Solution)
                .Include(x => x.Environment)
                .FirstOrDefaultAsync(p => p.PartnerCode == partnerCode.Value && p.Solution.Name == customerSolution);


            return partnerSubscription;
        }

        public async Task<PartnerRegistration> GetPartnerRegistrationCodeByCodeAsync(long? partnerCode)
        {
            var partner = await dbContext.PartnerRegistrations.FindAsync(partnerCode);
            return partner;
        }

        public async Task<List<KYCCategoryCustomerType>> GetKYCBusinessCategoryByCustomerTypeGroupCodeAsync(int customerTypeGroupCode)
        {
            var kycCategoryCustomerType = await dbContext.kYCCategoryCustomerTypes.Where(x => x.CustomerTypeGroupCode == customerTypeGroupCode)
                .Include(x => x.KYCCategory)
                .ToListAsync();
            return kycCategoryCustomerType;

        }

        public async Task<IEnumerable<KYCReminderStatus>> GetKYCReminderStatusesAsync()
        {
            var kycReminderStatus = await dbContext.KYCReminderStatuses.ToListAsync();
            return kycReminderStatus;
        }

        public async Task<PartnerAccountStatus> GetLatestPartnerAccountStatusByPartnerSubscription(long partnerSubscriptionCode)
        {
            return await dbContext.PartnerAccountStatus
                .AsNoTracking()
                .Include(x => x.PartnerAccountStatusType)
                .Where(x => x.PartnerSubscriptionCode == partnerSubscriptionCode)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PartnerSubscription>> GetPartnerSubscriptionsByBusinessProfileIdsAsync(HashSet<int> businessProfileIds)
        {
            return await (
                from ps in dbContext.PartnerSubscriptions
                join pr in dbContext.PartnerRegistrations on ps.PartnerCode equals pr.Id  
                where businessProfileIds.Contains(pr.BusinessProfileCode)
                select ps
            ).ToListAsync();
        }
    }
}