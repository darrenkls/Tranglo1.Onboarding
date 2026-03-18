using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetKYCBusinessUserSummaryByCodeQuery : BaseQuery<Result<List<GetBusinessUserKYCProgressOutputDTO>>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public string EntityCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<List<GetBusinessUserKYCProgressOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Summary for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetKYCBusinessUserSummaryByCodeQueryHandler : IRequestHandler<GetKYCBusinessUserSummaryByCodeQuery, Result<List<GetBusinessUserKYCProgressOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly TrangloUserManager _userManager;

        public GetKYCBusinessUserSummaryByCodeQueryHandler(
            BusinessProfileService businessProfileService,
            IBusinessProfileRepository repository,
            IPartnerRepository partnerRepository,
            TrangloUserManager userManager)
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
            _partnerRepository = partnerRepository;
            _userManager = userManager;
        }

        public async Task<Result<List<GetBusinessUserKYCProgressOutputDTO>>> Handle(GetKYCBusinessUserSummaryByCodeQuery request, CancellationToken cancellationToken)
        {
            var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);

            if (businessProfile is null)
            {
                return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>(
                           $"Business Profile {request.BusinessProfileCode} doesn't exist."
                       );
            }

            if (request.AdminSolution == null && request.CustomerSolution == null)
            {
                return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>("Invalid request");
            }

            // This Query is not meant for TC (fired from Customer Portal)
            if (ClaimCode.Connect == request.CustomerSolution)
            {
                return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>(
                    $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                );
            }
            // This query is not meant for TC (fired from Admin Portal)
            if (Solution.Connect.Id == request.AdminSolution)
            {
                return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>(
                    $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                );
            }

            bool isBusinessSolution = ClaimCode.Business == request.CustomerSolution
                                   || Solution.Business.Id == request.AdminSolution;

            if (!isBusinessSolution)
            {
                return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>(
                    $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                );
            }

            var result = await GetKYCBusinessSummary(request);

            if (result.IsFailure)
            {
                string source = ClaimCode.Business == request.CustomerSolution ? "Customer" : "Admin";
                return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>(
                    $"{source} user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                );
            }

            return result;
        }

        private async Task<Result<List<GetBusinessUserKYCProgressOutputDTO>>> GetKYCBusinessSummary(GetKYCBusinessUserSummaryByCodeQuery request)
        {
            var validationResult = await _businessProfileService.IsBusinessCustomerMandatoryFieldCompletedAsync(request.BusinessProfileCode);

            var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (applicationUser is TrangloStaff)
            {
                var result = GetKycCategorySummariesForAdminUser(validationResult, partner, request.EntityCode);

                return Result.Success(result);
            }
            else if (applicationUser is CustomerUser)
            {
                var result = GetSummariesForCustomerUser(validationResult, partner, request.EntityCode);

                return Result.Success(result);
            }

            return Result.Failure<List<GetBusinessUserKYCProgressOutputDTO>>($"Invalid Admin Solution Code {request.AdminSolution}.");
        }

        private static KYCProgressStatus GetKycProgressStatus(KYCBusinessSummary validationResult, PartnerRegistration partner, KYCCategory category)
        {
            // Mandatory check is skipp for the following
            var shouldSkipMandatoryCheck = (partner.CustomerType.IsIndividual() && (category == KYCCategory.Business_TransactionEvaluation || category == KYCCategory.Business_Verification || category == KYCCategory.Business_Documentation)) ||
                (partner.CustomerType.IsCorporate() && (category == KYCCategory.Business_TransactionEvaluation ||
                    category == KYCCategory.Business_Verification)) ||
                (partner.CustomerType.IsCryptocurrency() && (category == KYCCategory.Business_LicenseInfo ||
                    category == KYCCategory.Business_Verification ||
                    category == KYCCategory.Business_Documentation ||
                    category == KYCCategory.Business_AMLOrCFT ||
                    category == KYCCategory.Business_ComplianceInfo)) ||
                (partner.CustomerType.IsRemittancePartner() && (category == KYCCategory.Business_LicenseInfo ||
                    category == KYCCategory.Business_Verification ||
                    category == KYCCategory.Business_Documentation ||
                    category == KYCCategory.Business_AMLOrCFT ||
                    category == KYCCategory.Business_ComplianceInfo));

            if (shouldSkipMandatoryCheck)
            {
                return KYCProgressStatus.GetStatus(true);
            }

            // Check mandatory fields for each category
            var mapping = new Dictionary<KYCCategory, bool>()
            {
                { KYCCategory.Business_BusinessProfile, validationResult.IsBusinessProfileCompleted },
                { KYCCategory.Business_Declaration, validationResult.IsDeclarationInfoCompleted },
                { KYCCategory.Business_Ownership, validationResult.IsOwnershipCompleted },
                { KYCCategory.Business_BusinessDeclaration, validationResult.IsBusinessDeclarationCompleted },
                { KYCCategory.Business_Documentation, validationResult.IsDocumentationCompleted },
                { KYCCategory.Business_LicenseInfo, validationResult.IsLicenseInfoCompleted }
            };

            if (!mapping.ContainsKey(category))
            {
                return KYCProgressStatus.GetStatus(false);
            }

            return KYCProgressStatus.GetStatus(mapping[category]);
        }
        
        #region Admin User
        private static List<GetBusinessUserKYCProgressOutputDTO> GetKycCategorySummariesForAdminUser(KYCBusinessSummary validationResult, PartnerRegistration partner, string entityType)
        {
            var trangloEntity = TrangloEntity.GetByEntityByTrangloId(entityType);
            var allowedMainCategories = KYCCategory.GetBusinessAdminAllowedMainCategories(partner.CustomerType, trangloEntity);

            // Set status for each category based on the customer type
            var result = new List<GetBusinessUserKYCProgressOutputDTO>();
            foreach (var allowedMainCategory in allowedMainCategories)
            {
                var summary = new GetBusinessUserKYCProgressOutputDTO()
                {
                    KYCCategory = allowedMainCategory,
                    KYCProgressStatus = GetKycProgressStatus(validationResult, partner, allowedMainCategory)
                };
                result.Add(summary);
            }

            return result;
        }
        #endregion Admin User

        #region Customer User
        private static List<GetBusinessUserKYCProgressOutputDTO> GetSummariesForCustomerUser(KYCBusinessSummary validationResult, PartnerRegistration partner, string entityType)
        {
            var result = new List<GetBusinessUserKYCProgressOutputDTO>();
            var trangloEntity = TrangloEntity.GetByEntityByTrangloId(entityType);
            // Get allowed main categories based on CustomerType 
            var allowedMainCategories = KYCCategory.GetBusinessCustomerUserAllowedMainCategories(partner.CustomerType, trangloEntity);

            foreach (var allowedMainCategory in allowedMainCategories)
            {
                var summary = new GetBusinessUserKYCProgressOutputDTO()
                {
                    KYCCategory = allowedMainCategory,
                    KYCProgressStatus = GetKycProgressStatus(validationResult, partner, allowedMainCategory),
                    BusinessUserKYCSubItems = GetKycSubCategoryProgressStatusForCustomerUser(validationResult, partner, allowedMainCategory)
                };

                result.Add(summary);
            }

            return result;
        }

        private static List<GetBusinessUserKYCProgressSubMenuOutputDTO> GetKycSubCategoryProgressStatusForCustomerUser(KYCBusinessSummary validationResult, PartnerRegistration partner, KYCCategory kycCategory)
        {
            if (kycCategory == KYCCategory.Business_BusinessProfile)
            {
                return GetBusinessProfileSubcategoriesProgressStatus(validationResult);
            }
            else if (kycCategory == KYCCategory.Business_Ownership)
            {
                return GetOwnershipSubcategoriesProgressStatus(validationResult, partner);
            }

            return new List<GetBusinessUserKYCProgressSubMenuOutputDTO>();
        }

        private static List<GetBusinessUserKYCProgressSubMenuOutputDTO> GetBusinessProfileSubcategoriesProgressStatus(KYCBusinessSummary validationResult)
        {
            var result = new List<GetBusinessUserKYCProgressSubMenuOutputDTO>();

            foreach (var allowedSubcategory in KYCSubCategory.GetBusinessCustomerUserAllowedSubCategoriesForBusinessProfileCategory())
            {
                if (allowedSubcategory == KYCSubCategory.Business_Profile_CompanyDetails)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsCompanyDetailCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Profile_Address)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsAddressCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Profile_CompanyContact)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsCompanyContactCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Profile_ContactPerson)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsContactPersonCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
            }

            return result;
        }

        private static List<GetBusinessUserKYCProgressSubMenuOutputDTO> GetOwnershipSubcategoriesProgressStatus(KYCBusinessSummary validationResult, PartnerRegistration partner)
        {
            var result = new List<GetBusinessUserKYCProgressSubMenuOutputDTO>();

            foreach (var allowedSubcategory in KYCSubCategory.GetBusinessCustomerUserAllowedSubCategoriesForOwnershipCategory(partner.CustomerType))
            {
                if (allowedSubcategory == KYCSubCategory.Business_Ownership_ShareHolder)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsShareholderCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Ownership_BOD)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsBoardOfDirectorCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Ownership_AuthorisedPerson)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsAuthorisedPersonsCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Ownership_UBO)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsUltimateBeneficialOwnerCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
                else if (allowedSubcategory == KYCSubCategory.Business_Ownership_PrincipalOfficer)
                {
                    var subCategoryStatus = new GetBusinessUserKYCProgressSubMenuOutputDTO()
                    {
                        KYCSubCategory = allowedSubcategory,
                        KYCProgressStatus = KYCProgressStatus.GetStatus(validationResult.IsPrincipalOfficerCompleted)
                    };

                    result.Add(subCategoryStatus);
                }
            }

            return result;
        }
        #endregion Customer User
    }
}


