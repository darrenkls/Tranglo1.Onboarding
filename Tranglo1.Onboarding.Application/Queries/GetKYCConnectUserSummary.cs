using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Domain.Entities;
using System.Linq;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetKYCConnectUserSummaryQuery : BaseQuery<Result<List<GetKYCConnectUserSummaryOutputDTO>>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<List<GetKYCConnectUserSummaryOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Connect User Summary for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetKYCConnectUserSummaryQueryHandler : IRequestHandler<GetKYCConnectUserSummaryQuery, Result<List<GetKYCConnectUserSummaryOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;

        public GetKYCConnectUserSummaryQueryHandler(
            BusinessProfileService businessProfileService,
            IBusinessProfileRepository repository)
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
        }

        public async Task<Result<List<GetKYCConnectUserSummaryOutputDTO>>> Handle(GetKYCConnectUserSummaryQuery request, CancellationToken cancellationToken)
        {
            var summary = new List<GetKYCConnectUserSummaryOutputDTO>();
            var validationResult = await _businessProfileService.IsMandatoryFieldCompletedAsync(request.BusinessProfileCode, Solution.Connect);

            if (validationResult != null)
            {
                //Category Should be Connect Only
                var category = await _repository.GetKYCConnectCategories();

                category = category.Where(o => o.Id != KYCCategory.Connect_Declaration.Id).ToList();

                foreach (var item in category)
                {
                    bool Status = false;
                    var details = new GetKYCConnectUserSummaryOutputDTO()
                    {
                        CategoryCode = item.Id,
                        Category = item.Name,
                        CategoryDisplay = item.PortalDisplayName
                    };

                    if (item == KYCCategory.Connect_BusinessProfile)
                    {
                        Status = validationResult.isBusinessProfileCompleted;

                        var companyDetailStatus = KYCProgressStatus.GetStatus(validationResult.BusinessProfileSummary.IsCompanyDetailsCompleted);
                        var addressStatus = KYCProgressStatus.GetStatus(validationResult.BusinessProfileSummary.IsAddressCompleted);
                        var contactPersonStatus = KYCProgressStatus.GetStatus(validationResult.BusinessProfileSummary.IsContactPersonCompleted);
                        details.SubCategories = new List<KYCConnectSubCategory>()
                        {
                            new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Profile_CompanyDetails.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Profile_CompanyDetails.Name,
                                StatusDesc = companyDetailStatus.Name,
                                Status = companyDetailStatus.Id,
                            },
                            new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Profile_Address.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Profile_Address.Name,
                                StatusDesc = addressStatus.Name,
                                Status = addressStatus.Id,
                            },
                            new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Profile_ContactPerson.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Profile_ContactPerson.Name,
                                StatusDesc = contactPersonStatus.Name,
                                Status = contactPersonStatus.Id,
                            }
                        };

                    }
                    else if (item == KYCCategory.Connect_LicenseInfo)
                    {
                        Status = validationResult.isLicenseInfoCompleted;
                    }
                    else if (item == KYCCategory.Connect_Ownership)
                    {
                        Status = validationResult.isOwnershipCompleted;

                        var shareholderStatus = KYCProgressStatus.GetStatus(validationResult.OwnershipSummary.IsShareholderCompleted);
                        var ultimateBeneStatus = KYCProgressStatus.GetStatus(validationResult.OwnershipSummary.IsUltimateBeneficialOwnerCompleted);
                        var boardOfDirectorStatus = KYCProgressStatus.GetStatus(validationResult.OwnershipSummary.IsBoardOfDirectorCompleted);
                        var principalOfficerStatus = KYCProgressStatus.GetStatus(validationResult.OwnershipSummary.IsPrincipalOfficerCompleted);
                        var authorisedPersonStatus = KYCProgressStatus.GetStatus(validationResult.OwnershipSummary.IsAuthorisedPersonCompleted);
                        var licensedParentStatus = KYCProgressStatus.GetStatus(validationResult.OwnershipSummary.IsLicensedParentCompanyCompleted);

                        details.SubCategories = new List<KYCConnectSubCategory>()
                        {
                            new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Ownership_Shareholder.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Ownership_Shareholder.Name,
                                StatusDesc = shareholderStatus.Name,
                                Status = shareholderStatus.Id,
                            },
                            new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Ownership_UltimateOwner.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Ownership_UltimateOwner.Name,
                                StatusDesc = ultimateBeneStatus.Name,
                                Status = ultimateBeneStatus.Id,
                            },
                            new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Ownership_BoardOfDirector.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Ownership_BoardOfDirector.Name,
                                StatusDesc = boardOfDirectorStatus.Name,
                                Status = boardOfDirectorStatus.Id,
                            },
                             new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Ownership_PrincipalOfficer.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Ownership_PrincipalOfficer.Name,
                                StatusDesc = principalOfficerStatus.Name,
                                Status = principalOfficerStatus.Id,
                            },
                              new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Ownership_AuthorisedPerson.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Ownership_AuthorisedPerson.Name,
                                StatusDesc = authorisedPersonStatus.Name,
                                Status = authorisedPersonStatus.Id,
                            },
                               new KYCConnectSubCategory()
                            {
                                SubCategoryCode = KYCSubCategory.Connect_Ownership_LicensedParentCompany.Id,
                                SubCategoryDesc = KYCSubCategory.Connect_Ownership_LicensedParentCompany.Name,
                                StatusDesc = licensedParentStatus.Name,
                                Status = licensedParentStatus.Id,
                            },
                        };
                    }
                    else if (item == KYCCategory.Connect_Documentation)
                    {
                        Status = validationResult.isDocumentationCompleted;
                    }
                    else if (item == KYCCategory.Connect_AMLOrCFT)
                    {
                        Status = validationResult.isAMLCompleted;
                    }
                    else if (item == KYCCategory.Connect_ComplianceInfo)
                    {
                        Status = validationResult.isCoInfoCompleted;
                    }
                    //else if (item == KYCCategory.Connect_Declaration)
                    //{
                    //    Status = validationResult.isDeclarationInfoCompleted;
                    //}

                    details.Status = KYCProgressStatus.GetStatus(Status).Id;
                    details.StatusDesc = KYCProgressStatus.GetStatus(Status).Name;

                    summary.Add(details);
                }
            }

            return Result.Success(summary);
        }

    }
}
