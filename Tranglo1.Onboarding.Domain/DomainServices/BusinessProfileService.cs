using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Entities.Specifications.Category;
using Tranglo1.Onboarding.Domain.Entities.Specifications.CustomerUserBusinessProfiles;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Domain.DomainServices
{
    public class BusinessProfileService
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ISignUpCodeRepository _signUpCodeRepository;
        private readonly IExternalUserRoleRepository _externalUserRoleRepository;
        private readonly IConfiguration _config;

        protected IBusinessProfileRepository Repository => _businessProfileRepository;
        protected IApplicationUserRepository ApplicationUserRepository => _applicationUserRepository;
        protected ISignUpCodeRepository SignUpCodeRepository => _signUpCodeRepository;
        protected IExternalUserRoleRepository ExternalUserRoleRepository => _externalUserRoleRepository;

        public BusinessProfileService(
            IPartnerRepository partnerRepository,
            IBusinessProfileRepository businessProfileRepository,
            IApplicationUserRepository applicationUserRepository,
            ISignUpCodeRepository signUpCodeRepository,
            IExternalUserRoleRepository externalUserRoleRepository,
            IConfiguration config,
            ILogger<BusinessProfileService> logger,
              RBAService rbaService
            )
        {
            this._partnerRepository = partnerRepository;
            this._businessProfileRepository = businessProfileRepository;
            this._applicationUserRepository = applicationUserRepository;
            this._signUpCodeRepository = signUpCodeRepository;
            this._externalUserRoleRepository = externalUserRoleRepository;
            this._config = config;

        }

        public async Task<InternalDocumentUpload> GetInternalDocumentUploadByDocumentIdAsync(InternalDocumentUpload checkFileUpload)
        {
            return await _businessProfileRepository.GetInternalDocumentUploadByDocumentIdAsync(checkFileUpload);
        }

        public async Task<Result<IReadOnlyList<BusinessProfile>>> GetBusinessProfileByEmailAsync(CustomerUser customer)
        {
            IReadOnlyList<BusinessProfile> _BusinessProfiles = await Repository.FindBusinessProfileByEmailAsync(customer.Email);

            return Result.Success(_BusinessProfiles);
        }

        public async Task<InternalDocumentUpload> RemoveInternalDocumentUploadAsync(InternalDocumentUpload documentUploadResult)
        {
            return await _businessProfileRepository.RemoveInternalDocumentUploadAsync(documentUploadResult);
        }

        public async Task<DocumentCategoryBP> GetDocumentCategoryBPAsync(long documentCategoryCode, int businessProfileCode)
        {
            return await Repository.GetDocumentCategoryBPAsync(documentCategoryCode, businessProfileCode);
        }

        public async Task<IReadOnlyList<DocumentCategory>> GetDocumentCategoryByGroupIdAsync(long documentCategoryCode)
        {
            return await Repository.GetDocumentCategoryByGroupId(documentCategoryCode);
        }

        public async Task<ShareholderIndividualLegalEntity> GetShareholderIndividualLegalEntityByLegalEntityCodeAsync(long id)
        {
            return await Repository.GetShareholderIndividualLegalEntityByLegalEntityCodeAsync(id);
        }

        public async Task<DocumentReleaseBP> GetDocumentReleasedByIdAsync(int businessProfileCode, Guid documentId)
        {
            return await Repository.GetDocumentReleasedByIdAsync(businessProfileCode, documentId);
        }

        public async Task<DocumentUploadBP> GetDocumentUploadDocumentId(long id)
        {
            return await Repository.GetDocumentUploadDocumentId(id);
        }

        public async Task<DocumentReleaseBP> UpdateDocumentReleasedBP(DocumentReleaseBP documentReleasedInfo)
        {
            return await Repository.UpdateDocumentReleasedBP(documentReleasedInfo);
        }

        public async Task<DocumentReleaseBP> AddDocumentReleasedUploadAsync(DocumentReleaseBP documentReleasedUpload)
        {
            return await Repository.AddDocumentReleasedUploadAsync(documentReleasedUpload);
        }

        public async Task<DocumentUploadBP> GetDocumentUploadByIdAsync(long id, Guid documentId)
        {
            return await Repository.GetDocumentUploadByIdAsync(id, documentId);
        }

        public async Task<InternalDocumentUpload> AddInternalDocumentUploadAsync(InternalDocumentUpload internalDocumentUpload)
        {
            return await Repository.AddInternalDocumentUploadAsync(internalDocumentUpload);
        }

        public async Task<DocumentCategoryBP> UpdateDocumentCategoryBPInfo(DocumentCategoryBP documentCategoryInfo)
        {
            return await Repository.UpdateDocumentCategoryBPInfo(documentCategoryInfo);
        }

        public async Task<DocumentUploadBP> UpdateDocumentUploadBP(DocumentUploadBP documentInfo)
        {
            return await Repository.UpdateDocumentUploadBP(documentInfo);
        }

        //PartnerBusinessProfileInitial
        public async Task<Result<BusinessProfile>> EnsurePartnerBusinessProfileAsync(
            string companyRegisteredName, string tradeName, BusinessNature businessNature, ContactNumber contactNumber,
            CountryMeta companyRegisteredCountry, Email email, string iMID, string companyAddress, string zipCodePostCode, string trangloEntity,
            string trangloStaffLoginId, string partnerName, string contactPersonName, string forOthers, long? RspStagingId, long? supplierPartnerId,
            long? customerTypeCode, string formerRegisteredCompanyName, string aliasName, CountryMeta nationality,
            long? relationshipTieUpCode, DateTime? dateOfBirth, BusinessProfileIDType businessProfileIdType, string idNumber,
            DateTime? idExpiryDate, ServiceType serviceType, CollectionTier collectionTier, List<long?> solutions, bool isTncTick, Title titleCode, string titleOthers)
        {
            BusinessProfile businessProfile = new BusinessProfile();

            // Entity passed in here is used to set default Service Type
            businessProfile.AddBusinessProfileInitial(partnerName, companyRegisteredCountry, contactPersonName, trangloEntity, titleCode, titleOthers);

            var partnerBP = await Repository.AddBusinessProfilesAsync(businessProfile);
            businessProfile.UpdateBusinessProfilePartner(tradeName, businessNature, companyRegisteredName, contactNumber, companyRegisteredCountry,
                           email, iMID, companyAddress, zipCodePostCode, trangloEntity, trangloStaffLoginId, contactPersonName
                           , forOthers, RspStagingId, supplierPartnerId, customerTypeCode, formerRegisteredCompanyName, aliasName, nationality,
                           relationshipTieUpCode, dateOfBirth, businessProfileIdType, idNumber, idExpiryDate, serviceType, collectionTier, isTncTick, titleCode, titleOthers);


            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(customerTypeCode.Value);
            if (customerType is null)
            {
                return Result.Failure<BusinessProfile>($"Customer Type is null");
            }

            List<KYCSubModuleReview> kYCSubModules = new List<KYCSubModuleReview>();

            foreach (var solutionsTypes in solutions)
            {
                if (solutionsTypes == Solution.Business.Id)
                {
                    // Different Customer Types have different KYC Categories
                    var kycBusinesCategories = await _businessProfileRepository.GetKYCCategoriesByCustomerTypeGroupCodeAsync(customerType.CustomerTypeGroupCode);

                    foreach (var kYCCategory in kycBusinesCategories)
                    {
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }


                    if (trangloEntity == TrangloEntity.TSB.TrangloEntityCode && customerType.CustomerTypeGroupCode != CustomerType.Individual.CustomerTypeGroupCode)
                    {
                        var kycCategories = await _businessProfileRepository.GetKYCBusinessCategories();
                        var verificationCategory = kycCategories.Find(x => x.Id == KYCCategory.Business_Verification.Id);
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, verificationCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }


                }
                if (solutionsTypes == Solution.Connect.Id)
                {
                    var kycConnectCategories = await Repository.GetKYCConnectCategories();

                    foreach (var kYCCategory in kycConnectCategories)
                    {
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }
                }
            }

            await Repository.AddKYCSubmoduleReviews(kYCSubModules);


            // Add Business Declaration records
            foreach (var solutionTypes in solutions)
            {
                if (solutionTypes == Solution.Business.Id)
                {
                    if (customerType != null)
                    {
                        var customerBusinessDeclaration = new CustomerBusinessDeclaration(partnerBP.Id);
                        customerBusinessDeclaration = await _businessProfileRepository.AddCustomerBusinessDeclarationAsync(customerBusinessDeclaration);

                        var customerBusinessDeclarationAnswers = new List<CustomerBusinessDeclarationAnswer>();
                        var declarationQuestions = await _businessProfileRepository.GetDeclarationQuestionsByCustomerTypeAsync(customerType.Id);

                        foreach (var d in declarationQuestions)
                        {
                            var customerBusinessDeclarationAnswer = new CustomerBusinessDeclarationAnswer(customerBusinessDeclaration.Id, d.Id);
                            customerBusinessDeclarationAnswers.Add(customerBusinessDeclarationAnswer);
                        }

                        customerBusinessDeclarationAnswers.Reverse();
                        await _businessProfileRepository.AddCustomerBusinessDeclarationAnswersAsync(customerBusinessDeclarationAnswers);
                    }
                }

            }


            return Result.Success(businessProfile);
        }

        public async Task<Result<BusinessProfile>> EnsurePartnerType(BusinessProfile businessProfile, bool isAllSupplyPartner)
        {
            businessProfile.SetKYCSubmissionStatusBasedOnPartnerType(isAllSupplyPartner);

            var updatePartnerBP = await Repository.UpdateBusinessProfileAsync(businessProfile);
            return Result.Success(businessProfile);
        }

        //End of PartnerBusinessProfile
        public async Task<Result<BusinessProfile>> EnsureBusinessProfileAsync(
            CustomerUser customer, string loginId, params ExternalUserRole[] initialRoles)
        {
            var _CurrentProfiles = await Repository.FindBusinessProfileByEmailAsync(customer.Email);
            var _CurrentProfile = _CurrentProfiles.FirstOrDefault();

            if (_CurrentProfile != null)
            {
                return _CurrentProfile;
            }
            var _customerUserRegistration = await ApplicationUserRepository.GetCustomerUserRegistrationsByLoginIdAsync(loginId);
            var _signUpCode = await SignUpCodeRepository.GetSignUpCodesAsync(_customerUserRegistration.SignUpCode);
            bool hasSignUpCode = _customerUserRegistration.SignUpCode != null && _signUpCode != null;
            bool hasPartnerCode = _signUpCode?.PartnerCode != 0;

            /*
             * 3 different flows during sign up:
             * 1. Self sign up by keying in the company name on Signup screen-> Create a new partner/business profile and stamp company name & country
             * 2. Sign up by keying in sign up code -> Straight proceed with partner's business profile
             * 2a. Sign Up Code is a code with a registered partner/business profile -> (Partner Registration & Business profile are created when perform registration)
             * 2b. Sign Up Code is a code without a registered partner/business profile -> (Partner Registration & Business profile are created when generate signup code)
            */

            BusinessProfile businessProfile = new BusinessProfile();

            var initialRoleList = new List<ExternalUserRole>();

            if (hasSignUpCode)
            {
                var _partner = await _partnerRepository.GetPartnerDetailsByCodeAsync(hasPartnerCode ? _signUpCode.PartnerCode : 0);
                businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeTrackAsync(_partner != null ? _partner.BusinessProfileCode : 0);
                var solutions = await _partnerRepository.GetSolutionsByPartnerAsync(_partner.Id);

                _partner.TermsAcceptanceDate = DateTime.UtcNow.Date;
                #region Assign Leads Origin
                _partner.PartnerRegistrationLeadsOrigin = _customerUserRegistration.PartnerRegistrationLeadsOrigin;
                _partner.OtherPartnerRegistrationLeadsOrigin = _customerUserRegistration.OtherPartnerRegistrationLeadsOrigin;
                #endregion

                await _partnerRepository.UpdatePartnerRegistrationAsync(_partner);

                foreach (var s in solutions)
                {
                    var solutionCode = Convert.ToInt32(s.Id);
                    var initialRole = await _externalUserRoleRepository.GetInitialRoleAsync(solutionCode);

                    initialRoleList.Add(initialRole);
                }
            }
            // 1. Self sign up by keying in the company name
            if (!hasSignUpCode)
            {
                string companyName = _customerUserRegistration.CompanyName;
                int? solutionCode = _customerUserRegistration.SolutionCode;
                string incorporationCountry = customer.CountryMeta.CountryISO2;

                string entity = null;
                if (solutionCode.HasValue && solutionCode.Value == Solution.Business.Id)
                {
                    if (incorporationCountry != null && incorporationCountry == CountryMeta.Malaysia.CountryISO2)
                    {
                        entity = TrangloEntity.TSB.TrangloEntityCode;
                    }
                    else
                    {
                        entity = TrangloEntity.TPL.TrangloEntityCode;
                    }
                }

                // Entity passed in here is used to set default Service Type
                businessProfile.AddBusinessProfileInitial(companyName, customer.CountryMeta, customer.FullName.Value, entity, null, null, solutionCode);
                await Repository.AddBusinessProfilesAsync(businessProfile);

                var solution = solutionCode.HasValue ? Enumeration.FindById<Solution>(solutionCode.Value) : null;

                businessProfile.EnsurePartnerIsInitialized(customer.Email, true, entity, customer.CountryMeta, _customerUserRegistration.CustomerTypeCode, null, null, null, null, solution,
                    leadsOrigin: _customerUserRegistration.PartnerRegistrationLeadsOrigin,
                    otherLeadsOrigin: _customerUserRegistration.OtherPartnerRegistrationLeadsOrigin);



                List<KYCSubModuleReview> kYCSubModules = new List<KYCSubModuleReview>();

                if (solution.Id == Solution.Connect.Id)
                {
                    var kycConnectCategories = await Repository.GetKYCConnectCategories();

                    foreach (var kYCCategory in kycConnectCategories)
                    {
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }

                    //Define Collection Tier
                    CollectionTier collectionTier = new CollectionTier();
                    collectionTier = CollectionTier.Tier_3;

                    //update collection Tier
                    businessProfile.CollectionTier = collectionTier;
                    await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);
                }

                if (solution.Id == Solution.Business.Id)
                {

                    //Changes to KYC SubModuleReview
                    var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(_customerUserRegistration.CustomerTypeCode.Value);
                    if (customerType is null)
                    {
                        return Result.Failure<BusinessProfile>($"Customer Type is null");
                    }
                    // Different Customer Types have different KYC Categories
                    var kycBusinesCategories = await _businessProfileRepository.GetKYCCategoriesByCustomerTypeGroupCodeAsync(customerType.CustomerTypeGroupCode);

                    foreach (var kYCCategory in kycBusinesCategories)
                    {
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }

                    if (entity == TrangloEntity.TSB.TrangloEntityCode && customerType.CustomerTypeGroupCode != CustomerType.Individual.CustomerTypeGroupCode)
                    {
                        var kycCategories = await _businessProfileRepository.GetKYCBusinessCategories();
                        var verificationCategory = kycCategories.Find(x => x.Id == KYCCategory.Business_Verification.Id);
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, verificationCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }

                    //Define Collection Tier
                    CollectionTier collectionTier = new CollectionTier();
                    if (customerType == CustomerType.Individual || customerType == CustomerType.Corporate_Normal_Corporate)
                    {
                        collectionTier = CollectionTier.Tier_1;
                    }
                    else if (customerType == CustomerType.Corporate_Cryptocurrency_Exchange || customerType == CustomerType.Remittance_Partner)
                    {
                        collectionTier = CollectionTier.Tier_3;
                    }

                    //update collection Tier
                    businessProfile.CollectionTier = collectionTier;
                    await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                    // Add Business Declaration records
                    if (customerType != null)
                    {
                        var customerBusinessDeclaration = new CustomerBusinessDeclaration(businessProfile.Id);
                        customerBusinessDeclaration = await _businessProfileRepository.AddCustomerBusinessDeclarationAsync(customerBusinessDeclaration);

                        var customerBusinessDeclarationAnswers = new List<CustomerBusinessDeclarationAnswer>();
                        var declarationQuestions = await _businessProfileRepository.GetDeclarationQuestionsByCustomerTypeAsync(customerType.Id);

                        foreach (var d in declarationQuestions)
                        {
                            var customerBusinessDeclarationAnswer = new CustomerBusinessDeclarationAnswer(customerBusinessDeclaration.Id, d.Id);
                            customerBusinessDeclarationAnswers.Add(customerBusinessDeclarationAnswer);
                        }

                        customerBusinessDeclarationAnswers.Reverse();
                        await _businessProfileRepository.AddCustomerBusinessDeclarationAnswersAsync(customerBusinessDeclarationAnswers);
                    }

                }

                await Repository.AddKYCSubmoduleReviews(kYCSubModules);

                var initialRole = await _externalUserRoleRepository.GetInitialRoleAsync(_customerUserRegistration.SolutionCode ?? 0);

                initialRoleList.Add(initialRole);
            }
            // 2.Sign up by keying in sign up code
            //else if (hasSignUpCode && hasPartnerCode) 
            //{
            //}

            CustomerUserBusinessProfile businessMapping =
                new CustomerUserBusinessProfile(customer, businessProfile);

            var customerUserBusinessProfile = await _businessProfileRepository.AddCustomerUserBusinessProfileMappingAsync(businessMapping);

            foreach (var i in initialRoleList)
            {
                CustomerUserBusinessProfileRole customerUserBusinessProfileRole =
                        new CustomerUserBusinessProfileRole(customerUserBusinessProfile, i);

                await Repository.AddCustomerUserBusinessProfileRole(customerUserBusinessProfileRole);
            }

            return businessProfile;
        }

        public async Task DeleteShareholderCompanyLegalEntityAsync(BusinessProfile businessProfile, ShareholderCompanyLegalEntity deleteLegalEntity, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteShareholderLegalEntityAsync(deleteLegalEntity, cancellationToken);
        }

        public async Task<ShareholderCompanyLegalEntity> GetShareholderCompanyLegalEntityByLegalEntityCodeAsync(long id)
        {
            return await Repository.GetShareholderCompanyLegalEntityByLegalEntityCodeAsync(id);
        }

        public async Task<ShareholderCompanyLegalEntity> AddShareholderCompanyLegalEntityAsync(BusinessProfile businessProfile, ShareholderCompanyLegalEntity shareholderCompanyLegalEntity)
        {
            var result = await Repository.AddShareholderLegalEntityAsync(shareholderCompanyLegalEntity);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;

        }

        public async Task<DocumentUploadBP> GetDocumentUploadByIdAsync(long id)
        {
            return await Repository.GetDocumentUploadByIdAsync(id);
        }

        public async Task<DocumentCategoryBP> UpdateDocumentCategoryBP(DocumentCategoryBP checkCategoryBP)
        {
            return await Repository.UpdateDocumentCategoryBP(checkCategoryBP);
        }



        public async Task<Result<IReadOnlyList<DocumentUploadBP>>> GeDocumentIdByCategoryBPCodeAsync(long id)
        {

            var DocumentUploadBPspec = Specification<DocumentUploadBP>.All;
            var byCategoryBPCode = new DocumentIdByCategoryBPCode(id);
            DocumentUploadBPspec = DocumentUploadBPspec.And(byCategoryBPCode);

            IReadOnlyList<DocumentUploadBP> DocumentUploadBPList = await Repository.GetDocumentUploadBPProfile(DocumentUploadBPspec);


            return Result.Success(DocumentUploadBPList);
        }

        public async Task<ShareholderIndividualLegalEntity> AddShareholderIndividualLegalEntityAsync(BusinessProfile businessProfile, ShareholderIndividualLegalEntity shareholderIndividualLegalEntity)
        {
            var result = await Repository.AddShareholderLegalEntityAsync(shareholderIndividualLegalEntity);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<List<EmailRecipient>> GetRecipientEmail(long bccType, long notificationTemplate)
        {
            return await Repository.GetRecipientEmail(bccType, notificationTemplate);
        }

        public async Task<List<EmailRecipient>> GetRecipientEmailByCollectionTier(long collectionTierCode, long recipientType, long notificationTemplate)
        {
            return await Repository.GetRecipientEmailByCollectionTier(collectionTierCode, recipientType, notificationTemplate);
        }

        public async Task<Result<IReadOnlyList<DocumentCategoryBP>>> GetCategoryBPInfoByCategoryCodeAsync(long documentCategoryCode)
        {

            var DocumentCategoryBPspec = Specification<DocumentCategoryBP>.All;
            var byCategoryCode = new CategoryBpInfoByCategoryCode(documentCategoryCode);
            DocumentCategoryBPspec = DocumentCategoryBPspec.And(byCategoryCode);

            IReadOnlyList<DocumentCategoryBP> DocumentCategoryBPList = await Repository.GetCategoryBPProfile(DocumentCategoryBPspec);

            return Result.Success(DocumentCategoryBPList);
        }

        public async Task<List<ShareholderCompanyLegalEntity>> GetShareholderCompanyLegalEntity(long id)
        {
            return await Repository.GetShareholderCompanyLegalEntity(id);
        }

        public async Task<List<ShareholderIndividualLegalEntity>> GetShareholderIndividualLegalEntity(long id)
        {
            return await Repository.GetShareholderIndividualLegalEntity(id);
        }

        public async Task DeleteShareholderIndividualLegalEntityAsync(BusinessProfile businessProfile, ShareholderIndividualLegalEntity shareholderIndividualLegalEntity, CancellationToken cancellationToken)
        {

            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteShareholderIndividualLegalEntityAsync(shareholderIndividualLegalEntity, cancellationToken);
        }

        public async Task<DocumentUploadBP> DeleteDocumentUploadBP(BusinessProfile businessProfile, DocumentUploadBP documentUploadBP)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Documentation);
            return await Repository.DeleteDocumentUploadBP(documentUploadBP);
        }


        public async Task<DocumentUploadBP> AddDocumentUploadAsync(BusinessProfile businessProfile, DocumentUploadBP document)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Documentation);
            return await Repository.AddDocumentUploadAsync(document);
        }



        public Task GetCategoryCategoryBPStatusCodeAsync(int documentCategoryCode)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentCategoryBP> GetDocumentCategoryAsync(int businessProfileCode, int documentCategoryCode)
        {
            return await Repository.GetDocumentCategoryAsync(businessProfileCode, documentCategoryCode);
        }


        public async Task<Result<IReadOnlyList<DocumentCategoryBP>>> GetCategoryBPyCategoryCodeAsync(int businessProfileCode)
        {
            IReadOnlyList<DocumentCategoryBP> DocumentCategoryBPList = await Repository.GetDocumentCategoryBPAsync(businessProfileCode);
            return Result.Success(DocumentCategoryBPList);
        }


        public async Task<Result<IReadOnlyList<DocumentCategoryBP>>> GetCategoryBPyCategoryCodeAsync(Specification<DocumentCategoryBP> filter)
        {
            IReadOnlyList<DocumentCategoryBP> DocumentCategoryBPList = await Repository.GetDocumentCategoryBPAsync(filter);
            return Result.Success(DocumentCategoryBPList);
        }

        public async Task<Result<IReadOnlyList<DocumentCategory>>> GetCategoryInfoByCategoryCodeAsync(long documentCategoryCode)
        {
            var DocumentCategorypec = Specification<DocumentCategory>.All;
            var byCategoryCode = new CategoryInfoByCategoryCode(documentCategoryCode);
            DocumentCategorypec = DocumentCategorypec.And(byCategoryCode);

            IReadOnlyList<DocumentCategory> DocumentCategoryList = await Repository.GetDocumentCategoryAsync(DocumentCategorypec);

            return Result.Success(DocumentCategoryList);
        }

        public async Task<Result<DocumentCategoryBP>> AddCategoryBPAsync(DocumentCategoryBP categoryBP)
        {
            return await Repository.AddCategoryBPAsync(categoryBP);
        }

        public async Task<Result<DocumentCommentBP>> AddCommentsAsync(DocumentCommentBP comments)
        {
            return await Repository.AddCommentsAsync(comments);
        }


        public async Task<Result<IReadOnlyList<BusinessProfile>>> GetBusinessProfileByUserId(CustomerUser customer)
        {
            IReadOnlyList<BusinessProfile> _BusinessProfiles = await Repository.FindBusinessProfileByEmailAsync(customer.Email);

            if (_BusinessProfiles != null && _BusinessProfiles.Any())
            {
                return Result.Success(_BusinessProfiles);
            }

            return Result.Failure<IReadOnlyList<BusinessProfile>>("User list is empty.");
        }

        public async Task<ShareholderCompanyLegalEntity> UpdateShareholderCompanyLegalEntityAsync(ShareholderCompanyLegalEntity a)
        {
            var result = await Repository.UpdateShareholderCompanyLegalEntityAsync(a);

            return result;
        }

        public async Task<Result<BusinessProfile>> GetBusinessProfileByCompanyName(string companyName)
        {
            BusinessProfile _BusinessProfiles = await Repository.GetBusinessProfileByCompanyNameAsync(companyName);

            if (_BusinessProfiles != null)
            {
                return Result.Success(_BusinessProfiles);
            }

            return Result.Failure<BusinessProfile>("User list is empty.");
        }

        public async Task<Result<BusinessProfile>> GetBusinessProfileByBusinessProfileCodeAsync(int businessProfileCode, bool isNoTrackKYCSubmissionStatus = false)
        {
            Specification<BusinessProfile> businessProfileSpec = Specification<BusinessProfile>.All;
            BusinessProfileByBusinessProfileCode byBusinessProfileCode = new BusinessProfileByBusinessProfileCode(businessProfileCode);
            businessProfileSpec = businessProfileSpec.And(byBusinessProfileCode);

            IReadOnlyList<BusinessProfile> businessProfileList = await Repository.GetBusinessProfilesAsync(businessProfileSpec, isNoTrackKYCSubmissionStatus);
            BusinessProfile businessProfile = businessProfileList.FirstOrDefault();

            return businessProfile;
        }

        public async Task<ShareholderIndividualLegalEntity> UpdateShareholderIndividualLegalEntityAsync(ShareholderIndividualLegalEntity b)
        {
            var result = await Repository.UpdateShareholderIndividualLegalEntityAsync(b);

            return result;
        }

        public struct UniqueCompanyNameValidation
        {
            public bool isInBusinessProfileList;
            public bool isInCustomerUserRegistrationList;
            public bool isInSignUpCodeList;
            public bool isInUsed => isInBusinessProfileList || isInCustomerUserRegistrationList || isInSignUpCodeList;
        };

        public async Task<UniqueCompanyNameValidation> CheckIsExistingCompanyNameAsync(string companyName)
        {
            string trimmedCompanyName = companyName.Trim();
            BusinessProfile businessProfileList = await Repository.GetBusinessProfileByCompanyNameAsync(trimmedCompanyName);
            CustomerUserRegistration customerUserRegistrationList = await ApplicationUserRepository.GetCustomerUserRegistrationsByCompanyNameAsync(trimmedCompanyName);
            SignUpCode signUpCodeList = await _signUpCodeRepository.GetActiveSignUpCodeByCompanyNameAsync(trimmedCompanyName);

            var res = new UniqueCompanyNameValidation();
            res.isInBusinessProfileList = (businessProfileList != null);
            res.isInCustomerUserRegistrationList = (customerUserRegistrationList != null);
            res.isInSignUpCodeList = (signUpCodeList != null);

            return res;
        }

        public async Task<bool> IsCompanyNameDuplicateDuringUpdate(string companyName, long businessProfileCode)
        {
            string trimmedCompanyName = companyName.Trim();
            BusinessProfile businessProfile = await Repository.GetBusinessProfileByCompanyNameAsync(trimmedCompanyName);

            if(businessProfile != null && businessProfile.Id != businessProfileCode)
            {
                return true;
            }
            return false;
        }

        public async Task<Result<DocumentUploadBP>> UploadDocumentsAsync(DocumentUploadBP documentUpload)
        {
            return await Repository.UploadDocumentsAsync(documentUpload);
        }


        public async Task<Result<IReadOnlyList<BusinessProfile>>> GetBusinessProfilesByBusinessProfileCodeAsync(int businessProfileCode)
        {
            var businessProfileSpec = Specification<BusinessProfile>.All;
            var byBusinessProfileCode = new BusinessProfileByBusinessProfileCode(businessProfileCode);
            businessProfileSpec = businessProfileSpec.And(byBusinessProfileCode);

            IReadOnlyList<BusinessProfile> businessProfileList = await Repository.GetBusinessProfilesAsync(businessProfileSpec);

            return Result.Success(businessProfileList);
        }

        public async Task<bool> UserHasTrangloEntity(TrangloStaff trangloStaff, int profileCode)
        {
            var trangloStaffEntity = await this._applicationUserRepository.GetTrangloStaffEntityAssignmentById(trangloStaff.LoginId);
            var getPartner = await this._partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(profileCode);
            var trangloEntityByPartner = await this._partnerRepository.GetTrangloEntitiesByPartnerAsync(getPartner.Id);

            if (trangloEntityByPartner != null)
            {
                //var check = trangloStaffEntity.Where(x => x.TrangloEntity == trangloEntityByPartner);
                foreach (var item in trangloStaffEntity)
                {
                    //if (item.TrangloEntity.Trim().ToLower() == trangloEntityByPartner.Trim().ToLower())
                    //{
                    //    return true;
                    //}

                    if (trangloEntityByPartner.Exists(x => x == item.TrangloEntity || x == null))
                    {
                        return true;
                    }
                }
            }
            else if (trangloEntityByPartner.Count() == 0)
            {
                return true;
            }
            return false;
        }

        public async Task<Result<COInformation>> UpdateCOInformationsAsync(BusinessProfile businessProfile, COInformation cOInfo)
        {
            var result = await Repository.UpdateCOInformationsAsync(cOInfo);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_ComplianceInfo);

            return result;
        }

        public async Task<Result<BusinessProfile>> CreateCustomerUserBusinessProfileAndRoleAsync(
            CustomerUser customer, int businessProfileCode, List<string> userRoleCode, bool isNewApplication)
        {
            var businessProfileSpec = Specification<BusinessProfile>.All;
            var byBusinessProfileCode = new BusinessProfileByBusinessProfileCode(businessProfileCode);
            businessProfileSpec = businessProfileSpec.And(byBusinessProfileCode);

            var businessProfileList = await Repository.GetBusinessProfilesAsync(businessProfileSpec);
            var businessProfile = businessProfileList.FirstOrDefault();
            var customerUserBusinessProfile = await Repository.GetCustomerUserBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            if (customerUserBusinessProfile != null && isNewApplication == true && customer.Id == customerUserBusinessProfile.UserId)
            {
                foreach (var r in userRoleCode)
                {
                    //UserRole userRole = Enumeration.FindById<UserRole>(r);
                    ExternalUserRole userRole = await ExternalUserRoleRepository.GetExternalRoleByRoleCodeAsync(r);

                    if (userRole != null)
                    {
                        CustomerUserBusinessProfileRole customerUserBusinessProfileRole = new CustomerUserBusinessProfileRole(customerUserBusinessProfile, userRole);
                        await Repository.AddCustomerUserBusinessProfileRole(customerUserBusinessProfileRole);
                    }
                }
            }
            if (customerUserBusinessProfile == null || customer.Id != customerUserBusinessProfile.UserId)
            {
                CustomerUserBusinessProfile businessMapping =
                    new CustomerUserBusinessProfile(customer, businessProfile);
                businessMapping.SetCompanyUserAccountStatus(CompanyUserAccountStatus.PendingActivation);

                await Repository.AddCustomerUserBusinessProfileMappingAsync(businessMapping);

                foreach (var r in userRoleCode)
                {
                    //UserRole userRole = Enumeration.FindById<UserRole>(r);
                    ExternalUserRole userRole = await ExternalUserRoleRepository.GetExternalRoleByRoleCodeAsync(r);

                    if (userRole != null)
                    {
                        CustomerUserBusinessProfileRole customerUserBusinessProfileRole = new CustomerUserBusinessProfileRole(businessMapping, userRole);
                        await Repository.AddCustomerUserBusinessProfileRole(customerUserBusinessProfileRole);
                    }
                }
            }
            return businessProfile;
        }

        public async Task<Result<COInformation>> AddCOInformationsAsync(BusinessProfile businessProfile, COInformation coInfo)
        {
            var result = await Repository.AddCOInformationsAsync(coInfo);

            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_ComplianceInfo);

            return result;
        }

        public async Task<Result<LicenseInformation>> UpdateLicenseInformationsAsync(BusinessProfile businessProfile, LicenseInformation licenseInfo)
        {
            var result = await Repository.UpdateLicenseInformationsAsync(licenseInfo);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_LicenseInfo);

            return result;
        }


        //public async Task<Result> AddBusinessProfileAsync(LicenseInformation licenseInformation)
        //{
        //    return await Repository.AddBusinessProfilesAsync(licenseInformation);
        //}



        public async Task<Result<IReadOnlyList<CustomerUserBusinessProfile>>> GetCustomerUserBusinessProfilesAsync(
            CustomerUser customer, int businessProfileCode)
        {
            var customerUserBusinessProfileSpec = Specification<CustomerUserBusinessProfile>.All;
            var byUserID = new CustomerUserBusinessProfileByUserID(customer.Id);
            var byBusinessProfileCode = new CustomerUserBusinessProfileByBusinessProfileCode(businessProfileCode);
            customerUserBusinessProfileSpec = customerUserBusinessProfileSpec.And(byUserID).And(byBusinessProfileCode);

            IReadOnlyList<CustomerUserBusinessProfile> customerUserBusinessProfileList = await Repository.GetCustomerUserBusinessProfilesAsync(customerUserBusinessProfileSpec);

            if (customerUserBusinessProfileList != null && customerUserBusinessProfileList.Any())
            {
                return Result.Success(customerUserBusinessProfileList);
            }

            return Result.Failure<IReadOnlyList<CustomerUserBusinessProfile>>("User list is empty.");
        }

        public async Task<Result<IReadOnlyList<BusinessProfile>>> GetBusinessProfileListAsync()
        {
            var customerUserBusinessProfileSpec = Specification<BusinessProfile>.All;


            IReadOnlyList<BusinessProfile> customerUserBusinessProfileList = await Repository.GetBusinessProfileListAsync();

            if (customerUserBusinessProfileList != null && customerUserBusinessProfileList.Any())
            {
                return Result.Success(customerUserBusinessProfileList);
            }

            return Result.Failure<IReadOnlyList<BusinessProfile>>("User list is empty.");
        }

        public async Task<Result<BusinessProfile>> AddBusinessProfileAsync(BusinessProfile businessProfile)
        {
            var addBusinessProfile = await Repository.AddBusinessProfilesAsync(businessProfile);

            return Result.Success(addBusinessProfile);
        }

        public async Task<BusinessProfile> UpdateBusinessProfileAsync(BusinessProfile businessProfile)
        {
            var result = await Repository.UpdateBusinessProfileAsync(businessProfile);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_BusinessProfile);

            return result;
        }


        public async Task<Result<LicenseInformation>> AddLicenseInformationsAsync(BusinessProfile businessProfile, LicenseInformation licenseInfo)
        {
            var result = await Repository.AddLicenseInformationsAsync(licenseInfo);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_LicenseInfo);

            return result;
        }

        public async Task<BoardOfDirector> AddBoardOfDirectorAsync(BusinessProfile businessProfile, BoardOfDirector boardOfDirector)
        {
            var result = await Repository.AddBoardOfDirectorAsync(boardOfDirector);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<IndividualLegalEntity> AddLegalEntityAsync(BusinessProfile businessProfile, IndividualLegalEntity legalEntity)
        {
            var result = await Repository.AddLegalEntityAsync(legalEntity);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }
        public async Task<CompanyLegalEntity> AddLegalEntityAsync(BusinessProfile businessProfile, CompanyLegalEntity legalEntity)
        {
            var result = await Repository.AddLegalEntityAsync(legalEntity);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<PrimaryOfficer> AddPrimaryOfficerAsync(BusinessProfile businessProfile, PrimaryOfficer primaryOfficer)
        {
            var result = await Repository.AddPrimaryOfficerAsync(primaryOfficer);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<ParentHoldingCompany> AddParentHoldingCompanyAsync(BusinessProfile businessProfile, ParentHoldingCompany parentHoldingCompany)
        {
            var result = await Repository.AddParentHoldingCompanyAsync(parentHoldingCompany);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Shareholder> AddShareholderAsync(BusinessProfile businessProfile, Shareholder shareholder)
        {
            var result = await Repository.AddShareholderAsync(shareholder);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }
        /*
        public async Task<IndividualShareholder> AddShareholderAsync(IndividualShareholder shareholder)
        {
            return await Repository.AddShareholderAsync(shareholder);
        }
        public async Task<CompanyShareholder> AddShareholderAsync(CompanyShareholder shareholder)
        {
            return await Repository.AddShareholderAsync(shareholder);
        }
        */
        public async Task<PoliticallyExposedPerson> AddPoliticallyExposedPersonAsync(BusinessProfile businessProfile, PoliticallyExposedPerson politicallyExposedPerson)
        {
            var result = await Repository.AddPoliticallyExposedPersonAsync(politicallyExposedPerson);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<AffiliateAndSubsidiary>> AddAffiliateAndSubsidiaryAsync(BusinessProfile businessProfile, AffiliateAndSubsidiary affiliateAndSubsidiary)
        {
            var result = await Repository.AddAffiliateAndSubsidiaryAsync(affiliateAndSubsidiary);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<AuthorisedPerson>> AddAuthorisedPersonAsync(BusinessProfile businessProfile, AuthorisedPerson authorisedPerson)
        {
            var result = await Repository.AddAuthorisedPersonAsync(authorisedPerson);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task DeleteAffiliateAndSubsidiaryAsync(BusinessProfile businessProfile, IEnumerable<AffiliateAndSubsidiary> affiliateAndSubsidiary, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteAffiliateAndSubsidiaryAsync(affiliateAndSubsidiary, cancellationToken);
        }

        public async Task DeleteBoardOfDirectorAsync(BusinessProfile businessProfile, IEnumerable<BoardOfDirector> boardOfDirector, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteBoardOfDirectorAsync(boardOfDirector, cancellationToken);
        }


        public async Task DeleteIndividualLegalEntityAsync(BusinessProfile businessProfile, IEnumerable<IndividualLegalEntity> legalEntity, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteIndividualLegalEntityAsync(legalEntity, cancellationToken);
        }

        public async Task DeleteCompanyLegalEntityAsync(BusinessProfile businessProfile, IEnumerable<CompanyLegalEntity> legalEntity, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteCompanyLegalEntityAsync(legalEntity, cancellationToken);
        }
        public async Task DeleteLegalEntityAsync(BusinessProfile businessProfile, IEnumerable<LegalEntity> legalEntity, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteLegalEntityAsync(legalEntity, cancellationToken);
        }
        public async Task DeleteShareholderAsync(BusinessProfile businessProfile, IEnumerable<Shareholder> shareholder, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteShareholderAsync(shareholder, cancellationToken);
        }


        public async Task DeleteIndividualShareholderAsync(BusinessProfile businessProfile, IEnumerable<Shareholder> shareholder, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteShareholderAsync(shareholder, cancellationToken);
        }

        public async Task DeleteParentHoldingCompanyAsync(BusinessProfile businessProfile, IEnumerable<ParentHoldingCompany> parentHoldingCompany, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteParentHoldingCompanyAsync(parentHoldingCompany, cancellationToken);

        }
        public async Task DeletePoliticallyExposedPersonAsync(BusinessProfile businessProfile, IEnumerable<PoliticallyExposedPerson> politicallyExposedPerson, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeletePoliticallyExposedPersonAsync(politicallyExposedPerson, cancellationToken);
        }

        public async Task DeletePrimaryOfficerAsync(IEnumerable<PrimaryOfficer> primaryOfficer, CancellationToken cancellationToken)
        {
            BusinessProfile businessProfile = primaryOfficer?.FirstOrDefault().BusinessProfile;
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeletePrimaryOfficerAsync(primaryOfficer, cancellationToken);
        }

        public async Task DeleteAuthorisedPersonAsync(IEnumerable<AuthorisedPerson> authorisedPerson, CancellationToken cancellationToken)
        {
            BusinessProfile businessProfile = authorisedPerson?.FirstOrDefault().BusinessProfile;
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            await Repository.DeleteAuthorisedPersonAsync(authorisedPerson, cancellationToken);
        }

        public async Task<Result<IReadOnlyList<BoardOfDirector>>> GetBoardOfDirectorByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<BoardOfDirector> _BoardOfDirectors = await Repository.GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_BoardOfDirectors);
        }
        public async Task<Result<IReadOnlyList<LegalEntity>>> GetLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<LegalEntity> _LegalEntities = await Repository.GetLegalEntityByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_LegalEntities);
        }

        public async Task<Result<IReadOnlyList<IndividualLegalEntity>>> GetIndividualLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<IndividualLegalEntity> _LegalEntities = await Repository.GetIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_LegalEntities);
        }
        public async Task<Result<IReadOnlyList<CompanyLegalEntity>>> GetCompanyLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<CompanyLegalEntity> _LegalEntities = await Repository.GetCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_LegalEntities);
        }

        public async Task<Result<IReadOnlyList<PrimaryOfficer>>> GetPrimaryOfficerByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<PrimaryOfficer> _PrimaryOfficers = await Repository.GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_PrimaryOfficers);
        }

        public async Task<Result<IReadOnlyList<ParentHoldingCompany>>> GetParentHoldingCompanyByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<ParentHoldingCompany> _ParentHoldingCompanies = await Repository.GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_ParentHoldingCompanies);
        }
        public async Task<Result<IReadOnlyList<Shareholder>>> GetShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<Shareholder> _Shareholders = await Repository.GetShareholderByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_Shareholders);
        }
        public async Task<Result<IReadOnlyList<IndividualShareholder>>> GetIndividualShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<IndividualShareholder> _Shareholders = await Repository.GetIndividualShareholderByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_Shareholders);
        }
        public async Task<Result<IReadOnlyList<CompanyShareholder>>> GetCompanyShareholderByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<CompanyShareholder> _Shareholders = await Repository.GetCompanyShareholderByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_Shareholders);
        }
        public async Task<Result<IReadOnlyList<ShareholderIndividualLegalEntity>>> GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            // Individual Ultimate Shareholder
            IReadOnlyList<ShareholderIndividualLegalEntity> shareholderIndividualLegalEntities = await Repository.GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile);
            return Result.Success(shareholderIndividualLegalEntities);
        }
        public async Task<Result<IReadOnlyList<ShareholderCompanyLegalEntity>>> GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            // Company Ultimate Shareholder
            IReadOnlyList<ShareholderCompanyLegalEntity> shareholderCompanyLegalEntities = await Repository.GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile);
            return Result.Success(shareholderCompanyLegalEntities);
        }
        public async Task<Result<IReadOnlyList<PoliticallyExposedPerson>>> GetPoliticallyExposedPersonByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<PoliticallyExposedPerson> _PoliticallyExposedPersons = await Repository.GetPoliticallyExposedPersonByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_PoliticallyExposedPersons);
        }

        public async Task<Result<IReadOnlyList<AffiliateAndSubsidiary>>> GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<AffiliateAndSubsidiary> _AffiliateAndSubsidiaries = await Repository.GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_AffiliateAndSubsidiaries);
        }

        //Phase 3 Changes
        public async Task<Result<IReadOnlyList<AuthorisedPerson>>> GetAuthorisedPersonByBusinessProfileCodeAsync(BusinessProfile businessProfile)
        {
            IReadOnlyList<AuthorisedPerson> _AuthorisedPerson = await Repository.GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile);

            return Result.Success(_AuthorisedPerson);
        }


        public async Task<Result<BoardOfDirector>> UpdateBoardOfDirectorAsync(BusinessProfile businessProfile, BoardOfDirector boardOfDirector, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateBoardOfDirectorAsync(boardOfDirector, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<IndividualLegalEntity>> UpdateLegalEntityAsync(BusinessProfile businessProfile, IndividualLegalEntity legalEntity, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateLegalEntityAsync(legalEntity, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }
        public async Task<Result<CompanyLegalEntity>> UpdateLegalEntityAsync(BusinessProfile businessProfile, CompanyLegalEntity legalEntity, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateLegalEntityAsync(legalEntity, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<PrimaryOfficer>> UpdatePrimaryOfficerAsync(BusinessProfile businessProfile, PrimaryOfficer primaryOfficer, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdatePrimaryOfficerAsync(primaryOfficer, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<ParentHoldingCompany>> UpdateParentHoldingCompanyAsync(BusinessProfile businessProfile, ParentHoldingCompany parentHoldingCompany, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateParentHoldingCompanyAsync(parentHoldingCompany, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }
        public async Task<Result<Shareholder>> UpdateShareholderAsync(BusinessProfile businessProfile, Shareholder shareholder, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateShareholderAsync(shareholder, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<PoliticallyExposedPerson>> UpdatePoliticallyExposedPersonAsync(BusinessProfile businessProfile, PoliticallyExposedPerson politicallyExposedPerson, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdatePoliticallyExposedPersonAsync(politicallyExposedPerson, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<AffiliateAndSubsidiary>> UpdateAffiliateAndSubsidiaryAsync(BusinessProfile businessProfile, AffiliateAndSubsidiary affiliateAndSubsidiary, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateAffiliateAndSubsidiaryAsync(affiliateAndSubsidiary, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }

        public async Task<Result<AuthorisedPerson>> UpdateAuthorisedPerson(BusinessProfile businessProfile, AuthorisedPerson authorisedPerson, CancellationToken cancellationToken)
        {
            var result = await Repository.UpdateAuthorisedPerson(authorisedPerson, cancellationToken);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Ownership);
            return result;
        }


        public async Task<Declaration> GetKYCDeclarationInfoAsync(int BusinessProfileCode)
        {
            return await Repository.GetKYCDeclarationInfoAsync(BusinessProfileCode);
        }

        public async Task<BusinessUserDeclaration> GetKYCBusinessDeclarationInfoAsync(int BusinessProfileCode)
        {
            return await Repository.GetKYCBusinessDeclarationInfoAsync(BusinessProfileCode);
        }


        public async Task<Declaration> InsertKYCDeclarationInfoAsync(BusinessProfile businessProfile, Declaration declaration)
        {
            var result = await Repository.InsertKYCDeclarationInfoAsync(declaration);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Declaration);
            return result;
        }

        public async Task<Declaration> UpdateKYCDeclarationInfoAsync(BusinessProfile businessProfile, Declaration declaration)
        {
            var result = await Repository.UpdateKYCDeclarationInfoAsync(declaration);
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_Declaration);
            return result;
        }

        public async Task<bool> CheckHasUploadedAMLDocumentation(int businessProfileCode, Solution solution)
        {
            var businessProfileResult = this.GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfileResult.Result.IsSuccess)
            {
                var businessProfile = businessProfileResult.Result.Value;
                var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var customerType = partner.CustomerType;

                if (solution == Solution.Business && customerType == null)
                {
                    return false;
                }

                var documentCategoryBP = await Repository.GetDocumentCategoryBPAMLCFTAsync(businessProfile, solution, customerType);
                var hasDocumentUploaded = await Repository.GetDocumentUploadBPAsync(documentCategoryBP);

                return (hasDocumentUploaded != null);
            }

            return false;
        }

        public async Task<bool> CheckHasUploadedBusinessAMLDocumentation(int businessProfileCode, Solution solution)
        {
            var businessProfileResult = this.GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfileResult.Result.IsSuccess)
            {
                var businessProfile = businessProfileResult.Result.Value;
                var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var customerType = partner.CustomerType;

                if (solution == Solution.Connect)
                {
                    return false;
                }

                var documentCategoryBP = await Repository.GetDocumentCategoryBPAMLCFTAsync(businessProfile, solution, customerType);
                var hasDocumentUploaded = await Repository.GetDocumentUploadBPAsync(documentCategoryBP);

                return (hasDocumentUploaded != null);
            }

            return false;
        }
        public async Task<bool> CheckHasUploadedAMLDocumentationNoTrackKYCSubmissionStatus(int businessProfileCode)
        {
            var businessProfileResult = this.GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfileResult.Result.IsSuccess)
            {
                var businessProfile = businessProfileResult.Result.Value;

                //TODO: During Verification on KYC to specify by Solution, Customer Type, BP ( Temporary pass in Connect/0 in Sprint 4)
                var documentCategoryBP = await Repository.GetDocumentCategoryBPAMLCFTAsync(businessProfile, Solution.Connect, null);
                var hasDocumentUploaded = await Repository.GetDocumentUploadBPAsync(documentCategoryBP);

                return (hasDocumentUploaded != null);
            }

            return false;
        }
        public async Task<bool> CheckHasAnsweredAMLQuestionnaire(int businessProfileCode)
        {
            var businessProfileResult = this.GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfileResult.Result.IsSuccess)
            {
                var businessProfile = businessProfileResult.Result.Value;

                var aMLCFTQuestionnaireAnswer = await Repository.GetAMLCFTQuestionnaireAnswerAsync(businessProfile);

                return (aMLCFTQuestionnaireAnswer != null);
            }

            return false;
        }

        public async Task<AMLCFTQuestionnaireAnswer> AddAMLCFTQuestionnaireAnswersAsync(AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_AMLOrCFT);
            return await Repository.AddAMLCFTQuestionnaireAnswersAsync(amlCFTQuestionnaireAnswer, cancellationToken);
        }

        public async Task<AMLCFTQuestionnaireAnswer> UpdateAMLCFTQuestionnaireAnswersAsync(AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_AMLOrCFT);
            return await Repository.UpdateAMLCFTQuestionnaireAnswersAsync(amlCFTQuestionnaireAnswer, cancellationToken);
        }

        public async Task DeleteAMLCFTQuestionnaireAnswersAsync(IEnumerable<AMLCFTQuestionnaireAnswer> amlCFTQuestionnaireAnswers, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            await UpdateKYCSubModuleUserUpdatedDate(businessProfile, KYCCategory.Connect_AMLOrCFT);
            await Repository.DeleteAMLCFTQuestionnaireAnswersAsync(amlCFTQuestionnaireAnswers, cancellationToken);
        }

        public async Task<COInformation> GetCOInfoByBusinessCode(int businessProfileCode)
        {
            return await Repository.GetCOInfoByBusinessCode(businessProfileCode);
        }

        public async Task<LicenseInformation> GetLicenseInfoByBusinessCode(int businessProfileCode)
        {
            return await Repository.GetLicenseInfoByBusinessCode(businessProfileCode);
        }

        public async Task<long> GetDocumentCategoryGroupIdBySolution(long? solutionCode)
        {
            return await Repository.GetDocumentCategoryGroupIdBySolution(solutionCode);
        }


        public async Task<bool> SubmitKycAsync(int businessProfileCode, long? solution, CollectionTier collectionTier, string customerSolution, ApplicationUser applicationUser)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfile.IsSuccess && businessProfile.Value != null)
            {
                //set the kyc to submitted
                businessProfile.Value.SubmitKYCForReview(solution, collectionTier, customerSolution, businessProfileCode, applicationUser);

                //update all document as pending review
                var DocumentCategoryBPspec = Specification<DocumentCategoryBP>.All;
                var byCategoryCode = new CategoryInfoByBusinessProfileCode(businessProfileCode);
                DocumentCategoryBPspec = DocumentCategoryBPspec.And(byCategoryCode);

                var CategoryBPList = await GetCategoryBPyCategoryCodeAsync(DocumentCategoryBPspec);
                if (CategoryBPList.IsSuccess && CategoryBPList.Value != null)
                {
                    foreach (var item in CategoryBPList.Value)
                    {

                        item.DocumentCategoryBPStatus = DocumentCategoryBPStatus.PendingReview;
                    }
                }

                return await Repository.SubmitKYCAsync(businessProfile.Value, CategoryBPList.Value.ToList());

            }

            return false;
        }

        public async Task<bool> SubmitBusinessKYCAsync(int businessProfileCode, long? solution, CollectionTier collectionTier, string customerSolution, ApplicationUser applicationUser)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfile.IsSuccess && businessProfile.Value != null)
            {
                //set the kyc to submitted
                businessProfile.Value.SubmitBusinessKYCForReview(solution, collectionTier, customerSolution, businessProfileCode, applicationUser);

                //update all document as pending review
                var DocumentCategoryBPspec = Specification<DocumentCategoryBP>.All;
                var byCategoryCode = new CategoryInfoByBusinessProfileCode(businessProfileCode);
                DocumentCategoryBPspec = DocumentCategoryBPspec.And(byCategoryCode);

                var CategoryBPList = await GetCategoryBPyCategoryCodeAsync(DocumentCategoryBPspec);
                if (CategoryBPList.IsSuccess && CategoryBPList.Value != null)
                {
                    foreach (var item in CategoryBPList.Value)
                    {

                        item.DocumentCategoryBPStatus = DocumentCategoryBPStatus.PendingReview;
                    }
                }

                return await Repository.SubmitBusinessKYCAsync(businessProfile.Value, CategoryBPList.Value.ToList());

            }

            return false;
        }
        #region Validation
        public async Task<KYCSummary> IsMandatoryFieldCompletedAsync(int businessProfileCode, Solution solution, List<Questionnaire> questionnaires = null)
        {
            var _summary = new KYCSummary();
            _summary.BusinessProfileSummary = await IsBusinessProfileCompleted(businessProfileCode);

            _summary.isLicenseInfoCompleted = await IsLicenseInfoCompleted(businessProfileCode);
            _summary.isCoInfoCompleted = await IsCoInformationCompleted(businessProfileCode);
            _summary.isDocumentationCompleted = await IsDocumentationCompleted(businessProfileCode);

            _summary.OwnershipSummary = await IsOwnershipInfoCompleted(businessProfileCode);

            _summary.isAMLCompleted = await IsAmlCFTCompleted(businessProfileCode, solution);
            _summary.isDeclarationInfoCompleted = await IsDeclarationInfoCompleted(businessProfileCode);

            return _summary;
        }


        #region KYC Business Summary Mandatory Checking

        public async Task<KYCBusinessSummary> IsBusinessCustomerMandatoryFieldCompletedAsync(int businessProfileCode, List<Questionnaire> questionnaires = null)
        {
            var _summary = new KYCBusinessSummary();
            _summary.IsBusinessDeclarationCompleted = true;
            _summary.IsBusinessProfileCompleted = await IsBusinessUserBusinessProfileCompleted(businessProfileCode);
            _summary.IsOwnershipCompleted = await IsBusinessOwnershipInfoCompleted(businessProfileCode);
            _summary.IsDeclarationInfoCompleted = await IsBusinessUserDeclarationInfoCompleted(businessProfileCode);

            _summary.IsCompanyDetailCompleted = await IsBusinessUserBusinessProfileCompanyDetailsCompleted(businessProfileCode);
            _summary.IsAddressCompleted = await IsBusinessUserBusinessProfileAddressCompleted(businessProfileCode);
            _summary.IsCompanyContactCompleted = await IsBusinessUserBusinessProfileCompanyContactCompleted(businessProfileCode);
            _summary.IsContactPersonCompleted = await IsBusinessUserBusinessProfileContactPersonCompleted(businessProfileCode);

            _summary.IsShareholderCompleted = await IsBusinessOwnershipShareholderCompleted(businessProfileCode);
            _summary.IsBoardOfDirectorCompleted = await IsBusinessOwnershipBoardOfDirectorCompleted(businessProfileCode);
            _summary.IsAuthorisedPersonsCompleted = await IsBusinessOwnershipAuthorisedPersonCompleted(businessProfileCode);
            _summary.IsUltimateBeneficialOwnerCompleted = await IsBusinessOwnershipLegalEntityCompleted(businessProfileCode);
            _summary.IsPrincipalOfficerCompleted = await IsBusinessOwnershipPrimaryOfficerCompleted(businessProfileCode);

            _summary.IsDocumentationCompleted = await IsBusinessDocumentationCompleted(businessProfileCode);
            _summary.IsLicenseInfoCompleted = await IsBusinessLicenseInfoCompleted(businessProfileCode);

            return _summary;
        }

        public async Task<KYCNormalCorporateBusinessSummary> IsNormalCorporateCustomerMandatoryFieldCompletedAsync(int businessProfileCode, List<Questionnaire> questionnaires = null)
        {
            var _normalCorporateSummary = new KYCNormalCorporateBusinessSummary();
            _normalCorporateSummary.isBusinessProfileCompleted = await IsBusinessUserBusinessProfileCompleted(businessProfileCode);
            _normalCorporateSummary.isLicenseInfoCompleted = await IsBusinessLicenseInfoCompleted(businessProfileCode);
            _normalCorporateSummary.isOwnershipCompleted = await IsBusinessOwnershipInfoCompleted(businessProfileCode);
            _normalCorporateSummary.isDocumentationCompleted = await IsDocumentationCompleted(businessProfileCode);
            _normalCorporateSummary.isBusinessUserDeclarationInfoCompleted = await IsBusinessUserDeclarationInfoCompleted(businessProfileCode);

            return _normalCorporateSummary;
        }

        private async Task<bool> IsBusinessUserBusinessProfileCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerProfile.CustomerType.Id);
            var customerEntityType = await _partnerRepository.GetTrangloEntitiesByPartnerAsync(partnerProfile.Id);

            //Validate if have Comment for this
            var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(businessProfileCode);
            if (kycSummaryFeedbackInfo != null)
            {
                foreach (var i in kycSummaryFeedbackInfo)
                {
                    if (i.KYCCategory != KYCCategory.Business_BusinessProfile)
                    {
                        continue;
                    }
                    var unresolvedCount = kycSummaryFeedbackInfo
                        .Count(a => a.KYCCategory == KYCCategory.Business_BusinessProfile && !a.IsResolved);

                    if (unresolvedCount > 0)
                    {
                        return false;
                    }
                }
            }

            foreach (var entity in customerEntityType)
            {
                // Perform operations using the 'entity'
                var trangloEntity = entity;
            }
            bool isTrangloEntityTEL = customerEntityType.Contains(TrangloEntity.TEL.TrangloEntityCode);
            bool isTrangloEntityTSB = customerEntityType.Contains(TrangloEntity.TSB.TrangloEntityCode);


            // Checking per Customer Type for Business Profile mandatory check
            if (businessProfile.IsSuccess)
            {
                var profile = businessProfile.Value;
                var HasService = (profile.IsEMoneyEwallet.HasValue && profile.IsEMoneyEwallet.Value != false) ||
                          (profile.IsForeignCurrencyExchange.HasValue && profile.IsForeignCurrencyExchange.Value != false) ||
                          (profile.IsRetailCommercialBankingServices.HasValue && profile.IsRetailCommercialBankingServices.Value != false) ||
                          (profile.IsForexTrading.HasValue && profile.IsForexTrading.Value != false) ||
                          (profile.IsMoneyTransferRemittance.HasValue && profile.IsMoneyTransferRemittance.Value != false) ||
                          (profile.IsIntermediataryRemittance.HasValue && profile.IsIntermediataryRemittance.Value != false) ||
                          (profile.IsCryptocurrency.HasValue && profile.IsCryptocurrency.Value != false) ||
                          (profile.IsOther.HasValue && profile.IsOther.HasValue == true && !string.IsNullOrEmpty(profile.OtherReason));
                bool isCompleted = false;

                if (customerType == CustomerType.Individual)
                {
                    isCompleted = profile.CompanyRegistrationName != null &&
                                  profile.BusinessNature != null &&
                                  profile.NationalityCode != null &&
                                  profile.CompanyRegisteredAddress != null &&
                                  !string.IsNullOrEmpty(profile.SenderCity) &&
                                  //profile.CompanyRegisteredZipCodePostCode != null &&
                                  profile.CompanyRegisteredCountryCode != null &&
                                  profile.DateOfBirth.HasValue &&
                                  profile.BusinessProfileIDType != null &&
                                  profile.IDNumber != null &&
                                  (profile.BusinessProfileIDType != IDType.International_Passport || profile.IDExpiryDate.HasValue) &&
                                  ContactNumber.Create(profile.ContactNumber.DialCode, profile.ContactNumber.CountryISO2Code, profile.ContactNumber.Value).IsSuccess &&
                                  (partnerProfile.Email != null && Email.Create(partnerProfile.Email.Value).IsSuccess);
                }
                else if (customerType == CustomerType.Corporate_Mass_Payout || customerType == CustomerType.Corporate_Normal_Corporate)
                {

                    if (isCompleted =
                                  //profile.RelationshipTieUpCode != null &&
                                  profile.IncorporationCompanyTypeCode != null &&
                                  profile.CompanyRegistrationName != null &&
                                  profile.BusinessNature != null &&
                                  profile.CompanyRegisteredAddress != null &&
                                  !string.IsNullOrEmpty(profile.SenderCity) &&
                                  //profile.CompanyRegisteredZipCodePostCode != null &&
                                  profile.CompanyRegisteredCountryCode != null &&
                                  profile.MailingAddress != null &&
                                  //profile.MailingZipCodePostCode != null &&
                                  profile.MailingCountryCode != null &&
                                  !string.IsNullOrEmpty(profile.CompanyRegistrationNo) &&
                                  profile.DateOfIncorporation.HasValue &&
                                  profile.ContactNumber != null &&
                                  ContactNumber.Create(profile.TelephoneNumber.DialCode, profile.TelephoneNumber.CountryISO2Code, profile.TelephoneNumber.Value).IsSuccess &&
                                  profile.ContactPersonName != null &&
                                  (partnerProfile.Email != null && Email.Create(partnerProfile.Email.Value).IsSuccess)
                                  && profile.SenderCity != null)
                        //&& (!profile.IsCompanyListed.HasValue ||
                        // (profile.IsCompanyListed.HasValue && (profile.IsCompanyListed.Value == false ||
                        //                                      (profile.IsCompanyListed.Value == true &&
                        //                                       !string.IsNullOrEmpty(profile.StockCode) &&
                        //                                       !string.IsNullOrEmpty(profile.StockExchangeName))))))


                        if (profile.CompanyRegisteredCountryCode == CountryMeta.Malaysia.Id)
                        {
                            if (string.IsNullOrEmpty(profile.TaxIdentificationNo))
                            {
                                return false;
                            }
                        }

                    {
                        if (isCompleted && profile.SSTRegistrationNumber != null && profile.SSTRegistrationNumber != "")
                        {
                            if (!string.IsNullOrEmpty(profile.SenderCity))
                            {
                                return true;
                            }

                        }
                        else
                        {
                            return false;
                        }



                        return false;
                    }

                }
                else if (customerType == CustomerType.Remittance_Partner || customerType == CustomerType.Corporate_Cryptocurrency_Exchange)
                {
                    isCompleted = profile.RelationshipTieUpCode != null &&
                                   profile.IncorporationCompanyTypeCode != null &&
                                   profile.CompanyRegistrationName != null &&
                                   profile.BusinessNature != null &&
                                   profile.CompanyRegisteredAddress != null &&
                                   !string.IsNullOrEmpty(profile.SenderCity) &&
                                   //profile.CompanyRegisteredZipCodePostCode != null &&
                                   profile.CompanyRegisteredCountryCode != null &&
                                   profile.MailingAddress != null &&
                                   //profile.MailingZipCodePostCode != null &&
                                   profile.MailingCountryCode != null &&
                                   profile.CompanyRegistrationNo != null &&
                                   profile.DateOfIncorporation.HasValue &&
                                   //profile.NumberOfBranches.HasValue &&
                                   ContactNumber.Create(profile.TelephoneNumber.DialCode, profile.TelephoneNumber.CountryISO2Code, profile.TelephoneNumber.Value).IsSuccess &&
                                   profile.ContactPersonName != null &&
                                   ContactNumber.Create(profile.ContactNumber.DialCode, profile.ContactNumber.CountryISO2Code, profile.ContactNumber.Value).IsSuccess &&
                                   (partnerProfile.Email != null && Email.Create(partnerProfile.Email.Value).IsSuccess) &&
                                   !string.IsNullOrEmpty(profile.Website) &&
                                   (!profile.IsCompanyListed.HasValue ||
                                    (profile.IsCompanyListed.HasValue && (profile.IsCompanyListed.Value == false ||
                                                                         (profile.IsCompanyListed.Value == true &&
                                                                          !string.IsNullOrEmpty(profile.StockCode) &&
                                                                          !string.IsNullOrEmpty(profile.StockExchangeName))))) &&
                                   (!isTrangloEntityTEL || (isTrangloEntityTEL && profile.IsMicroEnterprise.HasValue && profile.IsMicroEnterprise.Value)
                                   && profile.SenderCity != null);


                    if (profile.CompanyRegisteredCountryCode == CountryMeta.Malaysia.Id)
                    {
                        if (string.IsNullOrEmpty(profile.TaxIdentificationNo))
                        {
                            return false;
                        }
                    }
                }





                return isCompleted;
            }

            return false;
        }

        private async Task<bool> IsBusinessUserBusinessProfileCompanyDetailsCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerProfile.CustomerType.Id);
            var customerEntityType = await _partnerRepository.GetTrangloEntitiesByPartnerAsync(partnerProfile.Id);

            foreach (var entity in customerEntityType)
            {
                // Perform operations using the 'entity'
                var trangloEntity = entity;
            }
            bool isTrangloEntityTEL = customerEntityType.Contains(TrangloEntity.TEL.TrangloEntityCode);
            bool isTrangloEntityTSB = customerEntityType.Contains(TrangloEntity.TSB.TrangloEntityCode);


            // Checking per Customer Type for Business Profile mandatory check
            if (businessProfile.IsSuccess)
            {
                var profile = businessProfile.Value;

                bool isCompleted = false;

                if (customerType == CustomerType.Corporate_Mass_Payout || customerType == CustomerType.Corporate_Normal_Corporate)
                {

                    isCompleted = profile.IncorporationCompanyTypeCode != null &&
                                  profile.CompanyRegistrationName != null &&
                                  profile.BusinessNature != null &&
                                  profile.DateOfIncorporation.HasValue &&
                                  !string.IsNullOrEmpty(profile.CompanyRegistrationNo);

                    if (profile.CompanyRegisteredCountryCode == CountryMeta.Malaysia.Id)
                    {
                        if (string.IsNullOrEmpty(profile.TaxIdentificationNo))
                        {
                            return false;
                        }
                    }

                    if (profile.SSTRegistrationNumber != null && profile.SSTRegistrationNumber != "")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }

                return isCompleted;
            }

            return false;
        }

        private async Task<bool> IsBusinessUserBusinessProfileAddressCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerProfile.CustomerType.Id);
            var customerEntityType = await _partnerRepository.GetTrangloEntitiesByPartnerAsync(partnerProfile.Id);

            foreach (var entity in customerEntityType)
            {
                // Perform operations using the 'entity'
                var trangloEntity = entity;
            }
            bool isTrangloEntityTEL = customerEntityType.Contains(TrangloEntity.TEL.TrangloEntityCode);
            bool isTrangloEntityTSB = customerEntityType.Contains(TrangloEntity.TSB.TrangloEntityCode);


            // Checking per Customer Type for Business Profile mandatory check
            if (businessProfile.IsSuccess)
            {
                var profile = businessProfile.Value;
                bool isCompleted = false;

                if (customerType == CustomerType.Corporate_Mass_Payout || customerType == CustomerType.Corporate_Normal_Corporate)
                {

                    isCompleted =
                                  profile.CompanyRegisteredAddress != null &&
                                  //profile.CompanyRegisteredZipCodePostCode != null &&
                                  profile.CompanyRegisteredCountryCode != null &&
                                  profile.MailingAddress != null &&
                                  //profile.MailingZipCodePostCode != null &&
                                  profile.MailingCountryCode != null;

                }

                return isCompleted;
            }

            return false;
        }

        private async Task<bool> IsBusinessUserBusinessProfileContactPersonCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerProfile.CustomerType.Id);
            var customerEntityType = await _partnerRepository.GetTrangloEntitiesByPartnerAsync(partnerProfile.Id);

            foreach (var entity in customerEntityType)
            {
                // Perform operations using the 'entity'
                var trangloEntity = entity;
            }
            bool isTrangloEntityTEL = customerEntityType.Contains(TrangloEntity.TEL.TrangloEntityCode);
            bool isTrangloEntityTSB = customerEntityType.Contains(TrangloEntity.TSB.TrangloEntityCode);


            // Checking per Customer Type for Business Profile mandatory check
            if (businessProfile.IsSuccess)
            {
                var profile = businessProfile.Value;
                bool isCompleted = false;

                if (profile.TelephoneNumber != null)
                {
                    if (customerType == CustomerType.Corporate_Mass_Payout || customerType == CustomerType.Corporate_Normal_Corporate)
                    {

                        isCompleted =
                                      ContactNumber.Create(profile.TelephoneNumber.DialCode, profile.TelephoneNumber.CountryISO2Code, profile.TelephoneNumber.Value).IsSuccess &&
                                      profile.ContactPersonName != null &&
                                      (partnerProfile.Email != null && Email.Create(partnerProfile.Email.Value).IsSuccess);

                    }

                    return isCompleted;
                }
            }

            return false;
        }

        private async Task<bool> IsBusinessUserBusinessProfileCompanyContactCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerProfile.CustomerType.Id);
            var customerEntityType = await _partnerRepository.GetTrangloEntitiesByPartnerAsync(partnerProfile.Id);

            foreach (var entity in customerEntityType)
            {
                // Perform operations using the 'entity'
                var trangloEntity = entity;
            }
            bool isTrangloEntityTEL = customerEntityType.Contains(TrangloEntity.TEL.TrangloEntityCode);
            bool isTrangloEntityTSB = customerEntityType.Contains(TrangloEntity.TSB.TrangloEntityCode);


            // Checking per Customer Type for Business Profile mandatory check
            if (businessProfile.IsSuccess)
            {
                var profile = businessProfile.Value;
                bool isCompleted = false;

                if (profile.TelephoneNumber != null)
                {
                    if (customerType == CustomerType.Corporate_Mass_Payout || customerType == CustomerType.Corporate_Normal_Corporate)
                    {
                        isCompleted = ContactNumber.Create(profile.TelephoneNumber.DialCode, profile.TelephoneNumber.CountryISO2Code, profile.TelephoneNumber.Value).IsSuccess;

                        return isCompleted;
                    }
                }


            }

            return false;
        }

        //Ownership Checking
        #region Business Ownership Checking
        public async Task<bool> IsBusinessOwnershipShareholderCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            // Check Individual Shareholders
            var individualShareholderResult = await GetIndividualShareholderByBusinessProfileCodeAsync(businessProfile.Value);
            var hasIndividualShareholders = individualShareholderResult.Value.Any();

            if (hasIndividualShareholders)
            {
                foreach (var individualShareholder in individualShareholderResult.Value)
                {
                    if (!individualShareholder.IsTBCompleted())
                    {
                        return false;
                    }
                }
            }

            // Check Company Shareholders
            var companyShareholderResult = await GetCompanyShareholderByBusinessProfileCodeAsync(businessProfile.Value);
            var hasCompanyShareholders = companyShareholderResult.Value.Any();

            if (hasCompanyShareholders)
            {
                var individualUltimateShareholderResult = await GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
                var companyUltimateShareholderResult = await GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
                foreach (var companyShareholder in companyShareholderResult.Value)
                {
                    if (!companyShareholder.IsCompleted() ||
                        !companyShareholder.IsUltimateShareholderCompleted(Solution.Business, individualUltimateShareholderResult.Value, companyUltimateShareholderResult.Value))
                    {
                        return false;
                    }
                }
            }

            // Return true only if at least one type of shareholder exists and all are completed
            return hasIndividualShareholders || hasCompanyShareholders;
        }

        public async Task<bool> IsBusinessOwnershipLegalEntityCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var legalEntityIndividual = await GetIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            if (legalEntityIndividual.Value.Count == 0)
            {
                return false;
            }
            if (legalEntityIndividual.IsSuccess && legalEntityIndividual.Value != null)
            {
                foreach (var item in legalEntityIndividual.Value)
                {
                    var isCompleted = item.IsTBCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }

            var legalEntityCompany = await GetCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            if (legalEntityCompany.IsSuccess && legalEntityCompany.Value != null)
            {
                foreach (var item in legalEntityCompany.Value)
                {
                    var isCompleted = item.IsTBCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            return true;
        }

        public async Task<bool> IsBusinessOwnershipSubsidiaryCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var subsidiary = await GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile.Value);
            if (subsidiary.IsSuccess && subsidiary.Value != null)
            {
                foreach (var item in subsidiary.Value)
                {
                    if (string.IsNullOrEmpty(item.CompanyName) ||
                        !item.DateOfIncorporation.HasValue ||
                        string.IsNullOrEmpty(item.CompanyRegNo) ||
                        item.Country == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<bool> IsBusinessOwnershipParentHoldingCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var parent = await GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile.Value);
            if (parent.IsSuccess && parent.Value != null)
            {
                foreach (var item in parent.Value)
                {
                    if (string.IsNullOrEmpty(item.NameOfListedParentHoldingCompany) ||
                        item.Country == null ||
                        !item.DateOfIncorporation.HasValue)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<bool> IsBusinessOwnershipBoardOfDirectorCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var BOD = await GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile.Value);
            if (BOD.IsSuccess && BOD.Value != null)
            {
                foreach (var item in BOD.Value)
                {
                    var isCompleted = item.IsCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            if (((BOD.IsSuccess && BOD.Value.Count > 0)))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsBusinessOwnershipAuthorisedPersonCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var authorisedPerson = await GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile.Value);
            if (authorisedPerson.IsSuccess && authorisedPerson.Value.Count > 0)
            {
                foreach (var item in authorisedPerson.Value)
                {
                    var isCompleted = item.IsTBCompleted();

                    if (!isCompleted)
                    {
                        return false; // if any item is incomplete, fail early
                    }
                }

                // If loop completes, all items are completed
                return true;
            }

            return false;

        }

        public async Task<bool> IsBusinessOwnershipPrimaryOfficerCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var primary = await GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile.Value);
            if (primary.IsSuccess && primary.Value != null)
            {
                foreach (var item in primary.Value)
                {
                    var isCompleted = item.IsCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            if (((primary.IsSuccess && primary.Value.Count > 0)))
            {
                return true;
            }

            return false;

        }

        private async Task<bool> IsBusinessOwnershipInfoCompleted(int businessProfileCode)
        {
            var customerType = await _partnerRepository.GetCustomerTypeByBusinessProfileAsync(businessProfileCode);
            if (customerType == CustomerType.Individual)
            {
                // Individual customer type doesn't require ownership info checking
                return true;
            }

            if (customerType == CustomerType.Corporate_Normal_Corporate)
            {
                return await CheckNormalCorporateOwnershipCompletion(businessProfileCode);
            }
            else
            {
                return await CheckOtherCorporateOwnershipCompletion(businessProfileCode);
            }
        }

        public async Task<bool> IsBusinessDocumentationCompleted(int businessProfileCode)
        {
            //extra validation for TB Corporate 
            var partnerInfo = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionByPartnerCodeAsync(partnerInfo.Id);
            var uboInfo = await _businessProfileRepository.GetLegalEntityAsync(businessProfileCode);
            if (partnerSubInfo.Solution == Solution.Business)
            {
                if (uboInfo != null)
                {
                    if (decimal.TryParse(uboInfo.EffectiveShareholding, out var shareholding) && shareholding >= 25)
                    {

                        var documentCategoryBPInfo = await Repository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 64); // 64 refer to Copies of passport / Other identification documents of ultimate beneficial owner 
                        if (documentCategoryBPInfo == null)
                        {
                            return false;
                        }
                        var documentUploadInfo = await Repository.GetDocumentUploadBPsAsync(documentCategoryBPInfo.Id);
                        if (partnerSubInfo.Solution == Solution.Business && partnerInfo.CustomerTypeCode == CustomerType.Corporate_Normal_Corporate.Id)
                        {
                            if (documentUploadInfo.Count == 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            //Validate if have Comment for this
            var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(businessProfileCode);
            if (kycSummaryFeedbackInfo != null)
            {
                foreach (var i in kycSummaryFeedbackInfo)
                {
                    if (i.KYCCategory != KYCCategory.Business_Documentation)
                    {
                        continue;
                    }

                    var unresolvedCount = kycSummaryFeedbackInfo
                        .Count(a => a.KYCCategory == KYCCategory.Business_Documentation && !a.IsResolved);

                    if (unresolvedCount > 0)
                    {
                        return false;
                    }
                }
            }


            return true;
        }

        private async Task<bool> CheckNormalCorporateOwnershipCompletion(int businessProfileCode)
        {
            var businessProfileInfo = await _businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);

            if (businessProfileInfo.IncorporationCompanyTypeCode == IncorporationCompanyType.Sole_Proprietorship.Id)
            {
                var authorisedPersonsCompleted = await IsBusinessOwnershipAuthorisedPersonCompleted(businessProfileCode);
                var ultimateBeneficialCompleted = await IsBusinessOwnershipLegalEntityCompleted(businessProfileCode);

                return
                       authorisedPersonsCompleted &&
                       ultimateBeneficialCompleted;
            }
            else
            {
                var shareholderCompleted = await IsBusinessOwnershipShareholderCompleted(businessProfileCode);
                var boardOfDirectorCompleted = await IsBusinessOwnershipBoardOfDirectorCompleted(businessProfileCode);
                var authorisedPersonsCompleted = await IsBusinessOwnershipAuthorisedPersonCompleted(businessProfileCode);
                var ultimateBeneficialCompleted = await IsBusinessOwnershipLegalEntityCompleted(businessProfileCode);


                var isOverallCompleted =
                            shareholderCompleted &&
                            boardOfDirectorCompleted &&
                            authorisedPersonsCompleted &&
                            ultimateBeneficialCompleted;



                //Validate if have Comment for this
                var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(businessProfileCode);

                if (kycSummaryFeedbackInfo != null)
                {
                    foreach (var i in kycSummaryFeedbackInfo)
                    {
                        if (i.KYCCategory != KYCCategory.Business_Ownership)
                        {
                            continue;
                        }

                        var unresolvedCount = kycSummaryFeedbackInfo
                                .Count(a => a.KYCCategory == KYCCategory.Business_Ownership && !a.IsResolved);

                        if (isOverallCompleted && unresolvedCount > 0)
                        {
                            isOverallCompleted = false;
                        }
                    }
                }

                return isOverallCompleted;
            }
        }

        private async Task<bool> CheckOtherCorporateOwnershipCompletion(int businessProfileCode)
        {
            var shareholderCompleted = await IsBusinessOwnershipShareholderCompleted(businessProfileCode);
            var legalEntityCompleted = await IsBusinessOwnershipLegalEntityCompleted(businessProfileCode);
            var subsidiaryCompleted = await IsBusinessOwnershipSubsidiaryCompleted(businessProfileCode);
            var parentHoldingCompleted = await IsBusinessOwnershipParentHoldingCompleted(businessProfileCode);
            var primaryOfficerCompleted = await IsBusinessOwnershipPrimaryOfficerCompleted(businessProfileCode);
            var boardOfDirectorCompleted = await IsBusinessOwnershipBoardOfDirectorCompleted(businessProfileCode);
            var authorisedPersonsCompleted = await IsBusinessOwnershipAuthorisedPersonCompleted(businessProfileCode);

            return shareholderCompleted &&
                   legalEntityCompleted &&
                   subsidiaryCompleted &&
                   parentHoldingCompleted &&
                   primaryOfficerCompleted &&
                   boardOfDirectorCompleted &&
                   authorisedPersonsCompleted;
        }

        //End of Ownership Checking
        #endregion

        private async Task<bool> IsBusinessUserDeclarationInfoCompleted(int businessProfileCode)
        {
            var declaration = await GetKYCBusinessDeclarationInfoAsync(businessProfileCode);
            var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partner.CustomerTypeCode);
            if (declaration != null)
            {
                if (customerType.Id == CustomerType.Remittance_Partner.Id)
                {
                    if (declaration.IsAllApplicationAccurate.HasValue && declaration.IsAllApplicationAccurate == true)
                    {
                        return true;
                    }
                    else if ((declaration.IsAgreedTermsOfService.HasValue && declaration.IsAgreedTermsOfService == true) &&
                        !string.IsNullOrEmpty(declaration.SigneeName) &&
                        !string.IsNullOrEmpty(declaration.Designation) &&
                        (declaration.IsAuthorized.HasValue && declaration.IsAuthorized == true) &&
                        (declaration.IsDeclareTransactionTax.HasValue && declaration.IsDeclareTransactionTax == true) &&
                        (declaration.IsInformationTrue.HasValue && declaration.IsInformationTrue == true) &&
                        (declaration.DocumentId != null && declaration.DocumentId != Guid.Empty))
                    {
                        return true;
                    }
                }

                else if (customerType.Id != CustomerType.Remittance_Partner.Id)
                {
                    if (declaration.IsAllApplicationAccurate.HasValue && declaration.IsAllApplicationAccurate == true)
                    {
                        return true;
                    }
                    else if ((declaration.IsAgreedTermsOfService.HasValue && declaration.IsAgreedTermsOfService == true) &&
                        !string.IsNullOrEmpty(declaration.SigneeName) &&
                        !string.IsNullOrEmpty(declaration.Designation) &&
                        (declaration.IsNotRemittancePartner.HasValue && declaration.IsNotRemittancePartner == true) &&
                        (declaration.IsAuthorized.HasValue && declaration.IsAuthorized == true) &&
                        (declaration.IsDeclareTransactionTax.HasValue && declaration.IsDeclareTransactionTax == true) &&
                        (declaration.IsInformationTrue.HasValue && declaration.IsInformationTrue == true) &&
                        (declaration.DocumentId != null && declaration.DocumentId != Guid.Empty))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        private async Task<bool> IsBusinessLicenseInfoCompleted(int businessProfileCode)
        {
            var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partner.CustomerType.Id);
            var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(businessProfileCode);
            var license = await GetLicenseInfoByBusinessCode(businessProfileCode);
            if (license != null)
            {
                if (license.IsLicenseRequired == true)
                {
                    if (customerType == CustomerType.Remittance_Partner)
                    {
                        if (license.IsLicenseRequired.HasValue && (license.IsLicenseRequired.Value == false &&
                             !string.IsNullOrEmpty(license.Remark) && (license.RegulatorDocumentId != null) ||
                             (license.IsLicenseRequired.Value == true &&
                             !string.IsNullOrEmpty(license.LicenseCertNumber) &&
                             !string.IsNullOrEmpty(license.LicenseType) &&
                             !string.IsNullOrEmpty(license.PrimaryRegulatorLicenseService) &&
                             !string.IsNullOrEmpty(license.PrimaryRegulatorAMLCFT) &&
                             !string.IsNullOrEmpty(license.ActLawRemittanceLicense) &&
                             !string.IsNullOrEmpty(license.ActLawAMLCFT) &&
                             license.IssuedDate.HasValue && license.IssuedDate != DateTime.MinValue)))
                        {
                            return true;
                        }
                    }

                    else if (customerType == CustomerType.Corporate_Cryptocurrency_Exchange)
                    {
                        if (license.IsLicenseRequired.HasValue && (license.IsLicenseRequired.Value == false &&
                             !string.IsNullOrEmpty(license.Remark) && (license.RegulatorDocumentId != null) || !string.IsNullOrEmpty(license.RegulatorWebsite)) ||
                             (license.IsLicenseRequired.Value == true &&
                             !string.IsNullOrEmpty(license.LicenseCertNumber) &&
                             !string.IsNullOrEmpty(license.LicenseType) &&
                             !string.IsNullOrEmpty(license.PrimaryRegulatorLicenseService) &&
                             !string.IsNullOrEmpty(license.PrimaryRegulatorAMLCFT) &&
                             !string.IsNullOrEmpty(license.ActLawRemittanceLicense) &&
                             !string.IsNullOrEmpty(license.ActLawAMLCFT) &&
                             license.IssuedDate.HasValue && license.IssuedDate != DateTime.MinValue))
                        {
                            return true;
                        }
                    }

                    else if (customerType == CustomerType.Corporate_Mass_Payout || customerType == CustomerType.Corporate_Normal_Corporate)
                    {
                        if (license.IsLicenseRequired.HasValue && (license.IsLicenseRequired.Value == false &&
                             !string.IsNullOrEmpty(license.Remark) ||
                             (license.IsLicenseRequired.Value == true &&
                             !string.IsNullOrEmpty(license.LicenseCertNumber) &&
                             !string.IsNullOrEmpty(license.LicenseType) &&
                             !string.IsNullOrEmpty(license.PrimaryRegulatorLicenseService) &&
                             !string.IsNullOrEmpty(license.PrimaryRegulatorAMLCFT) &&
                             !string.IsNullOrEmpty(license.ActLawRemittanceLicense) &&
                             !string.IsNullOrEmpty(license.ActLawAMLCFT) &&
                             license.IssuedDate.HasValue && license.IssuedDate != DateTime.MinValue &&
                             (license.ExpiryDate == null || license.ExpiryDate > DateTime.Now))))
                        {

                            return true;
                        }
                    }
                    //Validate if have Comment for this

                    if (kycSummaryFeedbackInfo != null)
                    {
                        foreach (var i in kycSummaryFeedbackInfo)
                        {
                            if (i.KYCCategory != KYCCategory.Business_LicenseInfo)
                            {
                                continue;
                            }

                            var unresolvedCount = kycSummaryFeedbackInfo
                                .Count(a => a.KYCCategory == KYCCategory.Business_LicenseInfo && !a.IsResolved);

                            if (unresolvedCount > 0)
                            {
                                return false;
                            }
                        }
                    }
                }
                //Validate if have Comment for this

                if (kycSummaryFeedbackInfo != null)
                {
                    foreach (var i in kycSummaryFeedbackInfo)
                    {
                        if (i.KYCCategory != KYCCategory.Business_LicenseInfo)
                        {
                            continue;
                        }

                        var unresolvedCount = kycSummaryFeedbackInfo
                            .Count(a => a.KYCCategory == KYCCategory.Business_LicenseInfo && !a.IsResolved);

                        if (unresolvedCount > 0)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }


            return false;
        }
        #endregion

        private async Task<bool> IsDocumentationCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var documentGroup = 1; //await GetDocumentCategoryGroupIdBySolution(businessProfile.Value.SolutionCode);

            //extra validation for TB Corporate 
            var partnerInfo = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionByPartnerCodeAsync(partnerInfo.Id);
            var uboInfo = await _businessProfileRepository.GetLegalEntityAsync(businessProfileCode);

            if (partnerSubInfo.Solution == Solution.Business)
            {
                if (uboInfo != null)
                {
                    if (decimal.TryParse(uboInfo.EffectiveShareholding, out var shareholding) && shareholding >= 25)
                    {

                        var documentCategoryBPInfo = await Repository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 64);
                        if (documentCategoryBPInfo == null)
                        {
                            return false;
                        }
                        var documentUploadInfo = await Repository.GetDocumentUploadBPsAsync(documentCategoryBPInfo.Id);
                        if (partnerSubInfo.Solution == Solution.Business && partnerInfo.CustomerTypeCode == CustomerType.Corporate_Normal_Corporate.Id)
                        {
                            if (documentUploadInfo.Count == 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            var documentCategory = await GetDocumentCategoryByGroupIdAsync(documentGroup);
            foreach (var item in documentCategory)
            {
                if (item.IsAMLCFT)
                {
                    var isAnsweredAML = await CheckHasAnsweredAMLQuestionnaire(businessProfileCode);
                    if (isAnsweredAML)
                    {
                        continue;
                    }
                }
            }

            return true;
        }

        #region Connect Ownership Checking
        public async Task<bool> IsOwnershipShareholderCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            // Check Individual Shareholders
            var individualShareholderResult = await GetIndividualShareholderByBusinessProfileCodeAsync(businessProfile.Value);
            var hasIndividualShareholders = individualShareholderResult.Value.Any();

            if (hasIndividualShareholders)
            {
                foreach (var individualShareholder in individualShareholderResult.Value)
                {
                    if (!individualShareholder.IsTCCompleted(isTCRevampFeature))
                    {
                        return false;
                    }
                }
            }

            // Check Company Shareholders
            var companyShareholderResult = await GetCompanyShareholderByBusinessProfileCodeAsync(businessProfile.Value);
            var hasCompanyShareholders = companyShareholderResult.Value.Any();

            if (hasCompanyShareholders)
            {
                var individualUltimateShareholderResult = await GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
                var companyUltimateShareholderResult = await GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
                foreach (var companyShareholder in companyShareholderResult.Value)
                {
                    if (!companyShareholder.IsCompleted() ||
                        !companyShareholder.IsUltimateShareholderCompleted(Solution.Connect, individualUltimateShareholderResult.Value, companyUltimateShareholderResult.Value))
                    {
                        return false;
                    }
                }
            }

            // Return true only if at least one type of shareholder exists and all are completed
            return hasIndividualShareholders || hasCompanyShareholders;
        }

        //TC and TB
        public async Task<List<(long shareholderCode, bool isCompleted)>> IsOwnershipShareholdersCompleted(int businessProfileCode, Solution solution)
        {
            var result = new List<(long shareholderCode, bool isCompleted)>();
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            // Check Individual Shareholders
            var individualShareholders = await GetIndividualShareholderByBusinessProfileCodeAsync(businessProfile.Value);
            if (individualShareholders.IsSuccess && individualShareholders.Value?.Any() == true)
            {
                foreach (var individualShareholder in individualShareholders.Value)
                {
                    bool isCompleted = solution == Solution.Business
                        ? individualShareholder.IsTBCompleted()
                        : individualShareholder.IsTCCompleted(isTCRevampFeature);

                    result.Add((individualShareholder.Id, isCompleted));
                }
            }

            // Fetch ultimate shareholders once to avoid repeated queries
            var individualUltimateShareholders = await GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            var companyUltimateShareholders = await GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);

            // A Company Shareholder can only be considered as completed when itself is completed and
            // its ultimate shareholders are completed (if any)
            var companyShareholders = await GetCompanyShareholderByBusinessProfileCodeAsync(businessProfile.Value);
            if (companyShareholders.IsSuccess && companyShareholders.Value?.Any() == true)
            {
                foreach (var shareholderCompany in companyShareholders.Value)
                {
                    bool isCompanyShareholderCompleted = shareholderCompany.IsCompleted();
                    bool isUltimateShareholdersCompleted = shareholderCompany.IsUltimateShareholderCompleted(solution, individualUltimateShareholders.Value, companyUltimateShareholders.Value);

                    var isCompleted = isCompanyShareholderCompleted && isUltimateShareholdersCompleted;
                    result.Add((shareholderCompany.Id, isCompleted));
                }
            }

            return result;
        }

        //TC 
        public async Task<bool> IsOwnershipLegalEntityCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var legalEntityIndividual = await GetIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            if (legalEntityIndividual.IsSuccess && legalEntityIndividual.Value != null)
            {
                var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                foreach (var item in legalEntityIndividual.Value)
                {
                    var isCompleted = item.IsTCCompleted(isTCRevampFeature);
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }

            var legalEntityCompany = await GetCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            if (legalEntityCompany.IsSuccess && legalEntityCompany.Value != null)
            {
                foreach (var item in legalEntityCompany.Value)
                {
                    var isCompleted = item.IsTCCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }

            return true;
        }

        //TB & TC
        public async Task<List<bool>> IsOwnershipLegalEntitiesCompleted(int businessProfileCode, Solution solution)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var legalEntityIndividual = await GetIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            List<bool> isLegalEntityCompleted = new List<bool>();
            if (legalEntityIndividual.IsSuccess && legalEntityIndividual.Value != null)
            {
                var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                foreach (var item in legalEntityIndividual.Value)
                {
                    bool isCompleted = true;

                    isCompleted = solution == Solution.Connect ? item.IsTCCompleted(isTCRevampFeature) : item.IsTBCompleted();

                    isLegalEntityCompleted.Add(isCompleted);
                }
            }

            var legalEntityCompany = await GetCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);
            if (legalEntityCompany.IsSuccess && legalEntityCompany.Value != null)
            {
                foreach (var item in legalEntityCompany.Value)
                {
                    bool isCompleted = true;

                    isCompleted = isCompleted = solution == Solution.Connect ? item.IsTCCompleted() : item.IsTBCompleted();

                    isLegalEntityCompleted.Add(isCompleted);
                }
            }
            return isLegalEntityCompleted;
        }


        public async Task<bool> IsOwnershipSubsidiaryCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var subsidiary = await GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile.Value);
            if (subsidiary.IsSuccess && subsidiary.Value != null)
            {
                foreach (var item in subsidiary.Value)
                {
                    var isCompleted = item.IsCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            return true;
        }

        public async Task<List<bool>> IsOwnershipSubsidiariesCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var subsidiary = await GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile.Value);

            List<bool> completedSubs = new List<bool>();
            foreach (var item in subsidiary.Value)
            {
                bool isComplete = true;

                isComplete = item.IsCompleted();

                completedSubs.Add(isComplete);

            }
            return completedSubs;
        }

        public async Task<bool> IsOwnershipParentHoldingCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var parent = await GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile.Value);
            if (parent.IsSuccess && parent.Value != null)
            {
                foreach (var item in parent.Value)
                {
                    if (string.IsNullOrEmpty(item.NameOfListedParentHoldingCompany) ||
                        item.Country == null ||
                        !item.DateOfIncorporation.HasValue)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<List<bool>> IsOwnershipParentHoldingsCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var parent = await GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile.Value);
            List<bool> completedHolding = new List<bool>();
            if (parent.IsSuccess && parent.Value != null)
            {
                foreach (var item in parent.Value)
                {
                    bool isComplete = true;
                    if (string.IsNullOrEmpty(item.NameOfListedParentHoldingCompany) ||
                        item.Country == null ||
                        !item.DateOfIncorporation.HasValue)
                    {
                        isComplete = false;
                    }
                    completedHolding.Add(isComplete);
                }
            }
            return completedHolding;
        }

        //TC
        public async Task<bool> IsOwnershipBoardOfDirectorCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var BOD = await GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile.Value);
            if (BOD.IsSuccess && BOD.Value != null)
            {
                foreach (var item in BOD.Value)
                {
                    var isCompleted = item.IsCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            if (((BOD.IsSuccess && BOD.Value.Count > 0)))
            {
                return true;
            }

            return false;
        }

        //TC && TB
        public async Task<List<bool>> IsOwnershipBoardOfDirectorsCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var BOD = await GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile.Value);
            List<bool> completedBOD = new List<bool>();
            if (BOD.IsSuccess && BOD.Value != null)
            {
                foreach (var item in BOD.Value)
                {
                    bool isCompleted = true;
                    isCompleted = item.IsCompleted();

                    completedBOD.Add(isCompleted);
                }
            }
            bool isSuccess = false;
            if (((BOD.IsSuccess && BOD.Value.Count > 0)))
            {
                isSuccess = true;
            }
            completedBOD.Add(isSuccess);

            return completedBOD;
        }

        public async Task<bool> IsOwnershipAuthorisedPersonCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var authorisedPerson = await GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile.Value);
            if (authorisedPerson.IsSuccess && authorisedPerson.Value != null)
            {
                var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                foreach (var item in authorisedPerson.Value)
                {
                    var isCompleted = item.IsTCCompleted(isTCRevampFeature);
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            if (((authorisedPerson.IsSuccess && authorisedPerson.Value.Count > 0)))
            {
                return true;
            }

            return false;
        }

        //TC & TB

        public async Task<List<bool>> IsOwnershipAuthorisedPersonsCompleted(int businessProfileCode, Solution solution)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var authorisedPerson = await GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile.Value);
            List<bool> isAuthorisedPersonComplete = new List<bool>();
            if (authorisedPerson.IsSuccess && authorisedPerson.Value != null)
            {
                var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                foreach (var item in authorisedPerson.Value)
                {
                    bool isCompleted = true;
                    isCompleted = solution == Solution.Business ? item.IsTBCompleted() : item.IsTCCompleted(isTCRevampFeature);

                    isAuthorisedPersonComplete.Add(isCompleted);
                }
            }
            return isAuthorisedPersonComplete;
        }

        public async Task<bool> IsOwnershipPrimaryOfficerCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var primary = await GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile.Value);
            if (primary.IsSuccess && primary.Value != null)
            {
                foreach (var item in primary.Value)
                {
                    var isCompleted = item.IsCompleted();
                    if (isCompleted == false)
                    {

                        return isCompleted;
                    }
                }
            }
            if (((primary.IsSuccess && primary.Value.Count > 0)))
            {
                return true;
            }

            return false;

        }

        public async Task<List<bool>> IsOwnershipPrimaryOfficersCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);

            var primary = await GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile.Value);
            List<bool> completedPrimaryOfficers = new List<bool>();
            if (primary.IsSuccess && primary.Value != null)
            {
                foreach (var item in primary.Value)
                {
                    bool isCompleted = true;
                    isCompleted = item.IsCompleted();

                    completedPrimaryOfficers.Add(isCompleted);
                }
            }

            bool isSuccess = false;
            if (((primary.IsSuccess && primary.Value.Count > 0)))
            {
                isSuccess = true;
            }
            completedPrimaryOfficers.Add(isSuccess);

            return completedPrimaryOfficers;

        }

        //TC
        private async Task<KYCOwnershipSummary> IsOwnershipInfoCompleted(int businessProfileCode)
        {
            var ownershipSummary = new KYCOwnershipSummary();

            //Shareholders
            ownershipSummary.IsShareholderCompleted = await IsOwnershipShareholderCompleted(businessProfileCode);

            //Ultimate Beneficial Owner
            ownershipSummary.IsUltimateBeneficialOwnerCompleted = await IsOwnershipLegalEntityCompleted(businessProfileCode);

            //Board of Directors
            ownershipSummary.IsBoardOfDirectorCompleted = await IsOwnershipBoardOfDirectorCompleted(businessProfileCode);

            //Pricipal Officer
            ownershipSummary.IsPrincipalOfficerCompleted = await IsOwnershipPrimaryOfficerCompleted(businessProfileCode);

            //Authorised Person
            ownershipSummary.IsAuthorisedPersonCompleted = await IsOwnershipAuthorisedPersonCompleted(businessProfileCode);

            //Licensed Parent Company, Subsidiary and Affiliate
            ownershipSummary.IsLicensedParentCompanyCompleted = await IsOwnershipSubsidiaryCompleted(businessProfileCode);

            // var parentHoldingCompleted = await isOwnershipParentHoldingCompleted(businessProfileCode);
            //var politicallyExposedPersonCompleted = await isOwnershipPoliticallyExposedPersonCompleted(businessProfileCode);

            return ownershipSummary;
        }

        #endregion

        private async Task<KYCBusinessProfileSummary> IsBusinessProfileCompleted(int businessProfileCode)
        {
            var businessProfile = await GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(businessProfileCode);

            var businessProfileSummary = new KYCBusinessProfileSummary();

            if (businessProfile.IsSuccess && partnerProfile != null)
            {
                businessProfileSummary.IsCompanyDetailsCompleted = CheckIsTCCompanyDetailsCompleted(businessProfile.Value);
                businessProfileSummary.IsAddressCompleted = CheckIsTCAddressCompleted(businessProfile.Value);
                businessProfileSummary.IsContactPersonCompleted = CheckIsTCContactPersonCompleted(businessProfile.Value, partnerProfile);

                return businessProfileSummary;
            }

            return businessProfileSummary;
        }

        #region Check is Mandatory in Business Profile Sub Category
        //Check TC BusinessProfile - Company Details
        public bool CheckIsTCCompanyDetailsCompleted(BusinessProfile businessProfile)
        {
            var isCompleted = false;

            var isBusinessNatureCompleted = businessProfile.BusinessNature != null && businessProfile.BusinessNature != BusinessNature.Other
                || (businessProfile.BusinessNature == BusinessNature.Other && IsCompleted(businessProfile.ForOthers));

            var isTaxIdentificationNumberCompleted = businessProfile.CompanyRegisteredCountryCode == CountryMeta.Malaysia.Id ?
            IsCompleted(businessProfile.TaxIdentificationNo) : true;

            if (IsCompleted(businessProfile.CompanyRegistrationName) && IsCompleted(businessProfile.CompanyRegistrationNo)
                && IsCompleted(businessProfile.TradeName) && IsCompleted(businessProfile.SSTRegistrationNumber)
                && businessProfile.DateOfIncorporation != null && businessProfile.IncorporationCompanyTypeCode != null
                && IsCompleted(businessProfile.Website) && businessProfile.RelationshipTieUpCode != null && businessProfile.EntityType != null
                && IsServiceOfferedCompleted(businessProfile) && isBusinessNatureCompleted && isTaxIdentificationNumberCompleted)
            {
                isCompleted = true;
            }

            return isCompleted;
        }

        public bool CheckIsTCAddressCompleted(BusinessProfile businessProfile)
        {
            var isCompleted = false;
            if (IsCompleted(businessProfile.CompanyRegisteredAddress) && businessProfile.CompanyRegisteredCountryCode != null
                && IsCompleted(businessProfile.MailingAddress) && businessProfile.MailingCountryCode != null)
            {
                isCompleted = true;
            }
            return isCompleted;
        }

        public bool CheckIsTCContactPersonCompleted(BusinessProfile businessProfile, PartnerRegistration partnerRegistration)
        {
            var isCompleted = false;

            var isTelephoneCompleted = (businessProfile.TelephoneNumber != null &&
                ContactNumber.Create(businessProfile.TelephoneNumber?.DialCode, businessProfile.TelephoneNumber?.CountryISO2Code, businessProfile.TelephoneNumber?.Value).IsSuccess);
            var isContactCompleted = (businessProfile.ContactNumber != null &&
                ContactNumber.Create(businessProfile.ContactNumber.DialCode, businessProfile.ContactNumber.CountryISO2Code, businessProfile.ContactNumber.Value).IsSuccess);
            var isEmailCompleted = (partnerRegistration.Email != null && Email.Create(partnerRegistration.Email.Value).IsSuccess);

            if (IsCompleted(businessProfile.ContactPersonName) && isTelephoneCompleted && isContactCompleted
                && isEmailCompleted)
            {
                isCompleted = true;
            }

            return isCompleted;
        }

        private bool IsCompleted(string value)
        {
            return !String.IsNullOrWhiteSpace(value);
        }

        private bool IsServiceOfferedCompleted(BusinessProfile businessProfile)
        {
            if ((businessProfile.IsMoneyTransferRemittance ?? false) || (businessProfile.IsForeignCurrencyExchange ?? false)
                || (businessProfile.IsRetailCommercialBankingServices ?? false) || (businessProfile.IsForexTrading ?? false)
                || (businessProfile.IsEMoneyEwallet ?? false) || (businessProfile.IsIntermediataryRemittance ?? false)
                || (businessProfile.IsCryptocurrency ?? false) || (businessProfile.IsOther ?? false && IsCompleted(businessProfile.OtherReason)))
            {
                return true;
            }
            return false;
        }

        #endregion Check is Mandatory in TC Portal


        private async Task<bool> IsLicenseInfoCompleted(int businessProfileCode)
        {

            var license = await GetLicenseInfoByBusinessCode(businessProfileCode);
            if (license != null)
            {
                if (license.IsLicenseRequired.HasValue && (license.IsLicenseRequired.Value == false
                    || (license.IsLicenseRequired.Value == true &&
                        !string.IsNullOrEmpty(license.LicenseCertNumber) &&
                        !string.IsNullOrEmpty(license.LicenseType) &&
                        !string.IsNullOrEmpty(license.PrimaryRegulatorLicenseService) &&
                        !string.IsNullOrEmpty(license.PrimaryRegulatorAMLCFT) &&
                        !string.IsNullOrEmpty(license.ActLawRemittanceLicense) &&
                        !string.IsNullOrEmpty(license.ActLawAMLCFT) &&
                        license.IssuedDate.HasValue && license.IssuedDate != DateTime.MinValue))
                     )
                {
                    return true;
                }
            }


            return false;
        }

        private async Task<bool> IsCoInformationCompleted(int businessProfileCode)
        {
            var isValid = false;
            var _coInfo = await GetCOInfoByBusinessCode(businessProfileCode);
            if (_coInfo != null)
            {
                var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                var isContactValid = isTCRevampFeature == true || (isTCRevampFeature == false && _coInfo.ContactNumber != null &&
                    ContactNumber.Create(_coInfo.ContactNumber.DialCode, _coInfo.ContactNumber.CountryISO2Code, _coInfo.ContactNumber.Value).IsSuccess);

                if (!string.IsNullOrEmpty(_coInfo.ComplianceOfficer) &&
                    !string.IsNullOrEmpty(_coInfo.PositionTitle) &&
                    (_coInfo.EmailAddress != null &&
                    Email.Create(_coInfo.EmailAddress.Value).IsSuccess) &&
                    isContactValid)
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        private class Questionnaires
        {
            public int QuestionnaireCode { get; set; }
            public string QuestionnaireDescription { get; set; }
        }

        private async Task<bool> IsAmlCFTCompleted(int businessProfileCode, Solution solution)
        {
            bool isAMLCFTComplete = false;
            var businessProfileResult = this.GetBusinessProfileByBusinessProfileCodeAsync(businessProfileCode);
            if (businessProfileResult.Result.IsSuccess)
            {
                var businessProfile = businessProfileResult.Result.Value;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                IEnumerable<Questionnaires> result;
                List<Questionnaire> questionnairesByBusinessProfile = new List<Questionnaire>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "GetAMLCFTQuestionnairesByBusinessProfile",
                        new
                        {
                            businessProfileCode = businessProfileCode,
                            solutionCode = solution.Id
                        },
                        null, null, CommandType.StoredProcedure);

                    result = await reader.ReadAsync<Questionnaires>();

                    foreach (var i in result)
                    {
                        questionnairesByBusinessProfile.Add(await Repository.GetQuestionnaireByCodeAsync(i.QuestionnaireCode));
                    }
                }

                var questionnairesByAMLCFTQuestionnaire = await Repository.GetQuestionnairesByAMLCFTQuestionnairesAsync(businessProfileCode);

                //1. Get from AMLCFT Documentation based on the business profile
                bool isExistUploadedAMLDocumentaton = await CheckHasUploadedAMLDocumentation(businessProfileCode, solution);

                if (isExistUploadedAMLDocumentaton)
                {
                    return true;
                }

                // Production Bug Fix #44698
                foreach (var questionnaire in questionnairesByBusinessProfile)
                {
                    bool hasRecord = questionnairesByAMLCFTQuestionnaire.Contains(questionnaire);

                    if (!hasRecord)
                    {
                        return false;
                    }
                }

                var aMLCFTQuestionnaires = await Repository.GetAMLCFTQuestionnairesByBusinessProfileAsync(businessProfile);
                foreach (var aMLCFTQuestionnaire in aMLCFTQuestionnaires)
                {
                    if (aMLCFTQuestionnaire.Question?.QuestionInputType != QuestionInputType.SubQuestion && aMLCFTQuestionnaire.Question?.IsOptional != true)
                    {
                        var aMLCFTQuestionnaireAnswers = await Repository.GetAMLCFTQuestionnaireAnswersByQuestionnaireAsync(aMLCFTQuestionnaire);

                        if (aMLCFTQuestionnaireAnswers.Count() == 0)
                        {
                            return false;
                        }
                    }
                    isAMLCFTComplete = true;
                }
                return isAMLCFTComplete;
            }
            return false;
        }

        private async Task<bool> IsDeclarationInfoCompleted(int businessProfileCode)
        {
            var declaration = await GetKYCDeclarationInfoAsync(businessProfileCode);

            if (declaration == null) { return false; }

            if (declaration.IsShowOldUI)
            {
                if ((declaration.IsAgreedTermsOfService.HasValue && declaration.IsAgreedTermsOfService == true) &&
                    !string.IsNullOrEmpty(declaration.SigneeName) &&
                    !string.IsNullOrEmpty(declaration.Designation) &&
                    (declaration.IsAuthorized.HasValue && declaration.IsAuthorized == true) &&
                    (declaration.IsDeclareTransactionTax.HasValue && declaration.IsDeclareTransactionTax == true) &&
                    (declaration.IsInformationTrue.HasValue && declaration.IsInformationTrue == true) &&
                    (declaration.DocumentId != null && declaration.DocumentId != Guid.Empty))
                {
                    return true;
                }
            }
            else
            {
                if (declaration?.IsAllApplicationAccurate ?? false)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
        public async Task<KYCSubModuleReview> UpdateKYCSubModuleUserUpdatedDate(BusinessProfile businessProfile, KYCCategory kYCCategory)
        {
            var kycSubModuleReview = await this.Repository.GetKYCSubModuleReviewByBusinessProfileCategory(businessProfile, kYCCategory);

            if (kycSubModuleReview != null)
            {
                kycSubModuleReview.UpdateUserUpdatedDate();
                return await Repository.SaveKYCSubModuleReview(kycSubModuleReview);
            }

            return await Task.FromResult(kycSubModuleReview);
        }

        public async Task<KYCSubModuleReview> UpdateKYCSubModuleReviewResult(BusinessProfile businessProfile, KYCCategory kYCCategory, ReviewResult reviewResult)
        {
            var kycSubModuleReview = await this.Repository.GetKYCSubModuleReviewByBusinessProfileCategory(businessProfile, kYCCategory);

            if (kycSubModuleReview != null)
            {
                kycSubModuleReview.AssignReviewResult(reviewResult);
                return await Repository.SaveKYCSubModuleReview(kycSubModuleReview);
            }

            return await Task.FromResult(kycSubModuleReview);

        }

        public async Task UpdateReviewResultIfMandatoryNotFilled(BusinessProfile businessProfile, KYCCategory kYCCategory)
        {
            bool _isMandatoryCompleted = false;
            if (kYCCategory == KYCCategory.Connect_BusinessProfile)
            {
                var businessProfileSummary = await IsBusinessProfileCompleted(businessProfile.Id);
                _isMandatoryCompleted = businessProfileSummary.IsAllInfoCompleted();
            }
            else if (kYCCategory == KYCCategory.Connect_LicenseInfo)
            {
                _isMandatoryCompleted = await IsLicenseInfoCompleted(businessProfile.Id);
            }
            else if (kYCCategory == KYCCategory.Connect_Ownership)
            {
                var ownershipSummary = await IsOwnershipInfoCompleted(businessProfile.Id);
                _isMandatoryCompleted = ownershipSummary.IsAllInfoCompleted();
            }
            else if (kYCCategory == KYCCategory.Connect_Documentation)
            {
                _isMandatoryCompleted = await IsDocumentationCompleted(businessProfile.Id);
            }
            else if (kYCCategory == KYCCategory.Connect_ComplianceInfo)
            {
                _isMandatoryCompleted = await IsCoInformationCompleted(businessProfile.Id);
            }
            else if (kYCCategory == KYCCategory.Connect_AMLOrCFT)
            {
                _isMandatoryCompleted = await IsAmlCFTCompleted(businessProfile.Id, businessProfile.Solution);
            }
            else if (kYCCategory == KYCCategory.Connect_Declaration)
            {
                _isMandatoryCompleted = await IsDeclarationInfoCompleted(businessProfile.Id);
            }

            if (!_isMandatoryCompleted)
            {
                await UpdateKYCSubModuleReviewResult(businessProfile, kYCCategory, ReviewResult.Insufficient_Incomplete);
            }

        }

        public async Task<bool> SaveKYCSubModuleReviewList(BusinessProfile businessProfile, List<KYCSubModuleReview> KYCSubModuleReviewList)
        {
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            return await _businessProfileRepository.SaveKYCSubModuleReviewList(businessProfile, KYCSubModuleReviewList);
        }

        public async Task<Result<KYCSummaryFeedback>> SaveKYCSummaryFeedback(KYCSummaryFeedback kycSummaryFeedback)
        {
            return await _businessProfileRepository.SaveKYCSummaryFeedback(kycSummaryFeedback);
        }

        public async Task<DocumentCommentUploadBP> GetReviewRemarkByCommentCode(long categoryBPCode)
        {
            return await _businessProfileRepository.GetReviewRemarkByCommentCode(categoryBPCode);
        }

        public async Task<DocumentCommentUploadBP> AddDocumentCommentUploadBP(DocumentCommentUploadBP documentCommentUploadBP)
        {
            return await _businessProfileRepository.AddDocumentCommentUploadBP(documentCommentUploadBP);
        }

        public async Task<Questionnaire> GetQuestionnaireByCode(long questionnaireCode)
        {
            return await _businessProfileRepository.GetQuestionnaireByCodeAsync(questionnaireCode);
        }

        public async Task<Result<BusinessProfile>> EnsureCollectionTierOnTransactionEvaluation(BusinessProfile businessProfile, bool isQuestionAnswered)
        {
            if (businessProfile.CollectionTier != CollectionTier.Tier_3)
            {
                businessProfile.SetCollectionTierOnTransactionEvaluation(isQuestionAnswered);
                var updatePartnerBP = await Repository.UpdateBusinessProfileAsync(businessProfile);
            }
            return Result.Success(businessProfile);
        }

        public async Task<Result<BusinessProfile>> EnsureCollectionTierOnDocumentation(BusinessProfile businessProfile, bool isDocumentationAttached)
        {
            businessProfile.SetCollectionTierOnDocumentation(isDocumentationAttached);
            var updatePartnerBP = await Repository.UpdateBusinessProfileAsync(businessProfile);

            return Result.Success(businessProfile);
        }

        public async Task<Result<bool?>> ChangeCustomerTypeHandling(int businessProfileCode, long newCustomerTypeCode, long? currentCustomerTypeCode, int? adminUserID)
        {
            bool? redoBusinessDeclaration = null;

            try
            {
                var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
                var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
                var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistration.Id);
                var currentCustomerType = await _businessProfileRepository.GetCustomerTypeByCode(currentCustomerTypeCode);
                var newCustomerType = await _businessProfileRepository.GetCustomerTypeByCode(newCustomerTypeCode);

                if (newCustomerType == currentCustomerType) // Handle save without changing Customer Type
                {
                    return Result.Success(redoBusinessDeclaration);
                }
                else
                {
                    var businessSubscriptions = partnerSubscriptions.Where(x => x.Solution == Solution.Business).ToList();
                    bool isTBSubscriptionsGoLive = businessSubscriptions.Any(x => x.Environment == Entities.Environment.Production);

                    redoBusinessDeclaration =
                        ((currentCustomerType == CustomerType.Individual && newCustomerType != CustomerType.Individual) ||
                        (currentCustomerType != CustomerType.Individual && newCustomerType == CustomerType.Individual))
                        && !isTBSubscriptionsGoLive; // Partners that have gone live don't need to redo declaration

                    bool deleteOwnership =
                        ((currentCustomerType != CustomerType.Corporate_Normal_Corporate) && (currentCustomerType != CustomerType.Corporate_Mass_Payout)) &&
                        ((newCustomerType == CustomerType.Corporate_Normal_Corporate) || (newCustomerType == CustomerType.Corporate_Mass_Payout));

                    // 1) Redo business declaration
                    if (redoBusinessDeclaration is true)
                    {
                        // Delete current customerBusinessDeclarationAnswers for previous CustomerType
                        var customerBusinessDeclaration = await _businessProfileRepository.GetCustomerBusinessDeclarationByBusinessProfileCode(businessProfileCode);
                        var currentCustomerBusinessDeclarationAnswers = await _businessProfileRepository.GetCustomerBusinessDeclarationAnswersByCodeAsync(customerBusinessDeclaration.Id);
                        await _businessProfileRepository.DeleteCustomerBusinessDeclarationAnswersAsync(currentCustomerBusinessDeclarationAnswers);
                        var pending = await _businessProfileRepository.GetBusinessDeclarationStatus(BusinessDeclarationStatus.Pending.Id);

                        // Update CustomerBusinessDeclaration
                        customerBusinessDeclaration.BusinessDeclarationStatus = pending;
                        customerBusinessDeclaration.RedoCount = 3;
                        customerBusinessDeclaration.IsRedoBusinessDeclaration = true;
                        await _businessProfileRepository.UpdateCustomerBusinessDeclarationAsync(customerBusinessDeclaration);

                        // Add CustomerBusinessDeclarationAnswers for new CustomerType
                        var declarationQuestions = await _businessProfileRepository.GetDeclarationQuestionsByCustomerTypeAsync(newCustomerType.Id);
                        var customerBusinessDeclarationAnswers = new List<CustomerBusinessDeclarationAnswer>();
                        foreach (var d in declarationQuestions)
                        {
                            var customerBusinessDeclarationAnswer = new CustomerBusinessDeclarationAnswer(customerBusinessDeclaration.Id, d.Id);
                            customerBusinessDeclarationAnswers.Add(customerBusinessDeclarationAnswer);
                        }

                        customerBusinessDeclarationAnswers.Reverse();
                        await _businessProfileRepository.AddCustomerBusinessDeclarationAnswersAsync(customerBusinessDeclarationAnswers);
                    }
                    else
                    {
                        redoBusinessDeclaration = false; // Set false to indicate Non Redo Business Declaration
                    }

                    // 2) License Info
                    var licenseInformation = await _businessProfileRepository.GetLicenseInfoByBusinessCode(businessProfileCode);

                    // - move documentID to backup tbl
                    var changeCustomerTypeLicenseInfo = new ChangeCustomerTypeLicenseInformation(businessProfileCode, adminUserID.Value, licenseInformation?.RegulatorDocumentId);
                    changeCustomerTypeLicenseInfo = await _businessProfileRepository.AddChangeCustomerTypeLicenseInformation(changeCustomerTypeLicenseInfo);
                    // - delete documentID from main tbl
                    if (licenseInformation != null)
                    {
                        licenseInformation.RegulatorDocumentId = null;
                        licenseInformation.RegulatorDocumentName = null;
                    }

                    // 3) Co Info
                    var coInformation = await _businessProfileRepository.GetCOInfoByBusinessCode(businessProfileCode);

                    // - move documentID to backup tbl
                    var changeCustomerTypeCOInfo = new ChangeCustomerTypeCOInformation(businessProfileCode, adminUserID.Value, coInformation?.CoSignatureDocumentId);
                    changeCustomerTypeCOInfo = await _businessProfileRepository.AddChangeCustomerTypeCOInformation(changeCustomerTypeCOInfo);
                    // - delete documentID from main tbl
                    if (coInformation != null)
                    {
                        coInformation.CoSignatureDocumentId = Guid.Empty;
                        coInformation.COSignatureDocumentName = null;
                    }

                    // 4) Documentation
                    try
                    {
                        var documentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAsync(businessProfileCode);
                        if (documentCategoryBPs != null)
                        {
                            var documentUploadBPs = new List<DocumentUploadBP>();
                            var changeCustomerTypeDocumentUploadBPs = new List<ChangeCustomerTypeDocumentUploadBP>();
                            foreach (var d in documentCategoryBPs)
                            {
                                var documentUploadBP = await _businessProfileRepository.GetDocumentUploadBPsAsync(d.Id);
                                documentUploadBPs.AddRange(documentUploadBP);

                                foreach (var du in documentUploadBP)
                                {
                                    var changeCustomerTypeDocumentUploadBP = new ChangeCustomerTypeDocumentUploadBP(businessProfileCode, d.Id, adminUserID, du.DocumentId);
                                    changeCustomerTypeDocumentUploadBPs.Add(changeCustomerTypeDocumentUploadBP);
                                }
                            }
                            // - move documentID to backup tbl
                            changeCustomerTypeDocumentUploadBPs = await _businessProfileRepository.AddChangeCustomerTypeDocumentUploadBPs(changeCustomerTypeDocumentUploadBPs);

                            // - delete documentID from main tbl
                            await _businessProfileRepository.DeleteDocumentUploadBPs(documentUploadBPs);
                            await _businessProfileRepository.DeleteDocumentCategoryBPs(documentCategoryBPs);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Result.Failure<bool?>(ex.Message);
                    }

                    // 5) Ownership
                    if (deleteOwnership)
                    {
                        // Ultimate beneficial owner
                        var legalEntityList = await _businessProfileRepository.GetLegalEntityListAsync(businessProfileCode);
                        await _businessProfileRepository.DeleteLegalEntityAsync(legalEntityList, CancellationToken.None);

                        // Parent Holding Company
                        var parentHoldingCompanyList = await _businessProfileRepository.GetParentHoldingCompanyListAsync(businessProfileCode);
                        await _businessProfileRepository.DeleteParentHoldingCompanyAsync(parentHoldingCompanyList, CancellationToken.None);

                        // Primary Officers
                        var primaryOfficersList = await _businessProfileRepository.GetPrimaryOfficerListAsync(businessProfileCode);
                        foreach (var primaryOfficer in primaryOfficersList)
                        {
                            await _businessProfileRepository.UpdateShareholdersPrimaryOfficerReferenceAsync(primaryOfficer.Id);
                        }

                        await _businessProfileRepository.DeletePrimaryOfficerAsync(primaryOfficersList, CancellationToken.None);

                        // Affiliate and Subsidiaries
                        var affiliateAndSubsidiariesList = await _businessProfileRepository.GetAffiliateAndSubsidiaryListAsync(businessProfileCode);
                        await _businessProfileRepository.DeleteAffiliateAndSubsidiaryAsync(affiliateAndSubsidiariesList, CancellationToken.None);
                    }

                    // 6) KYC Submodule Review
                    try
                    {
                        var KYCSubmoduleReviewList = await _businessProfileRepository.GetKYCSubModuleReviewList(businessProfileCode);

                        // Delete for current Customer Type
                        var result = await _businessProfileRepository.DeleteKYCSubModuleReview(KYCSubmoduleReviewList, businessProfileCode);

                        if (result.Count == 0)
                        {
                            // Add for new Customer Type
                            // Different Customer Types have different KYC Categories
                            var kycBusinessCategories = await _businessProfileRepository.GetKYCCategoriesByCustomerTypeGroupCodeAsync(newCustomerType.CustomerTypeGroupCode);
                            var kYCSubModules = new List<KYCSubModuleReview>();
                            var insufficientIncomplete = await _businessProfileRepository.GetReviewResultAsync(ReviewResult.Insufficient_Incomplete.Id);

                            foreach (var kYCCategory in kycBusinessCategories)
                            {
                                KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, insufficientIncomplete);
                                kYCSubModules.Add(kYCSubModuleReview);
                            }

                            if (partnerSubscriptions.Any(x => x.Solution == Solution.Business && x.TrangloEntity == TrangloEntity.TSB.Name) && (newCustomerType.CustomerTypeGroupCode != CustomerType.Individual.CustomerTypeGroupCode))
                            {
                                var kycCategories = await _businessProfileRepository.GetKYCBusinessCategories();
                                var verificationCategory = kycCategories.Find(x => x.Id == KYCCategory.Business_Verification.Id);
                                KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, verificationCategory, insufficientIncomplete);
                                kYCSubModules.Add(kYCSubModuleReview);
                            }
                            await _businessProfileRepository.AddKYCSubmoduleReviews(kYCSubModules);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Result.Failure<bool?>(ex.Message);
                    }

                    // RBA
                    #region RBA
                    //var screeningInputs = await businessProfileRepository.GetScreeningInputsByBusinessProfileIdAsync(businessProfileCode);

                    //if (screeningInputs != null && screeningInputs.Count() > 0)
                    //{
                    //    await _rbaService.ProcessRiskEvaluationsWithWatchListAsync(screeningInputs.FirstOrDefault());
                    //}
                    #endregion RBA


                    return Result.Success(redoBusinessDeclaration);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure<bool?>(ex.Message);
            }
        }

        public async Task<Result> ChangeTrangloEntityHandling(int businessProfileCode, string trangloEntity, long? customerType)
        {
            var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistration.Id);
            var solution = partnerSubscriptions.Any(x => x.Solution == Solution.Business);
            var currentCustomerType = await _businessProfileRepository.GetCustomerTypeByCode(customerType);

            try
            {
                var KYCSubmoduleReviewList = await _businessProfileRepository.GetKYCSubModuleReviewList(businessProfileCode);

                // Add for new Customer Type
                // Different Customer Types have different KYC Categories
                var kycBusinessCategories = await _businessProfileRepository.GetKYCCategoriesByCustomerTypeGroupCodeAsync(currentCustomerType.CustomerTypeGroupCode);
                var kYCSubModules = new List<KYCSubModuleReview>();
                var insufficientIncomplete = await _businessProfileRepository.GetReviewResultAsync(ReviewResult.Insufficient_Incomplete.Id);

                if (trangloEntity == TrangloEntity.TSB.TrangloEntityCode && currentCustomerType.CustomerTypeGroupCode != CustomerType.Individual.CustomerTypeGroupCode)
                {
                    var kycCategories = await _businessProfileRepository.GetKYCBusinessCategories();
                    var verificationCategory = kycCategories.Find(x => x.Id == KYCCategory.Business_Verification.Id);
                    KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, verificationCategory, insufficientIncomplete);
                    kYCSubModules.Add(kYCSubModuleReview);
                }
                await _businessProfileRepository.AddKYCSubmoduleReviews(kYCSubModules);

            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }

            return Result.Success();
        }

        public async Task<Result> AddKYCSubModuleForTCMSB(int businessProfileCode, long? customerType)
        {
            var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistration.Id);
            var solution = partnerSubscriptions.Any(x => x.Solution == Solution.Connect);
            var currentCustomerType = await _businessProfileRepository.GetCustomerTypeByCode(customerType);

            List<KYCSubModuleReview> kYCSubModules = new List<KYCSubModuleReview>();
            try
            {
                if (solution)
                {
                    var kycConnectCategories = await Repository.GetKYCConnectCategories();

                    foreach (var kYCCategory in kycConnectCategories)
                    {
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, ReviewResult.Insufficient_Incomplete);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }

                    await Repository.AddKYCSubmoduleReviews(kYCSubModules);
                }

            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }

            return Result.Success();
        }

        public async Task<Result> AddCustomerBusinessDeclaration(int businessProfileCode)
        {
            var customerBusinessDeclarationProfile = await _businessProfileRepository.GetCustomerBusinessDeclarationByBusinessProfileCode(businessProfileCode);
            var partnerProfile = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(businessProfileCode);

            try
            {
                if (customerBusinessDeclarationProfile is null)
                {
                    if (partnerProfile.CustomerType.Id != null)
                    {

                        var customerBusinessDeclaration = new CustomerBusinessDeclaration(businessProfileCode);
                        customerBusinessDeclaration = await _businessProfileRepository.AddCustomerBusinessDeclarationAsync(customerBusinessDeclaration);

                        var customerBusinessDeclarationAnswers = new List<CustomerBusinessDeclarationAnswer>();
                        var declarationQuestions = await _businessProfileRepository.GetDeclarationQuestionsByCustomerTypeAsync(partnerProfile.CustomerType.Id);

                        foreach (var d in declarationQuestions)
                        {
                            var customerBusinessDeclarationAnswer = new CustomerBusinessDeclarationAnswer(customerBusinessDeclaration.Id, d.Id);
                            customerBusinessDeclarationAnswers.Add(customerBusinessDeclarationAnswer);
                        }

                        customerBusinessDeclarationAnswers.Reverse();
                        await _businessProfileRepository.AddCustomerBusinessDeclarationAnswersAsync(customerBusinessDeclarationAnswers);
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }

            return Result.Success();
        }

        public async Task<Result> AddKYCSubModuleForTBMSB(int businessProfileCode, long? customerType, string trangloEntity)
        {
            var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfileCode);
            var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistration.Id);
            var isBusinesSolution = partnerSubscriptions.Any(x => x.Solution == Solution.Business);
            var currentCustomerType = await _businessProfileRepository.GetCustomerTypeByCode(customerType);
            var insufficientReviewResult = await _businessProfileRepository.GetReviewResultAsync(ReviewResult.Insufficient_Incomplete.Id);
            List<KYCSubModuleReview> kYCSubModules = new List<KYCSubModuleReview>();
            try
            {
                if (isBusinesSolution)
                {
                    var kycBusinesCategories = await _businessProfileRepository.GetKYCCategoriesByCustomerTypeGroupCodeAsync(currentCustomerType.CustomerTypeGroupCode);

                    foreach (var kYCCategory in kycBusinesCategories)
                    {
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, kYCCategory, insufficientReviewResult);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }

                    if (trangloEntity == TrangloEntity.TSB.TrangloEntityCode && currentCustomerType.CustomerTypeGroupCode != CustomerType.Individual.CustomerTypeGroupCode)
                    {
                        var kycCategories = await _businessProfileRepository.GetKYCBusinessCategories();
                        var verificationCategory = kycCategories.Find(x => x.Id == KYCCategory.Business_Verification.Id);
                        KYCSubModuleReview kYCSubModuleReview = new KYCSubModuleReview(businessProfile, verificationCategory, insufficientReviewResult);
                        kYCSubModules.Add(kYCSubModuleReview);
                    }

                    await Repository.AddKYCSubmoduleReviews(kYCSubModules);


                    // Add Business Declaration records
                    if (customerType != null)
                    {
                        var customerBusinessDeclaration = new CustomerBusinessDeclaration(businessProfile.Id);
                        customerBusinessDeclaration = await _businessProfileRepository.AddCustomerBusinessDeclarationAsync(customerBusinessDeclaration);

                        var customerBusinessDeclarationAnswers = new List<CustomerBusinessDeclarationAnswer>();
                        var declarationQuestions = await _businessProfileRepository.GetDeclarationQuestionsByCustomerTypeAsync(currentCustomerType.Id);

                        foreach (var d in declarationQuestions)
                        {
                            var customerBusinessDeclarationAnswer = new CustomerBusinessDeclarationAnswer(customerBusinessDeclaration.Id, d.Id);
                            customerBusinessDeclarationAnswers.Add(customerBusinessDeclarationAnswer);
                        }

                        customerBusinessDeclarationAnswers.Reverse();
                        await _businessProfileRepository.AddCustomerBusinessDeclarationAnswersAsync(customerBusinessDeclarationAnswers);
                    }

                    //1) Documentation Comment : TCxTB Mapping for Remittance Partner
                    if (customerType == CustomerType.Remittance_Partner.Id)
                    {
                        var documentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAsync(businessProfileCode);
                        if (documentCategoryBPs != null)
                        {

                            foreach (var documentCategory in documentCategoryBPs)
                            {
                                if (documentCategory.DocumentCategoryCode == 1) //Valid Business Registration/Certificate of Incorporation
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 48; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 48);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {

                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 3)//Articles of Incorporation / Memorandum of Association
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 49; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 49);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 4)//Ownership structure chart up to UBO level(include percentage) 
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 50; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 50);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 5)//A list of all company directors showing full name,date and place of birth, nationality & country of residence 
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 51; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 51);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 6)//Copies of passport or other identification documents of directors, shareholders (25% and above) and authorized persons 
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 52; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 52);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 7)//IF the major shareholders (25% and above) is a company, please provide evidence of the beneficial owner (25%) of that holding company - full name, date of birth and nationality 
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 53; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 53);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 8)//Copy of Regulatory Approval / License to engage in Remittance / Money Transfer Business
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 54; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 54);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 9)//Last 3 years audited financial statements
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 55; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 55);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 10)//Photographs of the outlet consisting of 2 angles, i.e exterior (with the name of the entity at the entrance) & interior, along with the proof of address, i.e utility bill / tenancy agreement
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 56; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 56);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 11)//Compliance policies for Anti-money laundering and counter terrorism financing
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 57; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 57);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 12)//Certified True Copy on Board Resolution of Authorised Representative 
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 58; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 58);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 13)//Complete AML/CFT and KYC Questionnaires for Remittance Tie-Up
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 59; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 59);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                                else if (documentCategory.DocumentCategoryCode == 15)//Other Documents
                                {
                                    List<DocumentCategoryBP> documents = new List<DocumentCategoryBP>();
                                    var documentCategoryBP = new DocumentCategoryBP();
                                    documentCategoryBP.BusinessProfileCode = documentCategory.BusinessProfileCode;
                                    documentCategoryBP.DocumentCategoryCode = 61; // documentCategoryGroupCode = 4 : Remittance
                                    documentCategoryBP.DocumentCategoryBPStatusCode = documentCategory.DocumentCategoryBPStatusCode;

                                    documents.Add(documentCategoryBP);

                                    await _businessProfileRepository.AddDocumentCategoryBPs(documents);

                                    var recentDocumentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPsByBusinessProfileCodeAndDocumentCategoryCodeAsync(businessProfileCode, 61);
                                    var documentCommentBPs = await _businessProfileRepository.GetDocumentCommentBPsByDocumentCategoryBPCodeAsync(documentCategory.Id);
                                    List<DocumentCommentBP> documentComments = new List<DocumentCommentBP>();
                                    foreach (var documentComment in documentCommentBPs)
                                    {
                                        var documentCommentBP = new DocumentCommentBP();
                                        documentCommentBP.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                        documentCommentBP.Comment = documentComment.Comment;
                                        documentCommentBP.IsExternal = documentComment.IsExternal;

                                        documentComments.Add(documentCommentBP);
                                    }

                                    await _businessProfileRepository.AddDocumentCommentBP(documentComments);

                                    var existingDocumentUploads = await _businessProfileRepository.GetDocumentUploadBPsAsync(documentCategory.Id);
                                    if (existingDocumentUploads != null)
                                    {
                                        List<DocumentUploadBP> documentUploadBPs = new List<DocumentUploadBP>();
                                        foreach (var documentUpload in existingDocumentUploads)
                                        {
                                            var newDocumentUpload = new DocumentUploadBP();
                                            newDocumentUpload.DocumentCategoryBPCode = recentDocumentCategoryBPs.Id;
                                            newDocumentUpload.DocumentId = documentUpload.DocumentId;

                                            documentUploadBPs.Add(newDocumentUpload);
                                        }
                                        await _businessProfileRepository.AddListDocumentUploadAsync(documentUploadBPs);
                                    }
                                }

                            }
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }

            return Result.Success();
        }

        public async Task<Result<KYCSummaryFeedbackNotification>> InsertKYCSummaryFeedbackNotificationAsync(KYCSummaryFeedbackNotification notification, CancellationToken cancellationToken)
        {
            return await _businessProfileRepository.InsertKYCSummaryFeedbackNotificationAsync(notification, cancellationToken);
        }

        public async Task<List<PartnerSubscription>> GetPartnerSubscriptionByUserIdAsync(int userId)
        {
            var customerUserBusinessProfileSpec = Specification<CustomerUserBusinessProfile>.All;
            var byUserID = new CustomerUserBusinessProfileByUserID(userId);
            customerUserBusinessProfileSpec = customerUserBusinessProfileSpec.And(byUserID);

            var customerUserBusinessProfileList = (await Repository.GetCustomerUserBusinessProfilesAsync(customerUserBusinessProfileSpec));
            var businessProfileIds = customerUserBusinessProfileList.Select(x => x.BusinessProfileCode)
                .ToHashSet();

            return await _partnerRepository.GetPartnerSubscriptionsByBusinessProfileIdsAsync(businessProfileIds);
        }
    }
}

