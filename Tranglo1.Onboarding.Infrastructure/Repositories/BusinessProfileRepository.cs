using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class BusinessProfileRepository : IBusinessProfileRepository
    {
        private readonly BusinessProfileDbContext dbContext;

        public BusinessProfileRepository(BusinessProfileDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task<IReadOnlyList<ShareholderType>> GetShareholderTypesAsync(Specification<ShareholderType> filters)
        {
            var query = this.dbContext.ShareholderTypes
                .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<DocumentCategoryBPStatus>> GetDocumentCategoryBPStatusesAsync(Specification<DocumentCategoryBPStatus> filters)
        {
            var query = this.dbContext.DocumentCategoryBPStatuses
                .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<BusinessNature>> GetBusinessNaturesAsync(Specification<BusinessNature> filters)
        {
            var query = this.dbContext.BusinessNatures
                .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<BusinessProfile>> GetBusinessProfilesAsync(Specification<BusinessProfile> filters, bool isNoTrackKYCSubmissionStatus = false)
        {
            var query = (!isNoTrackKYCSubmissionStatus) ? this.dbContext.BusinessProfiles
                .Where(filters.ToExpression())
                .Include(x => x.KYCSubmissionStatus)
                .Include(x => x.BusinessKYCSubmissionStatus)
                .Include(x => x.CollectionTier)
                .Include(x => x.BusinessWorkflowStatus)
                .Include(x => x.EntityType)
                .Include(x => x.BusinessProfileIDType)
                .Include(x => x.RelationshipTieUp)
                //.Include(x => x.WorkflowStatus)
                :
                this.dbContext.BusinessProfiles
                .Where(filters.ToExpression())
                .Include(x => x.EntityType)
                .Include(x => x.RelationshipTieUp);
            //.Include(x => x.WorkflowStatus);

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<KYCStatus>> GetKYCStatusesAsync(Specification<KYCStatus> filters)
        {
            var query = this.dbContext.KYCStatuses
                .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<ReviewResult>> GetReviewResultsAsync(Specification<ReviewResult> filters)
        {
            var query = this.dbContext.ReviewResults
                .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<KYCCategory>> GetKYCCategoriesAsync(Specification<KYCCategory> filters)
        {
            var query = this.dbContext.KYCCategories
                .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<ReviewResult> GetReviewResultByCodeAsync(int businessProfileCode, long kycCategoryCode)
        {
            var kycReviewResult = await (from sub in dbContext.kYCSubModuleReviews
                                         join cat in dbContext.KYCCategories on sub.KYCCategoryCode equals cat.Id
                                         join rr in dbContext.ReviewResults on sub.ReviewResult.Id equals rr.Id into base_data
                                         from review_result in base_data.DefaultIfEmpty()
                                         where sub.BusinessProfileCode == businessProfileCode & sub.KYCCategoryCode == kycCategoryCode
                                         select review_result
                                         ).ToListAsync();

            return kycReviewResult.FirstOrDefault();
        }

        public async Task<WorkflowStatus> GetWorkflowStatusesAsync(long? workflowStatus)
        {
            return await dbContext.WorkflowStatuses.FirstOrDefaultAsync(x => x.Id == workflowStatus);
        }
        public async Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfilesByCodeAsync(long customerUserBusinessProfileCode)
        {
            var query = await this.dbContext.CustomerUserBusinessProfiles.FirstOrDefaultAsync(x => x.Id == customerUserBusinessProfileCode);

            return query;
        }
        public async Task<List<CustomerUserBusinessProfileRole>> GetCustomerUserBusinessProfilesByRoleCodeAsync(string roleCode)
        {
            var query = this.dbContext.CustomerUserBusinessProfileRoles.Where(x => x.RoleCode == roleCode);

            return await query.ToListAsync();
        }
        public async Task<BusinessProfile> GetBusinessProfileByCodeAsync(long? businessProfileCode)
        {
            var query = await this.dbContext.BusinessProfiles
                .Include(x => x.BusinessKYCSubmissionStatus)
                .Include(x => x.CollectionTier)
                .Include(x => x.BusinessWorkflowStatus)
                .Include(x => x.BusinessKYCStatus)
                .Include(x => x.KYCSubmissionStatus)
                //.Include(x => x.WorkflowStatus)
                .Include(x => x.CompanyRegisteredCountryMeta)
                .Include(x => x.ServiceType)
                .Include(x => x.NationalityMeta)
                .Include(x => x.PartnerRegistrations).ThenInclude(x => x.PartnerSubscriptions).ThenInclude(x => x.Solution)
                .FirstOrDefaultAsync(x => x.Id == businessProfileCode);

            return query;
        }
        public async Task<BusinessProfile> GetBusinessProfileByCodeTrackAsync(long? businessProfileCode)
        {
            var query = await this.dbContext.BusinessProfiles.FirstOrDefaultAsync(x => x.Id == businessProfileCode);

            return query;
        }

        public async Task<BusinessProfile> GetBusinessProfileByCompanyNameAsync(string companyName)
        {
            var query = await this.dbContext.BusinessProfiles.FirstOrDefaultAsync(x => x.CompanyName == companyName);

            return query;
        }

        public async Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfilesByUserIdAsync(long userId, long businessProfileCode)
        {
            var query = await this.dbContext.CustomerUserBusinessProfiles.FirstOrDefaultAsync(x => x.UserId == userId && x.BusinessProfileCode == businessProfileCode);

            return query;
        }

        public async Task<CustomerUserBusinessProfileRole> GetCustomerUserBusinessProfileRolesByCodeAsync(long? customerUserBusinessProfileCode, string roleCode)
        {
            var query = await this.dbContext.CustomerUserBusinessProfileRoles.FirstOrDefaultAsync(x => x.CustomerUserBusinessProfileCode == customerUserBusinessProfileCode && x.RoleCode == roleCode);

            return query;
        }

        public async Task<CustomerUserBusinessProfileRole> GetCustomerUserBusinessProfileRolesByCodeAsync(long? customerUserBusinessProfileCode)
        {
            var query = await this.dbContext.CustomerUserBusinessProfileRoles.FirstOrDefaultAsync(x => x.CustomerUserBusinessProfileCode == customerUserBusinessProfileCode);

            return query;
        }

        public async Task<IReadOnlyList<CustomerUserBusinessProfile>> GetCustomerUserBusinessProfilesAsync(Specification<CustomerUserBusinessProfile> filters)
        {
            var query = this.dbContext.CustomerUserBusinessProfiles
                .Where(filters.ToExpression())
                .Include(x => x.CompanyUserAccountStatus)
                .Include(x => x.CompanyUserBlockStatus);
            //.Include(x => x.AccountStatus);

            /* No tracking queries are useful when the results are used in a read-only scenario.
               They're quicker to execute because there's no need to set up the change tracking information.*/
            return await query.ToListAsync();
        }
        public async Task<IReadOnlyList<BusinessProfile>> GetBusinessProfileListAsync()
        {
            var query = this.dbContext.BusinessProfiles;

            /* No tracking queries are useful when the results are used in a read-only scenario.
               They're quicker to execute because there's no need to set up the change tracking information.*/
            return (IReadOnlyList<BusinessProfile>)await query.AsNoTracking().ToListAsync();
        }

        public async Task<IReadOnlyList<BusinessProfile>> GetSubmittedKycBusinessProfilesAsync()
        {
            // Tranglo Connect with BusinessProfile.KYCSubmissionStatusCode = 2 indicates KYC submitted
            // Tranglo Business with BusinessProfile.BusinessKYCSubmissionStatusCode = 2 indicates KYC submitted

            var businessProfile = await this.dbContext.BusinessProfiles
                .Include(x => x.BusinessKYCSubmissionStatus)
                .Include(x => x.KYCSubmissionStatus)
                .Where(x => (x.BusinessKYCSubmissionStatusCode != null && x.BusinessKYCSubmissionStatusCode == KYCSubmissionStatus.Submitted.Id) ||
                    (x.KYCSubmissionStatusCode != null && x.KYCSubmissionStatusCode == KYCSubmissionStatus.Submitted.Id))
                .AsNoTracking()
                .ToListAsync();

            return businessProfile;
        }

        //TODO: Refactor to combine all AML CFT checking to one Function
        public async Task<List<AMLCFTDisplayRules>> GetAMLCFTDisplayRuleAsync(EntityType entity, RelationshipTieUp tieUp, List<ServicesOffered> servicesOffered)
        {
            List<AMLCFTDisplayRules> displayRuleList = new List<AMLCFTDisplayRules>();

            if (servicesOffered.Count() == 0)
            {
                var displayRules = await this.dbContext.AMLCFTDisplayRule
                .Include(x => x.RelationshipTieUp)
                .Include(x => x.EntityType)
                .Include(x => x.Questionnaire)
                .Include(x => x.ServicesOffered)
                .Where(x => (x.EntityType == null || x.EntityType == entity) &&
                            (x.RelationshipTieUp == null || x.RelationshipTieUp == tieUp) &&
                            x.ServicesOffered == null).AsNoTracking().ToListAsync();

                if (displayRules != null)
                {
                    displayRuleList.AddRange(displayRules);
                }
            }
            else
            {
                foreach (var service in servicesOffered)
                {
                    var displayRules = await this.dbContext.AMLCFTDisplayRule
                    .Include(x => x.RelationshipTieUp)
                    .Include(x => x.EntityType)
                    .Include(x => x.Questionnaire)
                    .Include(x => x.ServicesOffered)
                    .Where(x => (x.EntityType == null || x.EntityType == entity) &&
                                (x.RelationshipTieUp == null || x.RelationshipTieUp == tieUp) &&
                                (x.ServicesOffered == null || x.ServicesOffered == service)).AsNoTracking().ToListAsync();

                    if (displayRules != null)
                    {
                        displayRuleList.AddRange(displayRules);
                    }
                }
            }

            return displayRuleList;
        }

        public async Task<List<Questionnaire>> GetQuestionnairesByAMLCFTQuestionnairesAsync(int businessProfileCode)
        {
            var questionnaires = await (from aml in this.dbContext.AMLCFTQuestionnaires.Where(x => x.BusinessProfile.Id == businessProfileCode)
                                        join quest in this.dbContext.Questions on aml.Question.Id equals quest.Id
                                        join qs in this.dbContext.QuestionSections on quest.QuestionSection.Id equals qs.Id
                                        join qt in this.dbContext.Questionnaires on qs.Questionnaire.Id equals qt.Id
                                        group qt by new { qt.Id, qt.Description } into groupResult
                                        select new Questionnaire(groupResult.Key.Id, groupResult.Key.Description)
                                 ).ToListAsync();

            return questionnaires;
        }

        public async Task<AMLCFTDisplayRules> GetAMLCFTDisplayRuleSingleAsync(EntityType entity, RelationshipTieUp tieUp, ServicesOffered servicesOffered)
        {
            var query = await this.dbContext.AMLCFTDisplayRule.Include(x => x.RelationshipTieUp).Include(x => x.EntityType).Include(x => x.Questionnaire).Where(x => x.EntityType == entity && x.RelationshipTieUp == tieUp && x.ServicesOffered == servicesOffered).FirstOrDefaultAsync();
            return query;
        }
        public async Task<AMLCFTDisplayRules> GetAMLCFTDisplayRuleQuestionnaireAsync(EntityType entity, RelationshipTieUp tieUp, ServicesOffered servicesOffered, Questionnaire questionnaire)
        {
            var query = await this.dbContext.AMLCFTDisplayRule.Include(x => x.RelationshipTieUp).Include(x => x.EntityType).Include(x => x.Questionnaire).Where(x => x.EntityType == entity && x.RelationshipTieUp == tieUp && x.ServicesOffered == servicesOffered && x.Questionnaire == questionnaire).FirstOrDefaultAsync();
            return query;
        }

        public async Task<AMLCFTDisplayRules> GetAMLCFTDisplayRuleByQuestionnaireAsync(EntityType entity, RelationshipTieUp tieUp, ServicesOffered servicesOffered, Questionnaire questionnaire)
        {
            var query = await this.dbContext.AMLCFTDisplayRule.Where(x => x.EntityType == entity && x.RelationshipTieUp == tieUp && x.ServicesOffered == servicesOffered && x.Questionnaire == questionnaire).FirstOrDefaultAsync();
            return query;
        }

        public async Task<IReadOnlyList<CustomerUserBusinessProfileRole>> GetCustomerUserBusinessProfilesRolesAsync(Specification<CustomerUserBusinessProfileRole> filters)
        {
            var query = this.dbContext.CustomerUserBusinessProfileRoles
                .Where(filters.ToExpression());

            /* No tracking queries are useful when the results are used in a read-only scenario.
               They're quicker to execute because there's no need to set up the change tracking information.*/
            return await query.AsNoTracking().ToListAsync();
        }

        public BusinessProfile AddBusinessProfiles(BusinessProfile businessProfile)
        {
            this.dbContext.BusinessProfiles.Add(businessProfile);
            this.dbContext.SaveChanges();
            return businessProfile;
        }

        public async Task<BusinessProfile> AddBusinessProfilesAsync(BusinessProfile businessProfile)
        {
            this.dbContext.BusinessProfiles.Add(businessProfile);
            await this.dbContext.SaveChangesAsync();
            return businessProfile;
        }
        public async Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfileByIdAndCodeAsync(int userId, int businessProfileCode)
        {
            var customerBusinessProfile = await this.dbContext.CustomerUserBusinessProfiles.FirstOrDefaultAsync(x => x.UserId == userId && x.BusinessProfileCode == businessProfileCode);
            return customerBusinessProfile;
        }

        public async Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfileByBusinessProfileCodeAsync(int? businessProfileCode)
        {
            var customerBusinessProfile = await this.dbContext.CustomerUserBusinessProfiles
                .FirstOrDefaultAsync(x => x.BusinessProfileCode == businessProfileCode);
            return customerBusinessProfile;
        }
        public async Task<CustomerUserBusinessProfile> GetCustomerUserBusinessProfilesByUserIdAsync(long userId)
        {
            var customerBusinessProfile = await this.dbContext.CustomerUserBusinessProfiles.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            return customerBusinessProfile;
        }

        public async Task<List<CustomerUserBusinessProfile>> GetCustomerUserBusinessProfilesByIdAsync(long userId)
        {
            var customerBusinessProfile = await this.dbContext.CustomerUserBusinessProfiles.Where(x => x.UserId == userId).ToListAsync();
            return customerBusinessProfile;
        }

        public async Task<Result<CustomerUserBusinessProfile>> UpdateCustomerUserBusinessProfileStatusAsync(CustomerUserBusinessProfile customerUserBusinessProfile)
        {
            this.dbContext.Update(customerUserBusinessProfile);
            await this.dbContext.SaveChangesAsync();
            return customerUserBusinessProfile;
        }
        public void UpdateBusinessProfile(BusinessProfile businessProfile)
        {
            var _businessProfile = this.dbContext.Entry(businessProfile).State = EntityState.Modified;

            this.dbContext.SaveChanges();
        }

        public async Task<BusinessProfile> UpdateBusinessProfileAsync(BusinessProfile businessProfile)
        {
            this.dbContext.Update(businessProfile);

            if (businessProfile.KYCSubmissionStatus != null)
            {
                this.dbContext.Entry(businessProfile.KYCSubmissionStatus).State = EntityState.Unchanged;
            }
            //var _businessProfile = this.dbContext.Entry(businessProfile).State = EntityState.Modified;

            await this.dbContext.SaveChangesAsync();

            return businessProfile;
        }

        public CustomerUserBusinessProfile AddCustomerUserBusinessProfile(CustomerUserBusinessProfile customerUserBusinessProfile)
        {
            var _customerUserBusinessProfile = this.dbContext.Add(customerUserBusinessProfile).Entity;

            this.dbContext.SaveChanges();

            return _customerUserBusinessProfile;
        }

        public async Task<CustomerUserBusinessProfileRole> AddCustomerUserBusinessProfileRole(CustomerUserBusinessProfileRole customerUserBusinessProfileRole)
        {
            var _customerUserBusinessProfileRole = this.dbContext.Add(customerUserBusinessProfileRole).Entity;
            /*
            dbContext.Entry(_customerUserBusinessProfileRole.Role).State = EntityState.Unchanged;

            foreach (var navigation in dbContext.Entry(_customerUserBusinessProfileRole.Role).Navigations)
            {
                navigation.IsModified = false;
            }
            */
            //Ensure Role is not changed here as this only involves Businessprofile related Db Context
            /*
            foreach (var reference in dbContext.Entry(_customerUserBusinessProfileRole.Role).References)
            {
                if (reference.TargetEntry == null)
                {
                    continue;
                }

                reference.TargetEntry.State = EntityState.Unchanged;
            }
            */
            await this.dbContext.SaveChangesAsync();

            return _customerUserBusinessProfileRole;
        }

        public async Task<IReadOnlyList<BusinessProfile>> FindBusinessProfileByEmailAsync(Email email)
        {
            var _MappingQuery = from mapping in dbContext.CustomerUserBusinessProfiles
                                where mapping.CustomerUser.Email.Value == email.Value
                                select mapping.BusinessProfile;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<CustomerUserBusinessProfile> AddCustomerUserBusinessProfileMappingAsync(CustomerUserBusinessProfile mapping)
        {
            dbContext.CustomerUserBusinessProfiles.Add(mapping);

            //CustomerUser entity is not tracked by BusinessProfile context. So
            //we have to always exclude it 
            dbContext.Entry(mapping.CustomerUser).State = EntityState.Unchanged;

            foreach (var navigation in dbContext.Entry(mapping.CustomerUser).Navigations)
            {
                navigation.IsModified = false;
            }

            foreach (var reference in dbContext.Entry(mapping.CustomerUser).References)
            {
                if (reference.TargetEntry == null)
                {
                    continue;
                }

                reference.TargetEntry.State = EntityState.Unchanged;
            }

            try
            {
                await dbContext.SaveChangesAsync();
                return mapping;

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task UpdateCustomerUserBusinessProfileAsync(CustomerUserBusinessProfile customerUserBusinessProfile, CancellationToken cancellationToken)
        {
            var updateResult = this.dbContext.Attach(customerUserBusinessProfile);
            await this.dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateCustomerUserBusinessProfileRangeAsync(List<CustomerUserBusinessProfile> customerUserBusinessProfiles, CancellationToken cancellationToken)
        {
            this.dbContext.CustomerUserBusinessProfiles.AttachRange(customerUserBusinessProfiles);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<Result<CustomerUserBusinessProfile>> AddCustomerUserBusinessProfileAsync(CustomerUserBusinessProfile customerUserBusinessProfile)
        {
            //this.dbContext.Update(customerUserBusinessProfile);
            this.dbContext.Entry(customerUserBusinessProfile).State = EntityState.Added;

            await this.dbContext.SaveChangesAsync();

            return customerUserBusinessProfile;
        }

        public async Task<Result<CustomerUserBusinessProfileRole>> AddCustomerUserBusinessProfileRoleAsync(CustomerUserBusinessProfileRole customerUserBusinessProfileRole)
        {
            //this.dbContext.Update(customerUserBusinessProfileRole);
            this.dbContext.Entry(customerUserBusinessProfileRole).State = EntityState.Added;

            await this.dbContext.SaveChangesAsync();

            return customerUserBusinessProfileRole;
        }

        public async Task<Result<CustomerUserBusinessProfile>> DeleteCustomerUserBusinessProfileAsync(CustomerUserBusinessProfile customerUserBusinessProfile)
        {
            //this.dbContext.CustomerUserBusinessProfiles.Remove(customerUserBusinessProfile);
            this.dbContext.Entry(customerUserBusinessProfile).State = EntityState.Deleted;
            //CustomerUserBusinessProfile cubp = this.dbContext.CustomerUserBusinessProfiles.Single(x => x.Id == customerUserBusinessProfile.Id);
            //this.dbContext.CustomerUserBusinessProfiles.Remove(cubp);
            await this.dbContext.SaveChangesAsync();

            return customerUserBusinessProfile;
        }

        public async Task<Result<CustomerUserBusinessProfileRole>> DeleteCustomerUserBusinessProfileRoleAsync(CustomerUserBusinessProfileRole customerUserBusinessProfileRole)
        {
            //this.dbContext.CustomerUserBusinessProfileRoles.Remove(customerUserBusinessProfileRole);
            this.dbContext.Entry(customerUserBusinessProfileRole).State = EntityState.Deleted;
            //CustomerUserBusinessProfileRole cubpr = this.dbContext.CustomerUserBusinessProfileRoles.Single(x => x.Id == customerUserBusinessProfileRole.Id);
            //this.dbContext.CustomerUserBusinessProfileRoles.Remove(cubpr);
            await this.dbContext.SaveChangesAsync();

            return customerUserBusinessProfileRole;
        }

        async Task<Result<LicenseInformation>> IBusinessProfileRepository.AddLicenseInformationsAsync(LicenseInformation licenseInformation)
        {
            this.dbContext.LicenseInformations.Add(licenseInformation);
            await this.dbContext.SaveChangesAsync();
            return licenseInformation;
        }

        public Task<Result<BusinessProfile>> UpdateBusinessProfilesAsync(BusinessProfile businessProfile)
        {
            throw new NotImplementedException();
        }

        async Task<Result<LicenseInformation>> IBusinessProfileRepository.UpdateLicenseInformationsAsync(LicenseInformation updatelicenseInfo)
        {
            this.dbContext.LicenseInformations.Update(updatelicenseInfo);
            await this.dbContext.SaveChangesAsync();
            return updatelicenseInfo;
        }

        async Task<Result<COInformation>> IBusinessProfileRepository.AddCOInformationsAsync(COInformation coInfo)
        {
            this.dbContext.COInformations.Add(coInfo);
            await this.dbContext.SaveChangesAsync();
            return coInfo;
        }

        async Task<Result<COInformation>> IBusinessProfileRepository.UpdateCOInformationsAsync(COInformation cOInfo)
        {
            this.dbContext.COInformations.Update(cOInfo);
            await this.dbContext.SaveChangesAsync();
            return cOInfo;
        }


        async Task<Result<DocumentCommentBP>> IBusinessProfileRepository.AddCommentsAsync(DocumentCommentBP comments)
        {
            this.dbContext.DocumentCommentBPs.Add(comments);
            await this.dbContext.SaveChangesAsync();
            return comments;
        }



        public async Task<IReadOnlyList<IDType>> GetIDTypesAsync(Specification<IDType> filters)
        {
            var query = this.dbContext.IDTypes
               .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<BusinessProfileIDType>> GetBusinessProfileIDTypesAsync(Specification<BusinessProfileIDType> filters)
        {
            var query = this.dbContext.BusinessProfileIDTypes
               .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<BoardOfDirector> AddBoardOfDirectorAsync(BoardOfDirector boardOfDirector)
        {
            this.dbContext.Entry(boardOfDirector).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync();

            return boardOfDirector;
        }

        public async Task<IndividualLegalEntity> AddLegalEntityAsync(IndividualLegalEntity legalEntity)
        {
            this.dbContext.Attach(legalEntity);


            /*     if (legalEntity.Nationality != null )
                 {
                     dbContext.Entry(legalEntity.Nationality).State = EntityState.Unchanged;
                 }
     */


            await this.dbContext.SaveChangesAsync();
            return legalEntity;
        }

        public async Task<CompanyLegalEntity> AddLegalEntityAsync(CompanyLegalEntity legalEntity)
        {
            this.dbContext.Attach(legalEntity);
            /*
            if (legalEntity.Country != null)
            {
                dbContext.Entry(legalEntity.Country).State = EntityState.Unchanged;
            }
            */
            await this.dbContext.SaveChangesAsync();
            return legalEntity;
        }

        public async Task<PrimaryOfficer> AddPrimaryOfficerAsync(PrimaryOfficer primaryOfficer)
        {

            this.dbContext.Entry(primaryOfficer).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync();
            return primaryOfficer;
        }

        public async Task<ParentHoldingCompany> AddParentHoldingCompanyAsync(ParentHoldingCompany parentHoldingCompany)
        {
            this.dbContext.Attach(parentHoldingCompany);
            if (parentHoldingCompany.Country != null)
            {
                dbContext.Entry(parentHoldingCompany.Country).State = EntityState.Unchanged;
            }
            await this.dbContext.SaveChangesAsync();
            return parentHoldingCompany;
        }


        public async Task<LegalEntity> AddLegalEntityAsync(LegalEntity legalEntity)
        {

            this.dbContext.LegalEntities.Add(legalEntity);

            if (legalEntity is CompanyLegalEntity companyLegalEntity)
            {
                dbContext.Entry(companyLegalEntity.Country).State = EntityState.Unchanged;
            }
            else if (legalEntity is IndividualLegalEntity individualLegalEntity)
            {
                dbContext.Entry(individualLegalEntity.Nationality).State = EntityState.Unchanged;
                dbContext.Entry(individualLegalEntity.IDType).State = EntityState.Unchanged;
            }

            await this.dbContext.SaveChangesAsync();
            return legalEntity;
        }
        public async Task<Shareholder> AddShareholderAsync(Shareholder shareholder)
        {
            this.dbContext.Shareholders.Attach(shareholder);



            await this.dbContext.SaveChangesAsync();
            return shareholder;
        }


        public async Task<PoliticallyExposedPerson> AddPoliticallyExposedPersonAsync(PoliticallyExposedPerson politicallyExposedPerson)
        {
            this.dbContext.Entry(politicallyExposedPerson).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync();
            return politicallyExposedPerson;
        }

        public async Task<Result<AffiliateAndSubsidiary>> AddAffiliateAndSubsidiaryAsync(AffiliateAndSubsidiary affiliateAndSubsidiary)
        {
            this.dbContext.Attach(affiliateAndSubsidiary);
            if (affiliateAndSubsidiary.Country != null)
            {
                dbContext.Entry(affiliateAndSubsidiary.Country).State = EntityState.Unchanged;
            }

            await this.dbContext.SaveChangesAsync();
            return affiliateAndSubsidiary;
        }

        public async Task<Result<AuthorisedPerson>> AddAuthorisedPersonAsync(AuthorisedPerson authorisedPerson)
        {
            this.dbContext.Add(authorisedPerson);
            await this.dbContext.SaveChangesAsync();
            return authorisedPerson;
        }

        public async Task<IReadOnlyList<BoardOfDirector>> GetBoardOfDirectorByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.BoardOfDirectors
                                //.AsNoTracking()
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Shareholder)
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.Title)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<BoardOfDirector> GetBoardOfDirectorByCodeAsync(long boardOfDirectorCode)
        {
            var bod = dbContext.BoardOfDirectors
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Shareholder)
                                .FirstOrDefaultAsync(x => x.Id == boardOfDirectorCode);

            return await bod;
        }

        public async Task<IReadOnlyList<IndividualLegalEntity>> GetIndividualLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.IndividualLegalEntities
                                .AsNoTracking()
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(s => ((IndividualLegalEntity)s).Nationality)
                                .Include(s => ((IndividualLegalEntity)s).CountryOfResidence)
                                .Include(x => x.Shareholder)
                                .Include(s => ((IndividualLegalEntity)s).Title)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }
        public async Task<IReadOnlyList<CompanyLegalEntity>> GetCompanyLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.CompanyLegalEntities
                                .AsNoTracking()
                                .Include(s => ((CompanyLegalEntity)s).Country)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<IndividualLegalEntity> GetIndividualLegalEntityByTableIDAsync(long tableID)
        {
            var _MappingQuery = from mapping in dbContext.IndividualLegalEntities
                                .AsNoTracking()
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(s => ((IndividualLegalEntity)s).Nationality)
                                .Include(s => ((IndividualLegalEntity)s).CountryOfResidence)
                                where mapping.Id == tableID
                                select mapping;

            return await _MappingQuery.FirstOrDefaultAsync();
        }
        public async Task<CompanyLegalEntity> GetCompanyLegalEntityByTableIDAsync(long tableID)
        {
            var _MappingQuery = from mapping in dbContext.CompanyLegalEntities
                                .AsNoTracking()
                                .Include(s => ((CompanyLegalEntity)s).Country)
                                where mapping.Id == tableID
                                select mapping;

            return await _MappingQuery.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<PrimaryOfficer>> GetPrimaryOfficerByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.PrimaryOfficers
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Shareholder)
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.Title)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<PrimaryOfficer> GetPrimaryOfficerByCodeAsync(long primaryOfficerCode)
        {
            var _MappingQuery = dbContext.PrimaryOfficers
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Shareholder)
                                .FirstOrDefaultAsync(x => x.Id == primaryOfficerCode);

            return await _MappingQuery;
        }

        public async Task<IReadOnlyList<ParentHoldingCompany>> GetParentHoldingCompanyByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.ParentHoldingCompanies
                                // .AsNoTracking()
                                .Include(x => x.Country)
                                .Include(x => x.BusinessProfile)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<IReadOnlyList<IndividualShareholder>> GetIndividualShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.IndividualShareholders
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.AuthorisedPerson)
                                .Include(x => x.BoardOfDirector)
                                .Include(x => x.PrimaryOfficer)
                                .Include(x => x.UltimateBeneficialOwner)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<IndividualShareholder> GetIndividualShareholderByTableIDAsync(long tableID)
        {
            var _MappingQuery = from mapping in dbContext.IndividualShareholders
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.AuthorisedPerson)
                                .Include(x => x.BoardOfDirector)
                                .Include(x => x.PrimaryOfficer)
                                where mapping.Id == tableID
                                select mapping;

            return await _MappingQuery.FirstOrDefaultAsync();
        }

        public async Task<CompanyShareholder> GetCompanyShareholderByTableIDAsync(long tableID)
        {
            var _MappingQuery = from mapping in dbContext.CompanyShareholders/*.AsNoTracking()*/
                                .Include(x => x.Country)
                                where mapping.Id == tableID
                                select mapping;

            return await _MappingQuery.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<CompanyShareholder>> GetCompanyShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.CompanyShareholders/*.AsNoTracking()*/
                                .Include(x => x.Country)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }
        public async Task<IReadOnlyList<Shareholder>> GetShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.Shareholders//.AsNoTracking()
                                .Include(s => ((IndividualShareholder)s).IDType)
                                .Include(s => ((IndividualShareholder)s).Gender)
                                .Include(s => ((IndividualShareholder)s).CountryOfResidence)
                                .Include(s => ((IndividualShareholder)s).Nationality)
                                .Include(s => ((IndividualShareholder)s).Title)
                                .Include(s => ((CompanyShareholder)s).Country)
                                .Include(s => s.AuthorisedPerson)
                                .Include(s => s.PrimaryOfficer)
                                .Include(s => s.BoardOfDirector)
                                .Include(s => s.UltimateBeneficialOwner)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }
        public async Task<IReadOnlyList<LegalEntity>> GetLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.LegalEntities//.AsNoTracking()
                                .Include(s => ((IndividualLegalEntity)s).IDType).AsNoTracking()
                                .Include(s => ((IndividualLegalEntity)s).Gender).AsNoTracking()
                                .Include(s => ((IndividualLegalEntity)s).Nationality).AsNoTracking()
                                .Include(s => ((IndividualLegalEntity)s).CountryOfResidence).AsNoTracking()
                                .Include(s => ((IndividualLegalEntity)s).Title).AsNoTracking()
                                .Include(s => ((CompanyLegalEntity)s).Country).AsNoTracking()
                                  .Include(s => ((ShareholderIndividualLegalEntity)s).IDType).AsNoTracking()
                                .Include(s => ((ShareholderIndividualLegalEntity)s).Gender).AsNoTracking()
                                .Include(s => ((ShareholderIndividualLegalEntity)s).Nationality).AsNoTracking()
                                .Include(s => ((ShareholderIndividualLegalEntity)s).CountryOfResidence).AsNoTracking()
                                 .Include(s => ((ShareholderCompanyLegalEntity)s).Country).AsNoTracking()
                                .Include(s => ((ShareholderIndividualLegalEntity)s).Title).AsNoTracking()
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }
        public async Task<IReadOnlyList<PoliticallyExposedPerson>> GetPoliticallyExposedPersonByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.PoliticallyExposedPersons
                                /*.AsNoTracking()*/
                                .Include(x => x.Nationality).AsNoTracking()
                                .Include(x => x.Gender).AsNoTracking()
                                .Include(x => x.IDType).AsNoTracking()
                                .Include(x => x.CountryOfResidence).AsNoTracking()
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<IReadOnlyList<AffiliateAndSubsidiary>> GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.AffiliatesAndSubsidiaries
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.Country)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }
        //Phase 3 Change
        public async Task<IReadOnlyList<AuthorisedPerson>> GetAuthorisedPersonByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.AuthorisedPeople
                                .Include(x => x.AuthorisationLevel)
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.IDType)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Nationality)
                                .Include(x => x.Shareholder)
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.Title)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }


        public async Task<AuthorisedPerson> GetAuthorisedPersonByCodeAsync(long authorisedPersonCode)
        {
            var _MappingQuery = dbContext.AuthorisedPeople
                                .Include(x => x.AuthorisationLevel)
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.IDType)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Nationality)
                                .Include(x => x.Shareholder)
                                .FirstOrDefaultAsync(x => x.Id == authorisedPersonCode);

            return await _MappingQuery;
        }
        public async Task<AuthorisedPerson> GetAuthorisedPersonByDefaultAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = dbContext.AuthorisedPeople
                                .Include(x => x.AuthorisationLevel)
                                .Include(x => x.BusinessProfile)
                                .Include(x => x.IDType)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Nationality)
                                .Include(x => x.Shareholder)
                                .FirstOrDefaultAsync(x => x.BusinessProfile == businessProfile && x.IsDefault);

            return await _MappingQuery;
        }

        public async Task<Result<BoardOfDirector>> UpdateBoardOfDirectorAsync(BoardOfDirector boardOfDirector, CancellationToken cancellationToken)
        {

            //this.dbContext.Attach(boardOfDirector).State = EntityState.Modified;
            var entry = this.dbContext.Update(boardOfDirector);
            this.dbContext.Entry(boardOfDirector).Reference(e => e.CountryOfResidence).IsModified = true;
            this.dbContext.Entry(boardOfDirector).Reference(e => e.Nationality).IsModified = true;
            this.dbContext.Entry(boardOfDirector).Reference(e => e.Title).IsModified = true;
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return boardOfDirector;
        }

        public async Task<Result<IndividualLegalEntity>> UpdateLegalEntityAsync(IndividualLegalEntity legalEntity, CancellationToken cancellationToken)
        {
            var entry = this.dbContext.Update(legalEntity);
            this.dbContext.Entry(legalEntity).Reference(e => e.CountryOfResidence).IsModified = true;
            this.dbContext.Entry(legalEntity).Reference(e => e.Nationality).IsModified = true;
            this.dbContext.Entry(legalEntity).Reference(e => e.Title).IsModified = true;
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return legalEntity;
        }
        public async Task<Result<CompanyLegalEntity>> UpdateLegalEntityAsync(CompanyLegalEntity legalEntity, CancellationToken cancellationToken)
        {
            /*            this.dbContext.Attach(legalEntity).State = EntityState.Modified;
            */
            var entry = this.dbContext.Update(legalEntity);
            this.dbContext.Entry(legalEntity).Reference(e => e.Country).IsModified = true;
            /*
            if(legalEntity.Country != null)
            {
                dbContext.Entry(legalEntity.Country).State = EntityState.Unchanged;
            }
            */
            await this.dbContext.SaveChangesAsync(cancellationToken);


            return legalEntity;
        }
        public async Task<Result<PrimaryOfficer>> UpdatePrimaryOfficerAsync(PrimaryOfficer primaryOfficer, CancellationToken cancellationToken)
        {

            var entry = this.dbContext.Update(primaryOfficer);
            this.dbContext.Entry(primaryOfficer).Reference(e => e.CountryOfResidence).IsModified = true;
            this.dbContext.Entry(primaryOfficer).Reference(e => e.Nationality).IsModified = true;
            this.dbContext.Entry(primaryOfficer).Reference(e => e.Title).IsModified = true;
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return primaryOfficer;
        }

        public async Task<Result<ParentHoldingCompany>> UpdateParentHoldingCompanyAsync(ParentHoldingCompany parentHoldingCompany, CancellationToken cancellationToken)
        {
            //this.dbContext.Attach(parentHoldingCompany).State = EntityState.Modified;
            /*
            if (parentHoldingCompany.Country != null)
            {
                dbContext.Entry(parentHoldingCompany.Country).State = EntityState.Unchanged;
            }
            */
            var entry = this.dbContext.Update(parentHoldingCompany);
            this.dbContext.Entry(parentHoldingCompany).Reference(e => e.Country).IsModified = true;
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return parentHoldingCompany;
        }

        public async Task<Result<Shareholder>> UpdateShareholderAsync(Shareholder shareholder, CancellationToken cancellationToken)
        {
            if (shareholder is IndividualShareholder individualShareholder)
            {
                var entry = this.dbContext.Update(shareholder);
                this.dbContext.Entry(individualShareholder).Reference(e => e.CountryOfResidence).IsModified = true;
                this.dbContext.Entry(individualShareholder).Reference(e => e.Nationality).IsModified = true;
                this.dbContext.Entry(individualShareholder).Reference(e => e.Title).IsModified = true;
                await this.dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {

                //dbContext.Entry(shareholder.BusinessProfile).State = EntityState.Unchanged;
                //var update = this.dbContext.Update(shareholder);

                var entry = this.dbContext.Update(shareholder);

                await this.dbContext.SaveChangesAsync(cancellationToken);
            }
            return shareholder;
        }
        public async Task<Result<LegalEntity>> UpdateLegalEntityAsync(LegalEntity legalEntity, CancellationToken cancellationToken)
        {
            //dbContext.Entry(legalEntity.BusinessProfile).State = EntityState.Unchanged;
            var entry = this.dbContext.Update(legalEntity);

            await this.dbContext.SaveChangesAsync(cancellationToken);

            return legalEntity;
        }
        public async Task<Result<PoliticallyExposedPerson>> UpdatePoliticallyExposedPersonAsync(PoliticallyExposedPerson politicallyExposedPerson, CancellationToken cancellationToken)
        {
            /*
            var update = this.dbContext.Update(politicallyExposedPerson);
            update.Navigation(nameof(politicallyExposedPerson.Gender)).IsModified = true;
            update.Navigation(nameof(politicallyExposedPerson.IDType)).IsModified = true;
            */
            //this.dbContext.Attach(politicallyExposedPerson).State = EntityState.Modified;

            var entry = this.dbContext.Update(politicallyExposedPerson);
            this.dbContext.Entry(politicallyExposedPerson).Reference(e => e.CountryOfResidence).IsModified = true;
            this.dbContext.Entry(politicallyExposedPerson).Reference(e => e.Nationality).IsModified = true;
            await this.dbContext.SaveChangesAsync(cancellationToken);


            return politicallyExposedPerson;
        }

        public async Task<Result<AffiliateAndSubsidiary>> UpdateAffiliateAndSubsidiaryAsync(AffiliateAndSubsidiary affiliateAndSubsidiary, CancellationToken cancellationToken)
        {
            /*
            var entry = this.dbContext.Attach(affiliateAndSubsidiary);
			if (affiliateAndSubsidiary.Country != null)
			{
                dbContext.Entry(affiliateAndSubsidiary.Country).State = EntityState.Unchanged;
            }
            */
            //this.dbContext.Attach(affiliateAndSubsidiary).State = EntityState.Modified;

            var entry = this.dbContext.Update(affiliateAndSubsidiary);
            this.dbContext.Entry(affiliateAndSubsidiary).Reference(e => e.Country).IsModified = true;
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return affiliateAndSubsidiary;
        }

        public async Task<Result<AuthorisedPerson>> UpdateAuthorisedPerson(AuthorisedPerson authorisedPerson, CancellationToken cancellationToken)
        {
            this.dbContext.AuthorisedPeople.Update(authorisedPerson);
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return authorisedPerson;
        }

        public async Task DeleteAffiliateAndSubsidiaryAsync(IEnumerable<AffiliateAndSubsidiary> affiliateAndSubsidiary, CancellationToken cancellationToken)
        {
            foreach (var item in affiliateAndSubsidiary)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteBoardOfDirectorAsync(IEnumerable<BoardOfDirector> boardOfDirector, CancellationToken cancellationToken)
        {
            foreach (var item in boardOfDirector)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteIndividualLegalEntityAsync(IEnumerable<IndividualLegalEntity> legalEntity, CancellationToken cancellationToken)
        {
            foreach (var item in legalEntity)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteCompanyLegalEntityAsync(IEnumerable<CompanyLegalEntity> legalEntity, CancellationToken cancellationToken)
        {
            foreach (var item in legalEntity)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteIndividualShareholderAsync(IEnumerable<IndividualShareholder> shareholder, CancellationToken cancellationToken)
        {
            foreach (var item in shareholder)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteCompanyShareholderAsync(IEnumerable<CompanyShareholder> shareholder, CancellationToken cancellationToken)
        {
            foreach (var item in shareholder)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);

        }

        public async Task DeleteShareholderAsync(IEnumerable<Shareholder> shareholder, CancellationToken cancellationToken)
        {
            foreach (var item in shareholder)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);

        }
        public async Task DeleteLegalEntityAsync(IEnumerable<LegalEntity> legalEntity, CancellationToken cancellationToken)
        {
            foreach (var item in legalEntity)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);

        }

        public async Task DeleteParentHoldingCompanyAsync(IEnumerable<ParentHoldingCompany> parentHoldingCompany, CancellationToken cancellationToken)
        {
            foreach (var item in parentHoldingCompany)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }
            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeletePoliticallyExposedPersonAsync(IEnumerable<PoliticallyExposedPerson> politicallyExposedPerson, CancellationToken cancellationToken)
        {
            foreach (var item in politicallyExposedPerson)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeletePrimaryOfficerAsync(IEnumerable<PrimaryOfficer> primaryOfficer, CancellationToken cancellationToken)
        {
            foreach (var item in primaryOfficer)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }
            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteAuthorisedPersonAsync(IEnumerable<AuthorisedPerson> authorisedPerson, CancellationToken cancellationToken)
        {
            foreach (var item in authorisedPerson)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }
            await this.dbContext.SaveChangesAsync(cancellationToken);


        }

        public Task GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(Specification<AffiliateAndSubsidiary> documentSpec)
        {
            throw new NotImplementedException();
        }


        public async Task<Declaration> GetKYCDeclarationInfoAsync(int BusinessProfileCode)
        {
            return await dbContext.Declarations
                            .Where(x => x.BusinessProfile.Id == BusinessProfileCode)
                            .FirstOrDefaultAsync();
        }

        public async Task<BusinessUserDeclaration> GetKYCBusinessDeclarationInfoAsync(int BusinessProfileCode)
        {
            return await dbContext.BusinessUserDeclarations
                            .Where(x => x.BusinessProfileCode == BusinessProfileCode)
                            .FirstOrDefaultAsync();
        }

        public async Task<Declaration> InsertKYCDeclarationInfoAsync(Declaration declaration)
        {
            await dbContext.Declarations.AddAsync(declaration);
            await dbContext.SaveChangesAsync();
            return declaration;
        }

        public async Task<Declaration> UpdateKYCDeclarationInfoAsync(Declaration declaration)
        {
            dbContext.Declarations.Update(declaration);
            await dbContext.SaveChangesAsync();
            return declaration;
        }

        public async Task<bool> SubmitKYCAsync(BusinessProfile BusinessProfile, List<DocumentCategoryBP> documentCategoryBPs)
        {
            try
            {
                //using (var _Transaction = await dbContext.Database.BeginTransactionAsync())
                //{

                //}
                foreach (var item in documentCategoryBPs)
                {
                    var updateResult = dbContext.DocumentCategoryBPs.Update(item);
                }
                dbContext.BusinessProfiles.Update(BusinessProfile);
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

            //var documentCategory = dbContext.DocumentCategoryBPs.Where(f => f.BusinessProfileCode==BusinessProfile.Id).ToList();
            //documentCategory.ForEach(a => a.DocumentCategoryBPStatus = status);
            //await dbContext.SaveChangesAsync();
            //return true;
        }

        public async Task<bool> SubmitBusinessKYCAsync(BusinessProfile BusinessProfile, List<DocumentCategoryBP> documentCategoryBPs)
        {
            try
            {
                foreach (var item in documentCategoryBPs)
                {
                    var updateResult = dbContext.DocumentCategoryBPs.Update(item);
                }
                dbContext.BusinessProfiles.Update(BusinessProfile);
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Result<DocumentCategoryBP>> AddCategoryBPAsync(DocumentCategoryBP categoryBP)
        {
            this.dbContext.DocumentCategoryBPs.Add(categoryBP);
            await this.dbContext.SaveChangesAsync();
            return categoryBP;
        }

        public async Task<Result<DocumentCommentUploadBP>> AddReviewRemarkAsync(DocumentCommentUploadBP commentBP)
        {
            this.dbContext.DocumentCommentUploadBPs.Add(commentBP);
            await this.dbContext.SaveChangesAsync();
            return commentBP;
        }



        public async Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryAsync(Specification<DocumentCategory> filters)
        {
            var query = this.dbContext.DocumentCategories
                    .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<DocumentCategoryBP> GetDocumentCategoryAsync(int documentCategoryCode)
        {
            return await dbContext.DocumentCategoryBPs.Where(x => x.DocumentCategoryCode == documentCategoryCode).FirstOrDefaultAsync();
        }

        public async Task<DocumentCommentUploadBP> GetReviewRemarkByCommentCode(long categoryBPCode)
        {
            return await dbContext.DocumentCommentUploadBPs.Where(x => x.DocumentCommentBPCode == categoryBPCode).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<DocumentCategoryBP>> GetDocumentCategoryBPAsync(Specification<DocumentCategoryBP> filters)
        {
            var query = this.dbContext.DocumentCategoryBPs
                    .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<DocumentCategoryBP>> GetDocumentCategoryBPAsync(int businessProfileCode)
        {
            var query = this.dbContext.DocumentCategoryBPs
                .Where(x => x.BusinessProfileCode == businessProfileCode);

            return await query.ToListAsync();
        }

        public Task<Result<DocumentCategoryBP>> GetCategoryBPyCategoryCodeAsync(int documentCategoryCode)
        {
            throw new NotImplementedException();
        }

        /*  public Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryAsync(int documentCategoryCode)
          {
              throw new NotImplementedException();
          }*/

        public Task<IReadOnlyList<DocumentCategory>> GetBusinessProfilesAsync(object categoryProfileSpec)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentCategoryBP> GetDocumentCategoryBPAMLCFTAsync(BusinessProfile businessProfile, Solution solution, CustomerType customerType)
        {
            int? customerTypeGroupCode;

            if (solution == Solution.Connect)
            {
                // When the solution is "Connect," set customerTypeGroupCode to 0
                customerTypeGroupCode = 0;
            }
            else
            {
                // For other solutions, use the original value of customerTypeGroupCode
                customerTypeGroupCode = customerType is null ? 0 : customerType.CustomerTypeGroupCode;
            }

            var query = this.dbContext.DocumentCategoryBPs
                .Include(x => x.BusinessProfile)
                .Include(x => x.DocumentCategory)
                .ThenInclude(x => x.DocumentCategoryGroup)
                .Where(x => x.DocumentCategory.IsAMLCFT == true
                    && x.DocumentCategory.DocumentCategoryGroup.Solution == solution);

            query = query.Where(x => x.DocumentCategory.DocumentCategoryGroup.CustomerTypeGroupCode == customerTypeGroupCode
                        || customerTypeGroupCode == 0);

            var documentCategoryBP = await query.FirstOrDefaultAsync(x => x.BusinessProfile == businessProfile);

            return documentCategoryBP;
        }
        public async Task<AMLCFTQuestionnaireAnswer> GetAMLCFTQuestionnaireAnswerAsync(BusinessProfile businessProfile)
        {
            var amlCFTQuestionnnaireAnswer = this.dbContext.AMLCFTQuestionnaireAnswers
                                                    .Include(x => x.AMLCFTQuestionnaire)
                                                    .ThenInclude(x => x.BusinessProfile)
                                                    .FirstOrDefaultAsync(x => x.AMLCFTQuestionnaire.BusinessProfile == businessProfile);

            return await amlCFTQuestionnnaireAnswer;
        }
        public async Task<Questionnaire> GetQuestionnaireByCodeAsync(long questionnaireCode)
        {
            var questionnaire = this.dbContext.Questionnaires.FirstOrDefaultAsync(x => x.Id == questionnaireCode);

            return await questionnaire;
        }
        public async Task<IEnumerable<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswersByQuestionnaireAsync(AMLCFTQuestionnaire aMLCFTQuestionnaire)
        {
            var amlCFTQuestionnnaireAnswer = this.dbContext.AMLCFTQuestionnaireAnswers
                                                    .Include(x => x.AMLCFTQuestionnaire)
                                                    .Where(x => x.AMLCFTQuestionnaire == aMLCFTQuestionnaire)?.ToListAsync();

            return await amlCFTQuestionnnaireAnswer;
        }

        public async Task<DocumentUploadBP> GetDocumentUploadBPAsync(DocumentCategoryBP documentCategoryBP)
        {
            //throw new NotImplementedException();
            var documentUpload = this.dbContext.DocumentUploadBPs
                                    .Include(x => x.DocumentCategoryBP)
                                    .FirstOrDefaultAsync(x => x.DocumentCategoryBP == documentCategoryBP);

            return await documentUpload;
        }

        public Task<Result<DocumentUploadBP>> UploadDocumentsAsync(DocumentUploadBP documentUpload)
        {
            throw new NotImplementedException();
        }

        public Task UploadDocumentAsync(DocumentUploadBP documentUpload)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentCategoryBP> GetDocumentCategoryAsync(int businessProfileCode, int documentCategoryCode)
        {
            return await dbContext.DocumentCategoryBPs.Where(x => x.BusinessProfile.Id == businessProfileCode && x.DocumentCategoryCode == documentCategoryCode).FirstOrDefaultAsync();
        }

        public async Task<DocumentCommentUploadBP> GetReviewRemarkDocumentAsync(int documentCommentBPCode)
        {
            return await dbContext.DocumentCommentUploadBPs.Where(x => x.DocumentCommentBPCode == documentCommentBPCode).FirstOrDefaultAsync();
        }

        public async Task<DocumentUploadBP> AddDocumentUploadAsync(DocumentUploadBP document)
        {
            this.dbContext.DocumentUploadBPs.Add(document);
            await this.dbContext.SaveChangesAsync();
            return document;
        }

        public async Task<List<DocumentUploadBP>> AddListDocumentUploadAsync(List<DocumentUploadBP> documentUploadBPs)
        {
            this.dbContext.DocumentUploadBPs.AddRange(documentUploadBPs);
            await this.dbContext.SaveChangesAsync();
            return documentUploadBPs;
        }

        public async Task<DocumentCategoryTemplate> AddDocumentTemplateAsync(DocumentCategoryTemplate documentTemplate)
        {
            this.dbContext.DocumentCategoryTemplates.Add(documentTemplate);
            await this.dbContext.SaveChangesAsync();
            return documentTemplate;
        }

        public async Task<AMLCFTDisplayRules> AddAMLCFTDisplayRules(AMLCFTDisplayRules displayRules)
        {
            this.dbContext.AMLCFTDisplayRule.Add(displayRules);
            await this.dbContext.SaveChangesAsync();
            return displayRules;
        }
        public async Task<AMLCFTDisplayRules> UpdateAMLCFTDisplayRules(AMLCFTDisplayRules displayRules)
        {
            this.dbContext.AMLCFTDisplayRule.Attach(displayRules);
            this.dbContext.Entry(displayRules).State = EntityState.Modified;
            await this.dbContext.SaveChangesAsync();

            return displayRules;
        }

        public async Task<DocumentUploadBP> GetDocumentUploadDataAsync(Guid documentId, long id)
        {
            return await dbContext.DocumentUploadBPs.Where(x => x.DocumentCategoryBPCode == id && x.DocumentId == documentId).FirstOrDefaultAsync();
        }

        public async Task<DocumentUploadBP> DeleteDocumentUploadBP(DocumentUploadBP documentUploadBP)
        {
            this.dbContext.DocumentUploadBPs.Remove(documentUploadBP);
            await this.dbContext.SaveChangesAsync();
            return documentUploadBP;
        }

        public async Task<RequisitionRunningNumber> GetRequisitionRunningNumberLatest()
        {
            return await dbContext.RequisitionRunningNumbers.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }
        public async Task<RequisitionRunningNumber> AddRequisitionRunningNumber(RequisitionRunningNumber requisitionRunningNumber)
        {
            this.dbContext.RequisitionRunningNumbers.Add(requisitionRunningNumber);
            await this.dbContext.SaveChangesAsync();
            return requisitionRunningNumber;
        }
        public async Task<RequisitionRunningNumber> UpdateRequisitionRunningNumber(RequisitionRunningNumber requisitionRunningNumber)
        {
            this.dbContext.RequisitionRunningNumbers.Update(requisitionRunningNumber);
            await this.dbContext.SaveChangesAsync();
            return requisitionRunningNumber;
        }
        public async Task<DocumentCategoryBP> GetDocumentCategoryBPAsync(long documentCategoryCode, int businessProfileCode)
        {
            return await dbContext.DocumentCategoryBPs.Where(x => x.DocumentCategoryCode == documentCategoryCode && x.BusinessProfileCode == businessProfileCode).FirstOrDefaultAsync();
        }

        public async Task<DocumentUploadBP> GetDocumentUploadDocumentId(long id)
        {
            return await dbContext.DocumentUploadBPs.Where(x => x.DocumentCategoryBPCode == id).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<DocumentUploadBP>> GetDocumentUploadBPProfile(Specification<DocumentUploadBP> filters)
        {
            var query = this.dbContext.DocumentUploadBPs
                 .Where(filters.ToExpression());

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<DocumentCategoryBP>> GetCategoryBPProfile(Specification<DocumentCategoryBP> filters)
        {
            var query = this.dbContext.DocumentCategoryBPs
                 .Where(filters.ToExpression());

            return await query.ToListAsync();
        }



        Task IBusinessProfileRepository.GetDocumentUploadDataAsync(Guid documentId, long id)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentUploadBP> GetDocumentUploadByIdAsync(long id, Guid documentId)
        {
            return await dbContext.DocumentUploadBPs.Where(x => x.DocumentCategoryBPCode == id && x.DocumentId == documentId).FirstOrDefaultAsync();
        }

        #region AML/CFT Add and Updates

        public async Task<AMLCFTQuestionnaire> AddAMLCFTQuestionnaireQuestionsAsync(AMLCFTQuestionnaire amlCFTQuestionnaire, CancellationToken cancellationToken)
        {
            this.dbContext.Entry(amlCFTQuestionnaire).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync(cancellationToken);
            return amlCFTQuestionnaire;
        }

        public async Task<AMLCFTQuestionnaireAnswer> AddAMLCFTQuestionnaireAnswersAsync(AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer, CancellationToken cancellationToken)
        {
            this.dbContext.Entry(amlCFTQuestionnaireAnswer.AMLCFTQuestionnaire).State = EntityState.Unchanged;
            this.dbContext.Entry(amlCFTQuestionnaireAnswer).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync(cancellationToken);
            return amlCFTQuestionnaireAnswer;
        }

        public async Task<AMLCFTQuestionnaireAnswer> UpdateAMLCFTQuestionnaireAnswersAsync(AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer, CancellationToken cancellationToken)
        {
            this.dbContext.Entry(amlCFTQuestionnaireAnswer.AMLCFTQuestionnaire).State = EntityState.Unchanged;
            this.dbContext.Entry(amlCFTQuestionnaireAnswer).State = EntityState.Modified;
            await this.dbContext.SaveChangesAsync(cancellationToken);
            return amlCFTQuestionnaireAnswer;
        }

        #endregion

        public async Task<AMLCFTQuestionnaire> GetAMLCFTQuestionAsync(BusinessProfile businessProfile, Question question)
        {
            /*var _MappingQuery = from mapping in dbContext.AMLCFTQuestionnaires
                                where mapping.BusinessProfile == businessProfile 
                                      && 
                                      mapping.Question == question
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();*/

            var query = await this.dbContext.AMLCFTQuestionnaires.FirstOrDefaultAsync(x => x.Question == question && x.BusinessProfile == businessProfile);

            return query;
        }

        public async Task<AMLCFTQuestionnaireAnswer> GetAMLCFTAnswerAsync(AMLCFTQuestionnaire amlCFTQuestionnaire, AnswerChoice answerChoice, string answerRemark)
        {
            /*var _MappingQuery = from mapping in dbContext.AMLCFTQuestionnaireAnswer
                                where mapping.AnswerChoice == answerChoice
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();*/

            //Check if need to add AMLCFTQuestionnaireAnswers in dbcontext
            var query = await this.dbContext.AMLCFTQuestionnaireAnswers.FirstOrDefaultAsync(x => x.AMLCFTQuestionnaire == amlCFTQuestionnaire && x.AnswerChoice == answerChoice && x.AnswerRemark == answerRemark);

            return query;
        }
        public async Task<Question> GetAMLCFTQuestionByQuestionCodeAsync(int questionCode)
        {
            /*var _MappingQuery = from mapping in dbContext.Questions
                                where mapping.Id == questionCode
                                select mapping;*/

            var query = await this.dbContext.Questions.FirstOrDefaultAsync(x => x.Id == questionCode);

            return query;
        }

        public async Task<AnswerChoice> GetAMLCFTAnswerChoiceAsync(int answerChoiceCode)
        {
            /*var _MappingQuery = from mapping in dbContext.AMLCFTQuestionnaireAnswer
                                where mapping.Id == answerChoiceCode
                                select mapping;*/

            var query = await this.dbContext.AnswerChoices.FirstOrDefaultAsync(x => x.Id == answerChoiceCode);

            return query;
        }

        public async Task DeleteAMLCFTQuestionnaireAnswersAsync(IEnumerable<AMLCFTQuestionnaireAnswer> amlCFTQuestionnaireAnswers, CancellationToken cancellationToken)
        {
            foreach (var item in amlCFTQuestionnaireAnswers)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAMLCFTQuestionnairesAsync(IEnumerable<AMLCFTQuestionnaire> amlCFTQuestionnaires, CancellationToken cancellationToken)
        {
            foreach (var item in amlCFTQuestionnaires)
            {
                this.dbContext.Entry(item).State = EntityState.Deleted;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<IReadOnlyList<AMLCFTQuestionnaire>> GetAMLCFTQuestionnairesByBusinessProfileAsync(BusinessProfile businessProfile)
        {
            var amlCFTQuestionnaires = dbContext.AMLCFTQuestionnaires
                                                .Where(x => x.BusinessProfile == businessProfile)
                                                .Include(x => x.Question).ThenInclude(x => x.QuestionInputType);

            return await amlCFTQuestionnaires.ToListAsync();
        }
        public async Task<IReadOnlyList<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswersByBusinessProfileAsync(BusinessProfile businessProfile)
        {
            var amlCFTQuestionnaires = await GetAMLCFTQuestionnairesByBusinessProfileAsync(businessProfile);

            var amlCFTQuestionnaireAnswers = from amlAnswer in dbContext.AMLCFTQuestionnaireAnswers.AsEnumerable()
                                             join amlCFTQuestionnaire in amlCFTQuestionnaires.AsEnumerable()
                                             on amlAnswer.AMLCFTQuestionnaire?.Id equals amlCFTQuestionnaire.Id
                                             select amlAnswer;

            return amlCFTQuestionnaireAnswers.ToList();
        }

        public async Task<IReadOnlyList<AMLCFTQuestionnaire>> GetAMLCFTQuestionnairesByBusinessProfileWithIgnoreQuestionnaireAsync(BusinessProfile businessProfile, List<long> ignoreQuestionnaire)
        {
            List<AMLCFTQuestionnaire> aMLCFTQuestionnaires = new List<AMLCFTQuestionnaire>();

            var amlCFTQuestionnaires = dbContext.AMLCFTQuestionnaires
                                                           .Where(x => x.BusinessProfile == businessProfile)
                                                           .Include(x => x.Question).ThenInclude(x => x.QuestionInputType)
                                                           .Where(x => !ignoreQuestionnaire.Contains(x.Question.QuestionSection.Questionnaire.Id));

            aMLCFTQuestionnaires = await amlCFTQuestionnaires.ToListAsync();

            //if (ignoreQuestionnaire.Count>1 && ignoreQuestionnaire.Contains(0))
            //{
            //    ignoreQuestionnaire.RemoveAll(x => x == 0);
            //    var amlCFTQuestionnaires = dbContext.AMLCFTQuestionnaires
            //                                                   .Where(x => x.BusinessProfile == businessProfile)
            //                                                   .Include(x => x.Question).ThenInclude(x => x.QuestionInputType)
            //                                                   .Where(x => !ignoreQuestionnaire.Contains(x.Question.QuestionSection.Questionnaire.Id));

            //    aMLCFTQuestionnaires = await amlCFTQuestionnaires.ToListAsync();
            //}
            //else if(ignoreQuestionnaire.Count == 1 && ignoreQuestionnaire.Contains(0)) {

            //    var amlCFTQuestionnaires = dbContext.AMLCFTQuestionnaires
            //                                    .Where(x => x.BusinessProfile == businessProfile)
            //                                    .Include(x => x.Question).ThenInclude(x => x.QuestionInputType);

            //    aMLCFTQuestionnaires = await amlCFTQuestionnaires.ToListAsync();
            //}

            return aMLCFTQuestionnaires;
        }

        public async Task<IReadOnlyList<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswersByBusinessProfileWithIgnoreQuestionnaireAsync(BusinessProfile businessProfile, List<long> ignoreQuestionnaire)
        {
            var amlCFTQuestionnaires = await GetAMLCFTQuestionnairesByBusinessProfileWithIgnoreQuestionnaireAsync(businessProfile, ignoreQuestionnaire);

            var amlCFTQuestionnaireAnswers = from amlAnswer in dbContext.AMLCFTQuestionnaireAnswers.AsEnumerable()
                                             join amlCFTQuestionnaire in amlCFTQuestionnaires.AsEnumerable()
                                             on amlAnswer.AMLCFTQuestionnaire?.Id equals amlCFTQuestionnaire.Id
                                             select amlAnswer;

            return amlCFTQuestionnaireAnswers.ToList();
        }


        public async Task<DocumentUploadBP> GetDocumentUploadByIdAsync(long id)
        {
            return await dbContext.DocumentUploadBPs.Where(x => x.DocumentCategoryBPCode == id).FirstOrDefaultAsync();
        }
        public async Task<DocumentCategoryBP> UpdateDocumentCategoryBP(DocumentCategoryBP checkCategoryBP)
        {
            this.dbContext.DocumentCategoryBPs.Update(checkCategoryBP);
            await this.dbContext.SaveChangesAsync();
            return checkCategoryBP;
        }

        public async Task<DocumentCommentUploadBP> UpdateDocumentCommentUploadBP(DocumentCommentUploadBP documentCommentUploadBP)
        {
            this.dbContext.DocumentCommentUploadBPs.Update(documentCommentUploadBP);
            await this.dbContext.SaveChangesAsync();
            return documentCommentUploadBP;
        }

        public async Task<DocumentCommentUploadBP> AddDocumentCommentUploadBP(DocumentCommentUploadBP documentCommentUploadBP)
        {
            this.dbContext.DocumentCommentUploadBPs.Add(documentCommentUploadBP);
            await this.dbContext.SaveChangesAsync();
            return documentCommentUploadBP;
        }

        public async Task<List<DocumentCommentBP>> GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(long documentCategoryBPCode)
        {
            return await dbContext.DocumentCommentBPs
                 .Where(x => x.DocumentCategoryBP.Id == documentCategoryBPCode)
                 .ToListAsync();
        }

        public async Task<List<DocumentCommentBP>> AddDocumentCommentBP(List<DocumentCommentBP> documentCommentBP)
        {
            this.dbContext.DocumentCommentBPs.AddRange(documentCommentBP);
            await this.dbContext.SaveChangesAsync();
            return documentCommentBP;
        }

        public async Task<COInformation> GetCOInfoByBusinessCode(int businessProfileCode)
        {
            return await dbContext.COInformations
                        .Where(c => c.BusinessProfileCode == businessProfileCode)
                        .FirstOrDefaultAsync();
        }

        public async Task<AMLCFTDisplayRules> GetAMLCFTDisplayRulesByCodeAsync(long displayRuleCode)
        {
            return await dbContext.AMLCFTDisplayRule
                        .Include(x => x.EntityType)
                        .Include(x => x.ServicesOffered)
                        .Include(x => x.RelationshipTieUp)
                        .Where(c => c.Id == displayRuleCode)
                        .FirstOrDefaultAsync();
        }
        public async Task<Result<AMLCFTDisplayRules>> DeleteAMLCFTDisplayRulesAsync(AMLCFTDisplayRules displayRules)
        {
            this.dbContext.AMLCFTDisplayRule.Remove(displayRules);
            await this.dbContext.SaveChangesAsync();

            return displayRules;
        }

        public async Task<LicenseInformation> GetLicenseInfoByBusinessCode(int businessProfileCode)
        {
            return await dbContext.LicenseInformations
                         .Where(c => c.BusinessProfileCode == businessProfileCode)
                         .FirstOrDefaultAsync();

        }

        public async Task<long> GetDocumentCategoryGroupIdBySolution(long? solutionCode)
        {
            return await dbContext.DocumentCategoryGroups.Where(x => x.Solution.Id == solutionCode)
                         .Select(x => x.Id).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryByGroupId(long documentCategoryGroupId)
        {
            return await dbContext.DocumentCategories
                  .Where(x => x.DocumentCategoryGroup.Id == documentCategoryGroupId).ToListAsync();
        }

        public async Task<bool> AddKYCSubmoduleReviews(List<KYCSubModuleReview> kYCSubModuleReviews)
        {
            try
            {
                foreach (var item in kYCSubModuleReviews)
                {
                    var updateResult = dbContext.kYCSubModuleReviews.Attach(item);
                }
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<DocumentUploadBP> UpdateDocumentUploadBP(DocumentUploadBP documentInfo)
        {
            this.dbContext.DocumentUploadBPs.Update(documentInfo);
            await this.dbContext.SaveChangesAsync();
            return documentInfo;
        }

        public async Task<DocumentCategoryBP> UpdateDocumentCategoryBPInfo(DocumentCategoryBP documentCategoryInfo)
        {
            this.dbContext.DocumentCategoryBPs.Update(documentCategoryInfo);
            await this.dbContext.SaveChangesAsync();
            return documentCategoryInfo;
        }

        public async Task<DocumentReleaseBP> GetDocumentReleasedByIdAsync(int businessProfileCode, Guid documentId)
        {
            return await dbContext.DocumentReleaseBPs.
                Where(x => x.BusinessProfile.Id == businessProfileCode && x.DocumentId == documentId)
                .FirstOrDefaultAsync();
        }

        public async Task<DocumentReleaseBP> UpdateDocumentReleasedBP(DocumentReleaseBP documentReleasedInfo)
        {
            //this.dbContext.DocumentReleaseBPs.Update(documentReleasedInfo);

            this.dbContext.Entry(documentReleasedInfo).State = EntityState.Added;
            await this.dbContext.SaveChangesAsync();

            return documentReleasedInfo;
        }

        public async Task<DocumentReleaseBP> AddDocumentReleasedUploadAsync(DocumentReleaseBP documentReleasedUpload)
        {
            this.dbContext.DocumentReleaseBPs.Add(documentReleasedUpload);
            await this.dbContext.SaveChangesAsync();
            return documentReleasedUpload;
        }

        public async Task<KYCSubModuleReview> SaveKYCSubModuleReview(KYCSubModuleReview kYCSubModuleReview)
        {
            this.dbContext.kYCSubModuleReviews.Attach(kYCSubModuleReview);

            await this.dbContext.SaveChangesAsync();
            return kYCSubModuleReview;
        }

        public async Task<KYCSubModuleReview> GetKYCSubModuleReviewByBusinessProfileCategory(BusinessProfile businessProfile, KYCCategory kYCCategory)
        {
            return await this.dbContext.kYCSubModuleReviews
                                .Include(x => x.ReviewResult)
                                .FirstOrDefaultAsync(x => x.BusinessProfile == businessProfile
                                                          && x.KYCCategory == kYCCategory);
        }
        public async Task<KYCSubModuleReview> GetKYCSubModuleReviewByBusinessProfileCategoryNoTracking(BusinessProfile businessProfile, KYCCategory kYCCategory)
        {
            return await this.dbContext.kYCSubModuleReviews.AsNoTracking()
                                .Include(x => x.ReviewResult)
                                .FirstOrDefaultAsync(x => x.BusinessProfile == businessProfile
                                                          && x.KYCCategory == kYCCategory);
        }
        public async Task<List<KYCSubModuleReview>> GetKYCSubModuleReviewByBusinessProfile(BusinessProfile businessProfile)
        {
            return await this.dbContext.kYCSubModuleReviews.AsNoTracking()
                                .Include(x => x.ReviewResult)
                                .Where(x => x.BusinessProfile == businessProfile).ToListAsync();
        }
        public async Task<bool> SaveKYCSubModuleReviewList(BusinessProfile businessProfile, List<KYCSubModuleReview> kycSubModuleReviewUpdateList)
        {
            try
            {
                var _KYCSubModuleReviewList = dbContext.kYCSubModuleReviews.Where(x => x.BusinessProfile == businessProfile).ToList();

                foreach (var item in kycSubModuleReviewUpdateList)
                {
                    var updateItem = _KYCSubModuleReviewList.FirstOrDefault(x => item.KYCCategoryCode == x.KYCCategoryCode);
                    updateItem.ReviewResult = item.ReviewResult;
                    updateItem.LastReviewedDate = item.LastReviewedDate;
                    this.dbContext.Entry(updateItem.ReviewResult).State = EntityState.Modified;
                    await this.dbContext.SaveChangesAsync();
                }


                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public async Task<Result<KYCSummaryFeedback>> SaveKYCSummaryFeedback(KYCSummaryFeedback kycSummaryFeedback)
        {
            this.dbContext.KYCSummaryFeedback.Attach(kycSummaryFeedback);
            if (kycSummaryFeedback.Id != 0)
            {
                this.dbContext.Entry(kycSummaryFeedback).State = EntityState.Modified;
            }
            await this.dbContext.SaveChangesAsync();

            return kycSummaryFeedback;
        }

        public async Task<Result<KYCCustomerSummaryFeedback>> SaveKYCCustomerSummaryFeedback(KYCCustomerSummaryFeedback kycCustomerSummaryFeedback)
        {
            this.dbContext.KYCCustomerSummaryFeedbacks.Attach(kycCustomerSummaryFeedback);
            if (kycCustomerSummaryFeedback.Id != 0)
            {
                this.dbContext.Entry(kycCustomerSummaryFeedback).State = EntityState.Modified;
            }
            await this.dbContext.SaveChangesAsync();

            return kycCustomerSummaryFeedback;
        }


        public async Task<List<EmailRecipient>> GetRecipientEmail(long bccType, long notificationTemplate)
        {
            return await dbContext.EmailRecipients.
              Where(x => x.RecipientType.Id == bccType
                && x.NotificationTemplateCode == notificationTemplate)
              .ToListAsync();
        }

        public async Task<List<EmailRecipient>> GetRecipientEmailByCollectionTier(long collectionTierCode, long recipientType, long notificationTemplate)
        {
            return await dbContext.EmailRecipients.
              Where(x => x.CollectionTierCode == collectionTierCode
                && x.RecipientType.Id == recipientType
                && x.NotificationTemplateCode == notificationTemplate)
              .ToListAsync();
        }

        public async Task<List<EmailRecipient>> GetRecipientEmailByAuthorityLevel(long authorityLevelCode, long recipientTypeCode, long notificationTemplateCode)
        {
            return await dbContext.EmailRecipients.
               Where(x => x.AuthorityLevelCode == authorityLevelCode
                && x.RecipientType.Id == recipientTypeCode
                && x.NotificationTemplateCode == notificationTemplateCode)
               .ToListAsync();
        }

        public async Task<CustomerUserBusinessProfile> GetCustomerUserBPById(int userId, int businessProfileCode)
        {
            var customerBusinessProfile = await this.dbContext.CustomerUserBusinessProfiles.Where(x => x.UserId == userId && x.BusinessProfileCode == businessProfileCode).FirstOrDefaultAsync();
            return customerBusinessProfile;
        }

        public async Task<Result<CustomerUser>> EditIsTPNUser(CustomerUser customerUser)
        {
            dbContext.CustomerUsers.Update(customerUser);
            await this.dbContext.SaveChangesAsync();
            return customerUser;
        }

        public async Task<InternalDocumentUpload> AddInternalDocumentUploadAsync(InternalDocumentUpload internalDocumentUpload)
        {
            this.dbContext.InternalDocumentUploads.Add(internalDocumentUpload);
            await this.dbContext.SaveChangesAsync();
            return internalDocumentUpload;
        }

        public async Task<InternalDocumentUpload> GetInternalDocumentUploadByDocumentIdAsync(InternalDocumentUpload checkFileUpload)
        {
            return await dbContext.InternalDocumentUploads.Where(x => x.DocumentId == checkFileUpload.DocumentId).FirstOrDefaultAsync();
        }

        public async Task<InternalDocumentUpload> RemoveInternalDocumentUploadAsync(InternalDocumentUpload documentUploadResult)
        {
            this.dbContext.InternalDocumentUploads.Update(documentUploadResult);
            await this.dbContext.SaveChangesAsync();
            return documentUploadResult;
        }

        public async Task<Questionnaire> GetQuestionnaireByQuestionnaireCodeAsync(long questionnaireCode)
        {
            return await dbContext.Questionnaires.Where(x => x.Id == questionnaireCode).FirstOrDefaultAsync();
        }

        public async Task<QuestionnaireSolution> GetQuestionnaireSolutionsByQuestionnaireCodeAsync(long questionnaireCode)
        {
            return await dbContext.QuestionnaireSolutions
                    .Include(x => x.Solution)
                    .Include(x => x.Questionnaire)
                    .Where(x => x.Questionnaire.Id == questionnaireCode).FirstOrDefaultAsync();
        }

        public async Task<QuestionnaireSolution> GetQuestionnaireSolutionByQuestionnaireAndSolutionAsync(Questionnaire questionnaireId, Solution solutionId)
        {
            // Perform the necessary database query to retrieve the questionnaire solution
            var questionnaireSolution = await dbContext.QuestionnaireSolutions
                .FirstOrDefaultAsync(qs => qs.Questionnaire == questionnaireId && qs.Solution == solutionId);

            return questionnaireSolution;
        }


        public async Task<Questionnaire> GetQuestionnaireByQuestionnaireDescriptionAsync(string questionnaireDescription)
        {
            return await dbContext.Questionnaires.Where(x => x.Description == questionnaireDescription).FirstOrDefaultAsync();
        }

        public async Task<List<QuestionSection>> GetQuestionSectionsByQuestionnaireCodeAsync(long questionnaireCode)
        {
            var questionSections = this.dbContext.QuestionSections
                                            .Include(x => x.Questionnaire)
                                            .Where(x => x.Questionnaire.Id == questionnaireCode)?.ToListAsync();

            return await questionSections;
        }

        public async Task<QuestionSection> GetQuestionSectionByQuestionSectionCodeAsync(long questionSectionCode)
        {
            var questionSection = this.dbContext.QuestionSections
                                            .Where(x => x.Id == questionSectionCode).FirstOrDefaultAsync();

            return await questionSection;
        }

        public async Task<List<Question>> GetQuestionsByQuestionSectionAsync(List<QuestionSection> questionSection)
        {
            List<Question> questions = new List<Question>();

            var question = this.dbContext.Questions
                                        .Include(x => x.QuestionSection)
                                        .Where(x => questionSection.Contains(x.QuestionSection));

            questions = await question.ToListAsync();

            return questions;
        }

        public async Task<List<AnswerChoice>> GetAnswerChoicesByQuestionAsync(List<Question> question)
        {
            List<AnswerChoice> answerChoices = new List<AnswerChoice>();

            var answerChoice = this.dbContext.AnswerChoices
                                        .Include(x => x.Question)
                                        .Where(x => question.Contains(x.Question));

            answerChoices = await answerChoice.ToListAsync();

            return answerChoices;
        }

        public async Task<Result<Questionnaire>> UpdateQuestionnaireStatusAsync(Questionnaire questionnaire)
        {
            this.dbContext.Entry(questionnaire).State = EntityState.Modified;
            await this.dbContext.SaveChangesAsync();

            return questionnaire;
        }

        public async Task<Result<Questionnaire>> AddOrUpdateQuestionnaireAsync(Questionnaire questionnaire, int action)
        {
            var questionnairedescriptionfrominput = questionnaire.Description.ToLower().Trim();
            var checkingexistingquestionnaire = await GetQuestionnaireByQuestionnaireDescriptionAsync(questionnairedescriptionfrominput);

            if (checkingexistingquestionnaire != null)
            {
                var questionnairedescriptionfromdb = checkingexistingquestionnaire.Description.ToLower().Trim();


                if (questionnairedescriptionfromdb == questionnairedescriptionfrominput)
                    return Result.Failure<Questionnaire>
                        ($"Questionnaire: {checkingexistingquestionnaire.Description} already exists.");
                else
                {
                    if (action == 1)
                    {
                        this.dbContext.Entry(questionnaire).State = EntityState.Added;
                    }

                    else if (action == 2)
                    {
                        this.dbContext.Questionnaires.Update(questionnaire);
                    }
                }
            }
            else
            {
                if (action == 1)
                {
                    this.dbContext.Entry(questionnaire).State = EntityState.Added;
                }

                else if (action == 2)
                {
                    this.dbContext.Questionnaires.Update(questionnaire);
                }
            }

            await this.dbContext.SaveChangesAsync();
            return questionnaire;
        }

        public async Task<Result<List<QuestionnaireSolution>>> UpdateQuestionnaireSolutionAsync(List<QuestionnaireSolution> questionnaire)
        {
            this.dbContext.QuestionnaireSolutions.AddRange(questionnaire);
            await this.dbContext.SaveChangesAsync();
            return questionnaire;
        }



        public async Task<Result<List<QuestionSection>>> AddOrUpdateQuestionSectionAsync(List<QuestionSection> questionSections, int action)
        {
            if (action == 1)
            {
                foreach (var qs in questionSections)
                {
                    this.dbContext.Entry(qs).State = EntityState.Added;
                }

            }
            else if (action == 2)
            {
                foreach (var qs in questionSections)
                {
                    this.dbContext.QuestionSections.Update(qs);
                }
            }

            await this.dbContext.SaveChangesAsync();
            return questionSections;
        }

        public async Task<Result<List<Question>>> AddOrUpdateQuestionsAsync(List<Question> questions, int action)
        {
            if (action == 1)
            {
                foreach (var q in questions)
                {
                    this.dbContext.Entry(q).State = EntityState.Added;
                }
            }
            else if (action == 2)
            {
                foreach (var q in questions)
                {
                    this.dbContext.Entry(q).State = EntityState.Modified;
                }
            }

            await this.dbContext.SaveChangesAsync();
            return questions;
        }

        public async Task<Result<List<AnswerChoice>>> AddOrUpdateAnswerChoicesAsync(List<AnswerChoice> answerChoices, int action)
        {
            if (action == 1)
            {
                foreach (var a in answerChoices)
                {
                    this.dbContext.Entry(a).State = EntityState.Added;
                }
            }
            else if (action == 2)
            {
                foreach (var a in answerChoices)
                {
                    this.dbContext.Entry(a).State = EntityState.Modified;
                }
            }

            await this.dbContext.SaveChangesAsync();
            return answerChoices;
        }

        public async Task<List<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswerByAnswerChoiceAsync(List<AnswerChoice> answerChoices)
        {
            List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers = new List<AMLCFTQuestionnaireAnswer>();

            var aMLCFTQuestionnaireAnswer = this.dbContext.AMLCFTQuestionnaireAnswers
                                        .Include(x => x.AnswerChoice)
                                        .Where(x => answerChoices.Contains(x.AnswerChoice));

            aMLCFTQuestionnaireAnswers = await aMLCFTQuestionnaireAnswer.ToListAsync();

            return aMLCFTQuestionnaireAnswers;
        }

        public async Task<Result<List<AMLCFTQuestionnaireAnswer>>> DeleteAMLCFTQuestionnaireAnswersByAnswerChoiceAsync(List<AnswerChoice> answerChoices)
        {
            var deletedAMLCFTQuestionnaireAnswers = await GetAMLCFTQuestionnaireAnswerByAnswerChoiceAsync(answerChoices);
            //this.dbContext.Entry(deletedAMLCFTQuestionnaireAnswers).State = EntityState.Deleted;

            if (deletedAMLCFTQuestionnaireAnswers.Count >= 1)
            {
                foreach (var a in deletedAMLCFTQuestionnaireAnswers)
                {
                    this.dbContext.Entry(a).State = EntityState.Deleted;
                }

                await this.dbContext.SaveChangesAsync();
            }
            return deletedAMLCFTQuestionnaireAnswers;
        }

        public async Task<List<AMLCFTQuestionnaire>> GetAMLCFTQuestionnaireByQuestionAsync(List<Question> questions)
        {
            List<AMLCFTQuestionnaire> aMLCFTQuestionnaires = new List<AMLCFTQuestionnaire>();

            var aMLCFTQuestionnaire = this.dbContext.AMLCFTQuestionnaires
                                        .Include(x => x.Question)
                                        .Where(x => questions.Contains(x.Question));

            aMLCFTQuestionnaires = await aMLCFTQuestionnaire.ToListAsync();

            return aMLCFTQuestionnaires;
        }

        public async Task<List<AMLCFTQuestionnaireAnswer>> GetAMLCFTQuestionnaireAnswerAsync(List<AMLCFTQuestionnaire> aMLCFTQuestionnaires)
        {
            List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers = new List<AMLCFTQuestionnaireAnswer>();

            var aMLCFTQuestionnaireAnswer = this.dbContext.AMLCFTQuestionnaireAnswers
                                        .Include(x => x.AMLCFTQuestionnaire)
                                        .Where(x => aMLCFTQuestionnaires.Contains(x.AMLCFTQuestionnaire));

            aMLCFTQuestionnaireAnswers = await aMLCFTQuestionnaireAnswer.ToListAsync();

            return aMLCFTQuestionnaireAnswers;
        }

        public async Task<bool> DeleteAMLCFTQuestionnaireAnswerAsync(List<AMLCFTQuestionnaireAnswer> amlCFTQuestionnaireAnswers)
        {
            try
            {
                if (amlCFTQuestionnaireAnswers.Count > 0)
                {
                    foreach (var item in amlCFTQuestionnaireAnswers)
                    {
                        this.dbContext.Entry(item).State = EntityState.Deleted;
                    }

                    await this.dbContext.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Result<List<AMLCFTQuestionnaire>>> DeleteAMLCFTQuestionnairesByQuestionAsync(List<Question> questions)
        {
            try
            {
                var deletedAMLCFTQuestionnaires = await GetAMLCFTQuestionnaireByQuestionAsync(questions);
                var deletedAMLCFTQuestionnaireAnswers = await GetAMLCFTQuestionnaireAnswerAsync(deletedAMLCFTQuestionnaires);

                var deleteAMLCFTQuestionnaireAnswers = await DeleteAMLCFTQuestionnaireAnswerAsync(deletedAMLCFTQuestionnaireAnswers);

                //this.dbContext.Entry(deletedAMLCFTQuestionnaires).State = EntityState.Deleted;

                foreach (var a in deletedAMLCFTQuestionnaires)
                {
                    this.dbContext.Entry(a).State = EntityState.Deleted;
                }

                await this.dbContext.SaveChangesAsync();
                return deletedAMLCFTQuestionnaires;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAnswerChoicesAsync(List<AnswerChoice> answerChoices)
        {
            try
            {
                bool result = false;
                if (answerChoices.Count > 0)
                {
                    var deletedAMLCFTQuestionnaireAnswers = await DeleteAMLCFTQuestionnaireAnswersByAnswerChoiceAsync(answerChoices);

                    if (deletedAMLCFTQuestionnaireAnswers.IsSuccess)
                    {
                        foreach (var a in answerChoices)
                        {
                            dbContext.Remove(a);
                            //await dbContext.SaveChangesAsync();
                        }

                        await this.dbContext.SaveChangesAsync();
                        result = true;
                        return result;
                    }
                    else
                    {
                        result = false;
                        return result;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteQuestionsAsync(List<Question> questions)
        {
            try
            {
                if (questions.Count > 0)
                {
                    var deletedAMLCFTQuestionnaires = await DeleteAMLCFTQuestionnairesByQuestionAsync(questions);
                    if (deletedAMLCFTQuestionnaires.IsSuccess)
                    {
                        foreach (var q in questions)
                        {
                            this.dbContext.Entry(q).State = EntityState.Deleted;
                        }

                        await this.dbContext.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Result<List<QuestionSection>>> DeleteQuestionSectionsAsync(List<QuestionSection> questionSections)
        {
            if (questionSections.Count > 0)
            {
                foreach (var qs in questionSections)
                {
                    this.dbContext.Entry(qs).State = EntityState.Deleted;
                }

                await this.dbContext.SaveChangesAsync();
            }
            return questionSections;
        }

        public async Task<Result<List<QuestionnaireSolution>>> DeleteQuestionnaireSolutionAsync(List<QuestionnaireSolution> questionnaireSolutions)

        {
            var questionnaireIds = questionnaireSolutions.Select(qs => qs.Questionnaire).Distinct().ToList();
            var solutionIds = questionnaireSolutions.Select(qs => qs.Solution).Distinct().ToList();

            var existingRecords = await this.dbContext.QuestionnaireSolutions
                .Where(qs => questionnaireIds.Contains(qs.Questionnaire) && solutionIds.Contains(qs.Solution))
                .ToListAsync();

            this.dbContext.QuestionnaireSolutions.RemoveRange(existingRecords);
            await this.dbContext.SaveChangesAsync();

            return questionnaireSolutions;

        }


        public async Task<Result<List<QuestionnaireSolution>>> AddQuestionnaireSolutionAsync(List<QuestionnaireSolution> questionnaireSolution)
        {
            this.dbContext.AddRange(questionnaireSolution);
            await this.dbContext.SaveChangesAsync();
            return questionnaireSolution;
        }



        public async Task<UserVerificationToken> FindUserTokenByEmailAsync(string email)
        {
            return await dbContext.UserVerificationTokens.Where(x => x.Email == email).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }

        public async Task<UserVerificationToken> AddUserVerificationToken(UserVerificationToken userVerificationToken)
        {
            dbContext.UserVerificationTokens.Add(userVerificationToken);
            await this.dbContext.SaveChangesAsync();
            return userVerificationToken;
        }

        public async Task<UserVerificationToken> UpdateUserVerificationToken(UserVerificationToken updateUserVerificationToken)
        {
            dbContext.UserVerificationTokens.Update(updateUserVerificationToken);
            await this.dbContext.SaveChangesAsync();
            return updateUserVerificationToken;
        }
        public async Task<Question> GetQuestionByQuestionCodeAsync(long questionCode)
        {
            var question = this.dbContext.Questions
                .FirstOrDefaultAsync(x => x.Id == questionCode);

            return await question;
        }

        public async Task<AnswerChoice> GetAnswerChoiceByAnswerChoiceCodeAsync(long answerChoiceCode)
        {
            var answerChoice = this.dbContext.AnswerChoices.FirstOrDefaultAsync(x => x.Id == answerChoiceCode);

            return await answerChoice;
        }

        public async Task<DocumentCategoryTemplate> UpdateDocumentTemplateAsync(DocumentCategoryTemplate documentTemplate)
        {
            dbContext.DocumentCategoryTemplates.Update(documentTemplate);
            await this.dbContext.SaveChangesAsync();
            return documentTemplate;
        }


        public async Task<DocumentCategoryTemplate> GetAMLATemplateInfo(long questionnaireCode)
        {
            return await dbContext.DocumentCategoryTemplates
                         .Where(c => c.QuestionnaireCode == questionnaireCode)
                         .FirstOrDefaultAsync();
        }


        public async Task<DocumentCategory> GetCategoryInfo(int documentCategoryCode)
        {
            return await dbContext.DocumentCategories
                        .Where(c => c.Id == documentCategoryCode)
                        .FirstOrDefaultAsync();
        }

        public async Task<DocumentCategoryTemplate> GetTemplateInfo(long id, long? questionnaireCode)
        {
            return await dbContext.DocumentCategoryTemplates
                .Where(c => c.DocumentCategoryCode == id && c.QuestionnaireCode == questionnaireCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Solution>> GetSolutionsByUserAsync(long userId)
        {
            var roleCodes = await dbContext.CustomerUserBusinessProfileRoles
                .Include(x => x.CustomerUserBusinessProfile)
                .Where(x => x.CustomerUserBusinessProfile.UserId == userId)
                .Select(x => x.RoleCode)
                .ToListAsync();

            var solutions = new List<Solution>();

            foreach (var r in roleCodes)
            {
                var solution = await dbContext.ExternalUserRoles
                .Include(x => x.Solution)
                .Where(x => x.RoleCode == r)
                .Select(x => x.Solution)
                .FirstOrDefaultAsync();

                solutions.Add(solution);
            }

            solutions = solutions.GroupBy(x => x.Id)
                     .Select(g => g.First())
                     .ToList();

            return solutions;
        }

        public async Task<CustomerType> GetCustomerTypeByCode(long? customerTypeCode)
        {
            return await dbContext.CustomerTypes
                .Where(c => c.Id == customerTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceType> GetServiceTypeByCode(long? serviceTypeCode)
        {
            return await dbContext.ServiceTypes
                .Where(c => c.Id == serviceTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<CollectionTier> GetCollectionTierByCode(long? collectionTierCode)
        {
            return await dbContext.CollectionTiers
                .Where(c => c.Id == collectionTierCode)
                .FirstOrDefaultAsync();
        }

        public async Task<IDType> GetIDTypeByCode(long? IDTypeCode)
        {
            return await dbContext.IDTypes
                .Where(c => c.Id == IDTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<BusinessProfileIDType> GetBusinessProfileIDTypeByCode(long? BusinessProfileIDTypeCode)
        {
            return await dbContext.BusinessProfileIDTypes
                .Where(c => c.Id == BusinessProfileIDTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<AuthorisationLevel> GetAuthorisationLevelCodeByCode(long? authorisationLevelCode)
        {
            return await dbContext.AuthorisationLevels
                .Where(c => c.Id == authorisationLevelCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<KYCCategory>> GetKYCConnectCategories()
        {
            return await dbContext.KYCCategories
                .Where(x => x.SolutionCode == Solution.Connect.Id)
                .ToListAsync();
        }

        public async Task<List<KYCCategory>> GetKYCBusinessCategories()
        {
            return await dbContext.KYCCategories
                .Where(x => x.SolutionCode == Solution.Business.Id)
                .ToListAsync();
        }
        public async Task<List<KYCCategory>> GetKYCCategoriesByCustomerTypeGroupCodeAsync(int customerTypeGroupCode)
        {
            return await dbContext.KYCCategoryCustomerTypes
                .Include(x => x.KYCCategory)
                .Where(x => x.CustomerTypeGroupCode == customerTypeGroupCode)
                .Select(x => x.KYCCategory)
                .ToListAsync();
        }

        public async Task<RelationshipTieUp> GetRelationshipTieUpByCodeAsync(long? relationshipTieUpCode)
        {
            return await dbContext.RelationshipTieUps
                .Where(a => a.Id == relationshipTieUpCode)
                .FirstOrDefaultAsync();
        }

        public async Task<IncorporationCompanyType> GetIncorporationCompanyTypeByCodeAsync(long? incorporationCompanyTypeCode)
        {
            return await dbContext.IncorporationCompanyTypes
                .Where(a => a.Id == incorporationCompanyTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<BusinessNature> GetBusinessNatureByCodeAsync(long? businessNatureCode)
        {
            return await dbContext.BusinessNatures
                .Where(a => a.Id == businessNatureCode)
                .FirstOrDefaultAsync();
        }
        public async Task<ServicesOffered> GetServicesOfferedByCodeAsync(long? servicesOfferedTypeCode)
        {
            return await dbContext.ServicesOffered
                .Where(a => a.Id == servicesOfferedTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<EntityType> GetEntityTypeByCodeAsync(long? entityTypeCode)
        {
            return await dbContext.EntityTypes
                .Where(a => a.Id == entityTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DeclarationQuestion>> GetDeclarationQuestionsByCustomerTypeAsync(long customerTypeCode)
        {
            var query = await dbContext.DeclarationQuestions
                .Where(x => x.CustomerType.Id == customerTypeCode)
                .Include(x => x.CustomerType)
                .Include(x => x.DeclarationQuestionType)
                .ToListAsync();

            return query;
        }

        public async Task<CustomerBusinessDeclaration> AddCustomerBusinessDeclarationAsync(CustomerBusinessDeclaration customerBusinessDeclaration)
        {
            this.dbContext.CustomerBusinessDeclarations.Add(customerBusinessDeclaration);
            await this.dbContext.SaveChangesAsync();

            return customerBusinessDeclaration;
        }

        public async Task<List<CustomerBusinessDeclarationAnswer>> AddCustomerBusinessDeclarationAnswersAsync(List<CustomerBusinessDeclarationAnswer> customerBusinessDeclarationAnswers)
        {
            this.dbContext.CustomerBusinessDeclarationAnswers.AddRange(customerBusinessDeclarationAnswers);
            await this.dbContext.SaveChangesAsync();

            return customerBusinessDeclarationAnswers;
        }

        public async Task<CustomerBusinessDeclarationAnswer> GetCustomerBusinessDeclarationAnswerByCodeAsync(long customerBusinessDeclarationAnswerCode)
        {
            var query = await dbContext.CustomerBusinessDeclarationAnswers
                .Where(x => x.Id == customerBusinessDeclarationAnswerCode)
                .FirstOrDefaultAsync();

            return query;
        }

        public async Task<CustomerBusinessDeclarationAnswer> UpdateCustomerBusinessDeclarationAnswerAsync(CustomerBusinessDeclarationAnswer customerBusinessDeclarationAnswer)
        {
            this.dbContext.CustomerBusinessDeclarationAnswers.Update(customerBusinessDeclarationAnswer);
            await this.dbContext.SaveChangesAsync();

            return customerBusinessDeclarationAnswer;
        }

        public async Task<List<CustomerBusinessDeclarationAnswer>> GetCustomerBusinessDeclarationAnswersByCodeAsync(long customerBusinessDeclarationCode)
        {
            var query = await dbContext.CustomerBusinessDeclarationAnswers
                .Where(x => x.CustomerBusinessDeclarationCode == customerBusinessDeclarationCode)
                .ToListAsync();

            return query;
        }

        public async Task<List<CustomerBusinessDeclarationAnswer>> UpdateCustomerBusinessDeclarationAnswersAsync(List<CustomerBusinessDeclarationAnswer> customerBusinessDeclarationAnswers)
        {
            this.dbContext.CustomerBusinessDeclarationAnswers.UpdateRange(customerBusinessDeclarationAnswers);
            await this.dbContext.SaveChangesAsync();

            return customerBusinessDeclarationAnswers;
        }

        public async Task DeleteCustomerBusinessDeclarationAnswersAsync(List<CustomerBusinessDeclarationAnswer> customerBusinessDeclarationAnswers)
        {
            this.dbContext.CustomerBusinessDeclarationAnswers.RemoveRange(customerBusinessDeclarationAnswers);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<CustomerBusinessDeclaration> GetCustomerBusinessDeclarationByCodeAsync(long customerBusinessDeclarationCode)
        {
            var query = await dbContext.CustomerBusinessDeclarations
                .Include(x => x.BusinessDeclarationStatus)
                .FirstOrDefaultAsync(x => x.Id == customerBusinessDeclarationCode);

            return query;
        }

        public async Task<CustomerBusinessDeclaration> UpdateCustomerBusinessDeclarationAsync(CustomerBusinessDeclaration customerBusinessDeclaration)
        {
            this.dbContext.CustomerBusinessDeclarations.Update(customerBusinessDeclaration);
            await this.dbContext.SaveChangesAsync();

            return customerBusinessDeclaration;
        }

        public async Task<List<BusinessDeclarationRejectionMatrix>> GetRejectionMatrixesByCustomerTypeAsync(long customerTypeCode)
        {
            var query = await dbContext.BusinessDeclarationRejectionMatrixes
                .Where(x => x.CustomerTypeCode == customerTypeCode)
                .ToListAsync();

            return query;
        }

        public async Task<BusinessDeclarationStatus> GetBusinessDeclarationStatus(long businessDeclarationStatusCode)
        {
            var query = await dbContext.BusinessDeclarationStatuses
                .FirstOrDefaultAsync(x => x.Id == businessDeclarationStatusCode);

            return query;
        }

        public async Task<List<CustomerBusinessTransactionEvaluationAnswer>> GetCustomerBusinessTransactionEvaluationAnswersAsync(int businessProfileCode)
        {
            var query = await this.dbContext.CustomerBusinessTransactionEvaluationAnswers
                            .Include(x => x.TransactionEvaluationAnswerChoice)
                            .Where(x => x.BusinessProfileCode == businessProfileCode).ToListAsync();

            return query;
        }
        public async Task<CustomerBusinessTransactionEvaluationAnswer> GetCustomerBusinessTransactionEvaluationAnswersByIdAsync(int id)
        {
            var query = await this.dbContext.CustomerBusinessTransactionEvaluationAnswers
                                  .FirstOrDefaultAsync(x => x.Id == id);

            return query;
        }
        public void DeleteCustomerBusinessTransactionEvaluationAnswersByBusinessProfileCodeAsync(List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswers)
        {
            this.dbContext.CustomerBusinessTransactionEvaluationAnswers.RemoveRange(customerBusinessTransactionEvaluationAnswers);
            this.dbContext.SaveChanges();
        }
        public async Task<List<CustomerBusinessTransactionEvaluationAnswer>> AddCustomerBusinessTransactionEvaluationAnswer(List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswers)
        {
            await this.dbContext.CustomerBusinessTransactionEvaluationAnswers.AddRangeAsync(customerBusinessTransactionEvaluationAnswers);
            this.dbContext.SaveChanges();
            return customerBusinessTransactionEvaluationAnswers;
        }
        public void DeleteCustomerBusinessTransactionEvaluationAnswer(List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswers)
        {
            this.dbContext.CustomerBusinessTransactionEvaluationAnswers.RemoveRange(customerBusinessTransactionEvaluationAnswers);
            this.dbContext.SaveChanges();
        }
        public async Task<TransactionEvaluationAnswerChoice> GetTransactionEvaluationAnswerChoiceAsync(int transactionEvaluationAnswerChoiceCode)
        {
            var query = await this.dbContext.TransactionEvaluationAnswerChoices
                            .FirstOrDefaultAsync(x => x.Id == transactionEvaluationAnswerChoiceCode);

            return query;
        }
        public async Task<TransactionEvaluationQuestion> GetTransactionEvaluationQuestionAsync(int transactionEvaluationQuestionCode)
        {
            var query = await this.dbContext.TransactionEvaluationQuestions
                            .Include(x => x.TransactionEvaluationQuestionInputType)
                            .FirstOrDefaultAsync(x => x.Id == transactionEvaluationQuestionCode);

            return query;
        }

        public async Task<Gender> GetGenderTypeByCode(long? genderTypeCode)
        {
            return await dbContext.Genders
                .Where(c => c.Id == genderTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<CountryMeta> GetCountryMetaByISO2CodeAsync(string countryCodeISO2)
        {
            return await dbContext.CountryMetas
                .Where(a => a.CountryISO2 == countryCodeISO2)
                .FirstOrDefaultAsync();
        }
        public async Task<KYCStatus> GetKYCStatusByCodeAsync(long? kycStatusCode)
        {
            return await dbContext.KYCStatuses
                .Where(a => a.Id == kycStatusCode)
                .FirstOrDefaultAsync();
        }

        public async Task<Solution> GetSolutionByCodeAsync(long? solutionCode)
        {
            return await dbContext.Solutions
                .Where(a => a.Id == solutionCode)
                .FirstOrDefaultAsync();
        }
        public async Task<DocumentCategory> GetDocumentCategoriesAsync(long documentCategoryCode)
        {
            return await dbContext.DocumentCategories.Where(x => x.Id == documentCategoryCode).FirstOrDefaultAsync();
        }

        public async Task<DocumentCategory> GetDocumentCategoriesMappingTCAsync(int seqNo)
        {
            //DocumentCategoryGroup id = 1 is Remittance on TC
            return await dbContext.DocumentCategories
                .Where(x => x.DocumentCategoryGroup.Id == 1 && x.SequenceNo == seqNo)
                .FirstOrDefaultAsync();
        }

        public async Task<DocumentCategory> GetDocumentCategoriesMappingTBAsync(int seqNo)
        {
            //DocumentCategoryGroup id = 5 is Remittance on TB
            return await dbContext.DocumentCategories
                .Where(x => x.DocumentCategoryGroup.Id == 5 && x.SequenceNo == seqNo)
                .FirstOrDefaultAsync();
        }

        public async Task<long?> GetSolutionByNameAsync(string solutionCode)
        {
            var solutionId = await dbContext.Solutions
                .Where(a => a.Name == solutionCode)
                .Select(a => (long?)a.Id) // Return nullable long
                .FirstOrDefaultAsync();

            return solutionId;
        }

        //Verification Changes
        public async Task<IEnumerable<VerificationIDType>> GetAllVerificationIDTypes()
        {
            return await dbContext.VerificationIDTypes.ToListAsync();

        }

        public async Task<IEnumerable<VerificationIDTypeSection>> GetAllVerificationIDTypeSections()
        {
            var query = this.dbContext.VerificationIDTypeSections
               .Include(x => x.VerificationIDType).ToListAsync();

            return await query;




        }

        public async Task<IEnumerable<VerificationStatus>> GetAllVerificationStatus()
        {
            return await dbContext.VerificationStatuses.ToListAsync();

        }

        public async Task<IEnumerable<SubmissionResult>> GetAllSubmissionResult()
        {
            return await dbContext.SubmissionResults.ToListAsync();

        }

        public async Task<IEnumerable<RiskScore>> GetAllRiskScores()
        {
            return await dbContext.RiskScores.ToListAsync();

        }

        public async Task<IEnumerable<RiskType>> GetAllRiskTypes()
        {
            return await dbContext.RiskTypes.ToListAsync();

        }

        public async Task<CustomerVerification> GetCustomerVerificationbyBusinessProfileCodeAsync(int businessProfileCode)
        {
            var query = await this.dbContext.CustomerVerifications
                            .Include(x => x.EKYCVerificationStatus)
                            .Include(x => x.F2FVerificationStatus)
                            .Include(x => x.VerificationIDType)
                            .Include(x => x.RiskScore)
                            .Include(x => x.RiskType)
                            .Where(x => x.BusinessProfile.Id == businessProfileCode)
                            .FirstOrDefaultAsync();

            return query;
        }

        public async Task<VerificationStatus> GetVerificationStatusByCodeAsync(long? verificationStatusCode)
        {
            var query = await dbContext.VerificationStatuses
              .Where(x => x.Id == verificationStatusCode).FirstOrDefaultAsync();

            return query;
        }

        public async Task<VerificationIDType> GetVerificationIDByCodeAsync(long? verificationIDCode)
        {
            var query = await dbContext.VerificationIDTypes
              .Where(x => x.Id == verificationIDCode).FirstOrDefaultAsync();

            return query;
        }

        public async Task<RiskScore> GetRiskScoreByCodeAsync(long? riskScoreCode)
        {
            var query = await dbContext.RiskScores
              .Where(x => x.Id == riskScoreCode).FirstOrDefaultAsync();

            return query;
        }


        public async Task<RiskType> GetRiskTypeByCodeAsync(long? riskTypeCode)
        {
            var query = await dbContext.RiskTypes
              .Where(x => x.Id == riskTypeCode).FirstOrDefaultAsync();

            return query;
        }

        public async Task<CustomerVerification> AddCustomerVerificationAsync(CustomerVerification customerVerification)
        {
            this.dbContext.CustomerVerifications.Add(customerVerification);
            await this.dbContext.SaveChangesAsync();
            return customerVerification;
        }

        public async Task<CustomerVerification> UpdateCustomerVerificationAsync(CustomerVerification customerVerification)
        {
            this.dbContext.CustomerVerifications.Update(customerVerification);
            await this.dbContext.SaveChangesAsync();
            return customerVerification;
        }

        public async Task<List<CustomerVerificationDocuments>> DeleteCustomerVerificationDocumentsByCustomerVerificationCodeAsync(long? customerVerificationCode)
        {
            var documents = dbContext.CustomerVerificationDocuments
                .Where(d => d.CustomerVerification.Id == customerVerificationCode)
                .ToList();

            dbContext.CustomerVerificationDocuments.RemoveRange(documents);
            await dbContext.SaveChangesAsync();

            return documents;


        }

        public async Task<CustomerVerification> GetCustomerVerificationbyCustomerVerificationCodeAsync(long? customerVerification)
        {
            var query = await this.dbContext.CustomerVerifications
                .Include(x => x.BusinessProfile)
                .Include(x => x.RiskScore)
                .Include(x => x.RiskType)
                .Include(x => x.VerificationIDType)
                .Include(x => x.EKYCVerificationStatus)
                .Include(x => x.F2FVerificationStatus)
                .Where(x => x.Id == customerVerification).FirstOrDefaultAsync();

            return query;
        }

        public async Task<List<CustomerVerificationDocuments>> GetCustomerVerificationDocumentbyCustomerVerificationCodeAsync(long? customerVerification)
        {
            var query = await this.dbContext.CustomerVerificationDocuments
                .Where(x => x.CustomerVerification.Id == customerVerification)
                .ToListAsync();

            return query;
        }

        public async Task<VerificationIDTypeSection> GetVerificationIDTypeSectionsByCode(long? verificationIDTypeCode)
        {
            var query = await this.dbContext.VerificationIDTypeSections
                .Include(x => x.VerificationIDType)
                .Where(x => x.Id == verificationIDTypeCode)
                .FirstOrDefaultAsync();

            return query;
        }

        public async Task<CustomerVerificationDocuments> AddCustomerVerificationDocumentsAsync(CustomerVerificationDocuments customerVerificationDocuments)
        {
            this.dbContext.CustomerVerificationDocuments.Add(customerVerificationDocuments);
            await this.dbContext.SaveChangesAsync();

            return customerVerificationDocuments;
        }

        public async Task<List<CustomerVerificationDocuments>> GetCustomerVerificationDocumentsByVerificationCodeAsync(long? customerVerificationCode)
        {
            var query = await this.dbContext.CustomerVerificationDocuments
                .Where(x => x.CustomerVerification.Id == customerVerificationCode)
                .ToListAsync();

            return query;

        }

        public async Task<CustomerVerificationDocuments> GetCustomerVerificationDocumentByDocumentIdAsync(Guid? documentId)
        {
            var query = await this.dbContext.CustomerVerificationDocuments
                .Where(x => x.RawDocumentID == documentId)
                .FirstOrDefaultAsync();

            return query;
        }

        public async Task<CustomerVerificationDocuments> DeleteCustomerVerificationDocumentAsync(long? customerVerificationDocumentCode)
        {
            var document = await this.dbContext.CustomerVerificationDocuments
                .FirstOrDefaultAsync(x => x.Id == customerVerificationDocumentCode);

            if (document != null)
            {
                this.dbContext.CustomerVerificationDocuments.Remove(document);
                await this.dbContext.SaveChangesAsync();
            }

            return document;
        }

        public async Task<CustomerVerification> DeleteCustomerVerificationTemplateAsync(long? customerVerificationCode)
        {
            var document = await this.dbContext.CustomerVerifications
                .FirstOrDefaultAsync(x => x.Id == customerVerificationCode);

            if (document != null)
            {
                document.TemplateID = null; // Set the TemplateID to null
                await this.dbContext.SaveChangesAsync();
            }

            return document;
        }



        public async Task<CustomerVerificationDocuments> GetCustomerVerificationDocumentsByCustomerVerificationDocumentCode(long? customerVerificationDocumentCode)
        {
            var query = await this.dbContext.CustomerVerificationDocuments
            .Where(x => x.Id == customerVerificationDocumentCode)
            .Include(x => x.CustomerVerification)
            .Include(x => x.VerificationIDTypeSection).ThenInclude(x => x.VerificationIDType)
            .Include(x => x.SubmissionResult)
            .FirstOrDefaultAsync();

            return query;
        }

        public async Task<CustomerVerificationDocuments> UpdateCustomerVerificationDocumentsAsync(CustomerVerificationDocuments customerVerificationDocuments)
        {
            // Retrieve the existing entity from the database
            var existingDocuments = await dbContext.CustomerVerificationDocuments.FindAsync(customerVerificationDocuments.Id);

            if (existingDocuments != null)
            {
                // Update the properties of the existing entity with the new values
                existingDocuments.WatermarkDocumentID = customerVerificationDocuments.WatermarkDocumentID;
                existingDocuments.WatermarkDocumentName = customerVerificationDocuments.WatermarkDocumentName;

                // Save the changes to the database
                await dbContext.SaveChangesAsync();
            }

            return existingDocuments;
        }
        public async Task<CustomerVerificationDocuments> GetCustomerVerificationDocumentUploadByVerificationCodeAsync(long? customerVerificationCode)
        {
            var query = await this.dbContext.CustomerVerificationDocuments
           .Where(x => x.CustomerVerification.Id == customerVerificationCode)
           .Include(x => x.CustomerVerification)
           .Include(x => x.VerificationIDTypeSection).ThenInclude(x => x.VerificationIDType)
           .Include(x => x.SubmissionResult)
           .FirstOrDefaultAsync();

            return query;
        }



        //sprint 6
        public async Task<BusinessUserDeclaration> GetBusinessUserDeclarationByBusinessProfileCodeAsync(long? businessProfileCode)
        {
            var businessUserDeclaration = await dbContext.BusinessUserDeclarations
                .SingleOrDefaultAsync(x => x.BusinessProfileCode == businessProfileCode);

            return businessUserDeclaration;
        }

        public async Task<BusinessUserDeclaration> GetBusinessUserDeclarationByBusinessUserDeclarationCode(long? businessUserDeclarationCode)
        {
            var businessUserDeclaration = await dbContext.BusinessUserDeclarations
                .SingleOrDefaultAsync(x => x.Id == businessUserDeclarationCode);

            return businessUserDeclaration;
        }

        public async Task<Declaration> GetCustomerConnectDeclarationByDeclarationCode(long? declarationCode)
        {
            var connectUserDeclaration = await dbContext.Declarations
                .Include(x => x.BusinessProfile)
                .SingleOrDefaultAsync(x => x.Id == declarationCode);

            return connectUserDeclaration;
        }

        public async Task<BusinessUserDeclaration> AddBusinessUserDeclarationAsync(BusinessUserDeclaration businessUserDeclaration)
        {
            this.dbContext.BusinessUserDeclarations.Add(businessUserDeclaration);
            await this.dbContext.SaveChangesAsync();

            return businessUserDeclaration;
        }

        public async Task<BusinessUserDeclaration> UpdateBusinessUserDeclarationAsync(BusinessUserDeclaration businessUserDeclaration)
        {
            this.dbContext.BusinessUserDeclarations.Update(businessUserDeclaration);
            await this.dbContext.SaveChangesAsync();

            return businessUserDeclaration;
        }

        public async Task<BusinessUserDeclaration> DeleteBusinessUserDeclarationSignatureAsync(Guid? documentId)
        {
            var entity = await this.dbContext.BusinessUserDeclarations.FirstOrDefaultAsync(b => b.DocumentId == documentId);

            if (entity != null)
            {
                entity.DocumentId = null; // Set the DocumentId property to null as Delete removes the whole row
                await this.dbContext.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<Declaration> DeleteConnectUserDeclarationSignatureAsync(Guid? documentId)
        {
            var entity = await this.dbContext.Declarations.FirstOrDefaultAsync(b => b.DocumentId == documentId);

            if (entity != null)
            {
                entity.DocumentId = null; // Set the DocumentId property to null as Delete removes the whole row
                await this.dbContext.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<CustomerBusinessDeclaration> GetCustomerBusinessDeclarationByBusinessProfileCode(long? businessProfileCode)
        {
            var customerBusinessDeclaration = await this.dbContext.CustomerBusinessDeclarations
                .Include(x => x.BusinessDeclarationStatus)
                .FirstOrDefaultAsync(x => x.BusinessProfileCode == businessProfileCode);
            return customerBusinessDeclaration;
        }

        public async Task<SubmissionResult> GetSubmissionResultByCode(long submissionResultCode)
        {
            return await dbContext.SubmissionResults.FirstOrDefaultAsync(x => x.Id == submissionResultCode);
        }

        public async Task<Result<List<CustomerVerificationDocuments>>> UpdateCustomerVerificationDocumentsListAsync(List<CustomerVerificationDocuments> customerVerificationDocuments)
        {
            this.dbContext.CustomerVerificationDocuments.UpdateRange(customerVerificationDocuments);
            await dbContext.SaveChangesAsync();

            return customerVerificationDocuments;
        }

        public async Task<KYCSubmissionStatus> GetBusinessKYCSubmissionStatusBySubmissionStatusCode(long? submissionStatusCode)
        {
            return await dbContext.KYCSubmissionStatuses
               .Where(c => c.Id == submissionStatusCode)
               .FirstOrDefaultAsync();
        }

        public async Task<List<DocumentCategoryBP>> GetDocumentCategoryBPsByBusinessProfileCodeAsync(int businessProfileCode)
        {
            var query = await dbContext.DocumentCategoryBPs
                .Where(x => x.BusinessProfileCode == businessProfileCode)
                .ToListAsync();

            return query;
        }

        public async Task<DocumentCategoryBP> GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(int businessProfileCode, long documentCategoryCode)
        {
            var query = await dbContext.DocumentCategoryBPs
                .Where(x => x.BusinessProfileCode == businessProfileCode && x.DocumentCategoryCode == documentCategoryCode)
                .FirstOrDefaultAsync();

            return query;
        }

        public async Task<List<DocumentUploadBP>> GetDocumentUploadBPsAsync(long documentCategoryBPCode)
        {
            var query = await dbContext.DocumentUploadBPs
                 .Where(x => x.DocumentCategoryBPCode == documentCategoryBPCode)
                .ToListAsync();

            return query;
        }

        public async Task<List<DocumentUploadBP>> DeleteDocumentUploadBPs(List<DocumentUploadBP> documentUploadBPs)
        {
            this.dbContext.DocumentUploadBPs.RemoveRange(documentUploadBPs);
            await this.dbContext.SaveChangesAsync();
            return documentUploadBPs;
        }

        public async Task<List<DocumentCategoryBP>> AddDocumentCategoryBPs(List<DocumentCategoryBP> documentCategoryBPs)
        {
            this.dbContext.DocumentCategoryBPs.AddRange(documentCategoryBPs);
            await this.dbContext.SaveChangesAsync();
            return documentCategoryBPs;
        }

        public async Task<List<DocumentCategoryBP>> DeleteDocumentCategoryBPs(List<DocumentCategoryBP> documentCategoryBPs)
        {
            this.dbContext.DocumentCategoryBPs.RemoveRange(documentCategoryBPs);
            await this.dbContext.SaveChangesAsync();
            return documentCategoryBPs;
        }

        public async Task<List<ParentHoldingCompany>> GetParentHoldingCompanyListAsync(int businessProfileCode)
        {
            var query = await dbContext.ParentHoldingCompanies
                 .Where(x => x.BusinessProfile.Id == businessProfileCode)
                .ToListAsync();

            return query;
        }

        public async Task<List<PrimaryOfficer>> GetPrimaryOfficerListAsync(int businessProfileCode)
        {
            var query = await dbContext.PrimaryOfficers
                 .Where(x => x.BusinessProfile.Id == businessProfileCode)
                .ToListAsync();

            return query;
        }
        public async Task<List<AffiliateAndSubsidiary>> GetAffiliateAndSubsidiaryListAsync(int businessProfileCode)
        {
            var query = await dbContext.AffiliatesAndSubsidiaries
                 .Where(x => x.BusinessProfile.Id == businessProfileCode)
                .ToListAsync();

            return query;
        }

        public async Task<List<LegalEntity>> GetLegalEntityListAsync(int businessProfileCode)
        {
            var query = await dbContext.LegalEntities
                 .Where(x => x.BusinessProfile.Id == businessProfileCode)
                .ToListAsync();

            return query;
        }

        public async Task<LegalEntity> GetLegalEntityAsync(int businessProfileCode)
        {
            var query = await dbContext.LegalEntities
                .Where(x => x.BusinessProfile.Id == businessProfileCode).FirstOrDefaultAsync();

            return query;
        }

        public async Task<List<KYCSubModuleReview>> GetKYCSubModuleReviewList(int businessProfileCode)
        {
            var query = await dbContext.kYCSubModuleReviews
                                .Include(x => x.ReviewResult)
                                .Where(x => x.BusinessProfile.Id == businessProfileCode)
                                .ToListAsync();

            return query;
        }

        public async Task<List<KYCSubModuleReview>> DeleteKYCSubModuleReview(List<KYCSubModuleReview> kYCSubModuleReviews, int businessProfileCode)
        {
            this.dbContext.kYCSubModuleReviews.RemoveRange(kYCSubModuleReviews);
            await this.dbContext.SaveChangesAsync();

            //Check if deleted
            kYCSubModuleReviews = await GetKYCSubModuleReviewList(businessProfileCode);

            return kYCSubModuleReviews;
        }

        public async Task<List<KYCSubModuleReview>> AddKYCSubModuleReviewAsync(List<KYCSubModuleReview> kYCSubModuleReview)
        {
            this.dbContext.kYCSubModuleReviews.AttachRange(kYCSubModuleReview);

            await this.dbContext.SaveChangesAsync();
            return kYCSubModuleReview;
        }

        public async Task<ReviewResult> GetReviewResultAsync(long reviewResultCode)
        {
            var query = await this.dbContext.ReviewResults.FirstOrDefaultAsync(x => x.Id == reviewResultCode);
            return query;
        }

        //Screening List 
        public async Task<IEnumerable<ScreeningInput>> GetScreeningInputsByBusinessProfileIdAsync(long? businessProfileCode)
        {
            var screeningList = await dbContext.ScreeningInputs
                .Where(si => si.BusinessProfile.Id == businessProfileCode)
                .Include(x => x.OwnershipStrucureType)
                .Include(x => x.ScreeningEntityType)
                .Include(x => x.WatchlistStatus)
                .Include(x => x.Screenings).ThenInclude(x => x.ScreeningDetails).ThenInclude(x => x.ScreeningListSource)
                .Include(x => x.Screenings).ThenInclude(x => x.ScreeningDetails).ThenInclude(x => x.ScreeningDetailsCategory)
                .AsSingleQuery()
                .ToListAsync();

            return screeningList;
        }
        public async Task<Result<ScreeningInput>> UpdateSingleScreeningInputAsync(ScreeningInput screeningInput)
        {
            this.dbContext.Update(screeningInput);
            await this.dbContext.SaveChangesAsync();
            return screeningInput;
        }
        public async Task<Result<List<ScreeningInput>>> UpdateScreeningInputAsync(List<ScreeningInput> screeningInputs)
        {
            try
            {
                this.dbContext.UpdateRange(screeningInputs);
                return screeningInputs;
            }
            catch (Exception ex)
            {
                return Result.Failure<List<ScreeningInput>>(ex.Message);
            }
        }

        public async Task<Result<List<ScreeningInput>>> AddScreeningInputAsync(List<ScreeningInput> screeningInputs)
        {
            try
            {
                this.dbContext.AddRange(screeningInputs);
                return Result.Success(screeningInputs);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<ScreeningInput>>(ex.Message);
            }
        }

        public async Task<IEnumerable<ScreeningDetail>> GetScreeningDetailsByScreeningInputCode(long? screeningInputCode)
        {
            var query = this.dbContext.ScreeningDetails
                        .Where(si => si.Screening.Id == screeningInputCode)
                        .ToListAsync();

            return await query;

        }

        public async Task<Result<List<Screening>>> SaveScreening(List<Screening> screening)
        {
            try
            {
                dbContext.Screenings.AddRange(screening);
                await dbContext.SaveChangesAsync();

                return screening;
            }
            catch (Exception ex)
            {
                return Result.Failure<List<Screening>>(ex.Message);
            }
        }
        public async Task<ScreeningInput> GetScreeningInputByClientReferenceAsync(string clientReference)
        {
            // Parse the clientReference to int
            if (!int.TryParse(clientReference, out int parsedClientReference))
            {
                // Handle invalid clientReference here if needed
                return null;
            }
            // Query the database for the ScreeningInput with the parsed clientReference
            var screeningInput = await dbContext.ScreeningInputs
                .FirstOrDefaultAsync(si => si.Id == parsedClientReference);
            return screeningInput;
        }

        public async Task<WatchlistStatus> GetWatchlistStatusByWatchlistStatusCode(long? watchlistStatusCode)
        {
            var watchlistStatus = await dbContext.WatchlistStatuses
                .FirstOrDefaultAsync(ws => ws.Id == watchlistStatusCode);
            return watchlistStatus;
        }

        public async Task<Result<List<ScreeningDetail>>> SaveScreeningDetails(List<ScreeningDetail> screeningDetails)
        {
            try
            {
                dbContext.ScreeningDetails.AddRange(screeningDetails);
                await dbContext.SaveChangesAsync();
                return screeningDetails;
            }
            catch (Exception ex)
            {
                return Result.Failure<List<ScreeningDetail>>(ex.Message);
            }
        }

        public async Task<Screening> GetScreeningByReferenceCodeAsync(Guid referenceCode)
        {
            var query = await dbContext.Screenings
                 .Where(x => x.ReferenceCode == referenceCode).FirstOrDefaultAsync();
            return query;
        }

        public async Task<ScreeningDetail> GetScreeningDetailByEntityIdAsync(long screeningId, long entityId)
        {
            return await dbContext.ScreeningDetails
                .FirstOrDefaultAsync(sd => sd.Screening.Id == screeningId && sd.EntityId == entityId);
        }

        public async Task<ScreeningDetail> UpdateScreeningDetails(ScreeningDetail screeningDetails)
        {
            dbContext.ScreeningDetails.Update(screeningDetails);
            await dbContext.SaveChangesAsync();
            return screeningDetails;
        }

        public async Task<OwnershipStrucureType> GetOwnershipStructureTypeByCode(long? ownershipStructureTypeCode)
        {
            return await dbContext.OwnershipStrucureTypes
                .Where(a => a.Id == ownershipStructureTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<ScreeningEntityType> GetScreeningEntityTypeByCode(long? screeningEntityTypeCode)
        {
            return await dbContext.ScreeningEntityTypes
                .Where(a => a.Id == screeningEntityTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Screening>> GetLatestScreeningByScreeningInputCodes(List<int> screeningInputCodes)
        {
            // Get all latestScreeningsWithDetails for the given input codes, ordered by date
            var allScreenings = await dbContext.Screenings
                .Where(s => screeningInputCodes.Contains(s.ScreeningInput.Id))
                .OrderByDescending(s => s.ScreeningDate)
                .ToListAsync();

            // Group by ScreeningInput and take the first (latest) from each group
            var groupedScreenings = allScreenings
                .GroupBy(s => s.ScreeningInput.Id)
                .Select(g => g.First())
                .ToList();

            // Get the IDs of the latest screenings
            var latestScreeningIds = groupedScreenings.Select(s => s.Id).ToList();

            // Get Latest Screenings with Details
            var latestScreeningsWithDetails = await dbContext.Screenings
                .Where(s => latestScreeningIds.Contains(s.Id))
                .Include(s => s.ScreeningDetails)
                    .ThenInclude(d => d.ScreeningDetailsCategory)
                .Include(s => s.ScreeningDetails)
                    .ThenInclude(d => d.ScreeningListSource)
                .ToListAsync();

            return latestScreeningsWithDetails;
        }

        public async Task<CompanyUserAccountStatus> GetCompanyUserAccountStatus(long? companyUserAccountStatusCode)
        {
            return await dbContext.CompanyUserAccountStatuses
                .Where(a => a.Id == companyUserAccountStatusCode)
                .FirstOrDefaultAsync();
        }

        public async Task<CompanyUserBlockStatus> GetCompanyUserBlockStatus(long? companyUserBlockStatusCode)
        {
            return await dbContext.CompanyUserBlockStatuses
                .Where(a => a.Id == companyUserBlockStatusCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ChangeCustomerTypeDocumentUploadBP>> AddChangeCustomerTypeDocumentUploadBPs(List<ChangeCustomerTypeDocumentUploadBP> changeCustomerTypeDocumentUploadBPs)
        {
            this.dbContext.ChangeCustomerTypeDocumentUploadBPs.AttachRange(changeCustomerTypeDocumentUploadBPs);
            await this.dbContext.SaveChangesAsync();
            return changeCustomerTypeDocumentUploadBPs;
        }

        public async Task<ChangeCustomerTypeLicenseInformation> AddChangeCustomerTypeLicenseInformation(ChangeCustomerTypeLicenseInformation changeCustomerTypeLicenseInformation)
        {
            this.dbContext.ChangeCustomerTypeLicenseInformations.Attach(changeCustomerTypeLicenseInformation);
            await this.dbContext.SaveChangesAsync();
            return changeCustomerTypeLicenseInformation;
        }

        public async Task<ChangeCustomerTypeCOInformation> AddChangeCustomerTypeCOInformation(ChangeCustomerTypeCOInformation changeCustomerTypeCOInformation)
        {
            this.dbContext.ChangeCustomerTypeCOInformations.Attach(changeCustomerTypeCOInformation);
            await this.dbContext.SaveChangesAsync();
            return changeCustomerTypeCOInformation;
        }

        public async Task<DefaultTemplateDocument> GetDefaultTemplateDocumentAsync(long defaultTemplateCode)
        {
            return await dbContext.DefaultTemplateDocuments
                .Where(a => a.Id == defaultTemplateCode)
                .FirstOrDefaultAsync();
        }

        public async Task<DefaultTemplateDocument> AddDefaultTemplateDocumentAsync(DefaultTemplateDocument defaultTemplateDocument)
        {
            dbContext.DefaultTemplateDocuments.Add(defaultTemplateDocument);
            await dbContext.SaveChangesAsync();

            return defaultTemplateDocument;
        }

        public async Task<DefaultTemplateDocument> UpdateDefaultTemplateDocumentAsync(DefaultTemplateDocument defaultTemplateDocument)
        {
            dbContext.DefaultTemplateDocuments.Update(defaultTemplateDocument);
            await dbContext.SaveChangesAsync();

            return defaultTemplateDocument;
        }

        public async Task<DateTime?> GetLastReviewConcurrentModifiedTimestamp(long businessProfileCode)
        {
            var businessProfile = await dbContext.BusinessProfiles
                .Where(bp => bp.Id == businessProfileCode)
                .Select(bp => bp.ReviewConcurrentLastModified)
                .FirstOrDefaultAsync();

            return businessProfile;
        }
        public async Task<Shareholder> UpdateShareholdersPrimaryOfficerReferenceAsync(long? primaryOfficerCode)
        {
            // Retrieve the shareholder with the specified primaryOfficerCode
            var shareholderToUpdate = await dbContext.Shareholders
                .FirstOrDefaultAsync(s => s.PrimaryOfficer.Id == primaryOfficerCode);

            if (shareholderToUpdate != null)
            {
                // Remove the reference to the primary officer
                shareholderToUpdate.PrimaryOfficer = null;

                // Save changes to the database
                await dbContext.SaveChangesAsync();
            }

            return shareholderToUpdate;
        }

        public async Task<ShareholderCompanyLegalEntity> GetShareholderCompanyLegalEntityByLegalEntityCodeAsync(long id)
        {
            return await dbContext.ShareholderCompanyLegalEntities
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }


        public async Task<ShareholderCompanyLegalEntity> AddShareLegalEntityAsync(ShareholderCompanyLegalEntity shareholderLegalEntity)
        {
            this.dbContext.Attach(shareholderLegalEntity);

            await this.dbContext.SaveChangesAsync();
            return shareholderLegalEntity;
        }

        public async Task<ShareholderCompanyLegalEntity> DeleteShareholderLegalEntityAsync(ShareholderCompanyLegalEntity deleteLegalEntity, CancellationToken cancellationToken)
        {

            this.dbContext.Entry(deleteLegalEntity).State = EntityState.Deleted;

            await this.dbContext.SaveChangesAsync(cancellationToken);

            return deleteLegalEntity;
        }

        public async Task<ShareholderCompanyLegalEntity> AddShareholderLegalEntityAsync(ShareholderCompanyLegalEntity shareholderCompanyLegalEntity)
        {
            this.dbContext.Attach(shareholderCompanyLegalEntity);

            await this.dbContext.SaveChangesAsync();
            return shareholderCompanyLegalEntity;
        }

        public async Task<ShareholderIndividualLegalEntity> AddShareholderLegalEntityAsync(ShareholderIndividualLegalEntity shareholderIndividualLegalEntity)
        {
            shareholderIndividualLegalEntity.Name = shareholderIndividualLegalEntity.CompanyName;
            this.dbContext.Add(shareholderIndividualLegalEntity);

            await this.dbContext.SaveChangesAsync();
            return shareholderIndividualLegalEntity;
        }

        public async Task<List<ShareholderCompanyLegalEntity>> GetShareholderCompanyLegalEntity(long id)
        {
            return await this.dbContext.ShareholderCompanyLegalEntities.Include(s => (s.Country)).Where(x => x.Shareholder.Id == id).ToListAsync();
        }

        public async Task<List<ShareholderIndividualLegalEntity>> GetShareholderIndividualLegalEntity(long id)
        {
            return await this.dbContext.ShareholderIndividualLegalEntities.Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(x => x.Nationality)
                                .Include(x => x.CountryOfResidence)
                                .Include(x => x.Title)
                                .Where(x => x.Shareholder.Id == id).ToListAsync();
        }

        public async Task<ShareholderIndividualLegalEntity> DeleteShareholderIndividualLegalEntityAsync(ShareholderIndividualLegalEntity shareholderIndividualLegalEntity, CancellationToken cancellationToken)
        {
            this.dbContext.Entry(shareholderIndividualLegalEntity).State = EntityState.Deleted;

            await this.dbContext.SaveChangesAsync(cancellationToken);

            return shareholderIndividualLegalEntity;
        }

        public async Task<QuestionManagement> GetAdminTemplateManagement(QuestionManagement questionManagement)
        {
            return await dbContext.QuestionManagements
                 .Where(a => a.QuestionnaireCode == questionManagement.QuestionnaireCode
                 && a.SolutionCode == questionManagement.SolutionCode
                 && a.TrangloEntityCode == questionManagement.TrangloEntityCode)
                 .FirstOrDefaultAsync();
        }

        public async Task<QuestionManagement> SaveAdminTemplateManagement(QuestionManagement questionManagement)

        {
            this.dbContext.QuestionManagements.Add(questionManagement);
            await this.dbContext.SaveChangesAsync();
            return questionManagement;
        }

        public async Task<QuestionManagement> UpdateAdminTemplateManagement(QuestionManagement questionManagement)
        {
            this.dbContext.QuestionManagements.Update(questionManagement);
            await this.dbContext.SaveChangesAsync();
            return questionManagement;


        }

        public async Task<ShareholderCompanyLegalEntity> UpdateShareholderCompanyLegalEntityAsync(ShareholderCompanyLegalEntity a)
        {
            dbContext.ShareholderCompanyLegalEntities.Update(a);
            await dbContext.SaveChangesAsync();
            return a;
        }

        public async Task<ShareholderIndividualLegalEntity> UpdateShareholderIndividualLegalEntityAsync(ShareholderIndividualLegalEntity b)
        {
            dbContext.ShareholderIndividualLegalEntities.Update(b);
            await dbContext.SaveChangesAsync();
            return b;
        }

        public async Task<ShareholderIndividualLegalEntity> GetShareholderIndividualLegalEntityByLegalEntityCodeAsync(long id)
        {
            return await dbContext.ShareholderIndividualLegalEntities
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<ShareholderIndividualLegalEntity>> GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.ShareholderIndividualLegalEntities
                                .AsNoTracking()
                                .Include(x => x.Gender)
                                .Include(x => x.IDType)
                                .Include(s => ((ShareholderIndividualLegalEntity)s).Nationality)
                                .Include(s => ((ShareholderIndividualLegalEntity)s).CountryOfResidence)
                                .Include(s => s.Shareholder)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task<IReadOnlyList<ShareholderCompanyLegalEntity>> GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            var _MappingQuery = from mapping in dbContext.ShareholderCompanyLegalEntities
                                .AsNoTracking()
                                .Include(s => ((ShareholderCompanyLegalEntity)s).Country)
                                .Include(s => s.Shareholder)
                                where mapping.BusinessProfile == businessProfile
                                select mapping;

            return (await _MappingQuery.ToListAsync()).AsReadOnly();
        }

        public async Task UpdateKYCCustomerSummaryFeedbackNotificationsAsReadAsync(Specification<KYCCustomerSummaryFeedbackNotification> specification, CancellationToken cancellationToken)
        {
            var unreadNotifications = await dbContext.KYCCustomerSummaryFeedbackNotifications
                .Include(x => x.BusinessProfile)
                .Include(x => x.KYCCustomerSummaryFeedback)
                .Where(specification.ToExpression())
                .ToListAsync(cancellationToken);

            if (unreadNotifications.Count == 0)
                return;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result<KYCCustomerSummaryFeedbackNotification>> InsertKYCCustomerSummaryFeedbackNotificationAsync(KYCCustomerSummaryFeedbackNotification notification, CancellationToken cancellationToken)
        {
            dbContext.KYCCustomerSummaryFeedbackNotifications.Add(notification);

            await dbContext.SaveChangesAsync(cancellationToken);

            return notification;
        }

        public async Task UpdateKYCSummaryFeedbackNotificationsAsReadByCategoryAsync(Specification<KYCSummaryFeedbackNotification> specification, CancellationToken cancellationToken)
        {
            var unreadNotifications = await dbContext.KYCSummaryFeedbackNotifications
                .Include(x => x.BusinessProfile)
                .Include(x => x.KYCSummaryFeedback)
                .Where(specification.ToExpression())
                .ToListAsync(cancellationToken);

            if (unreadNotifications.Count == 0)
                return;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result<KYCSummaryFeedbackNotification>> InsertKYCSummaryFeedbackNotificationAsync(KYCSummaryFeedbackNotification notification, CancellationToken cancellationToken)
        {
            dbContext.KYCSummaryFeedbackNotifications.Add(notification);
            await dbContext.SaveChangesAsync(cancellationToken);

            return notification;
        }

        public async Task<List<KYCSummaryFeedback>> GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(int businessProfileCode)
        {
            var query = this.dbContext.KYCSummaryFeedback
                .Where(x => x.BusinessProfile.Id == businessProfileCode)
                .Include(x => x.KYCCategory)
                .ToListAsync();

            return await query;
        }

        public async Task<Title> GetTitleTypeByCode(long? titleTypeCode)
        {
            return await dbContext.Titles
                .Where(c => c.Id == titleTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                var x = await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}