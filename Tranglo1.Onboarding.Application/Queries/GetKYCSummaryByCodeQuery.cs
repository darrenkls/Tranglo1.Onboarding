using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    /*    //[Permission(PermissionGroupCode.KYCSummary, UACAction.View)]
    */
    internal class GetKYCSummaryByCodeQuery : BaseQuery<Result<List<GetKYCProgressOutputDto>>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<List<GetKYCProgressOutputDto>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Summary for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetKYCSummaryByCodeQueryHandler : IRequestHandler<GetKYCSummaryByCodeQuery, Result<List<GetKYCProgressOutputDto>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;

        public GetKYCSummaryByCodeQueryHandler(
            BusinessProfileService businessProfileService,
            IBusinessProfileRepository repository)
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
        }

        public async Task<Result<List<GetKYCProgressOutputDto>>> Handle(GetKYCSummaryByCodeQuery request, CancellationToken cancellationToken)
        {
            var summary = new List<GetKYCProgressOutputDto>();
            var validationResult = await _businessProfileService.IsMandatoryFieldCompletedAsync(request.BusinessProfileCode, Solution.Connect);

            if (validationResult != null)
            {
                //Enum.GetValues(typeof(KYCCategory)).Cast<KYCCategory>().ToList();
                //Category Should be Connect Only
                var category = await _repository.GetKYCConnectCategories();

                //TODO: Temporary pass in Connect Solution for Sprint 4
                var isDocIncludeAML = await _businessProfileService.CheckHasUploadedAMLDocumentation(request.BusinessProfileCode, Solution.Connect);
                if (isDocIncludeAML)
                {
                    category.Remove(KYCCategory.Connect_AMLOrCFT);
                }

                foreach (var item in category)
                {
                    bool Status = false;
                    var details = new GetKYCProgressOutputDto()
                    {
                        CategoryCode = item.Id,
                        Category = item.Name //EnumExtensions.GetDisplayName(item)
                    };

                    if (item == KYCCategory.Connect_BusinessProfile)
                    {
                        Status = validationResult.isBusinessProfileCompleted;
                    }
                    else if (item == KYCCategory.Connect_LicenseInfo)
                    {
                        Status = validationResult.isLicenseInfoCompleted;
                    }
                    else if (item == KYCCategory.Connect_Ownership)
                    {
                        Status = validationResult.isOwnershipCompleted;
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
                    else if (item == KYCCategory.Connect_Declaration)
                    {
                        Status = validationResult.isDeclarationInfoCompleted;
                    }

                    details.Status = Status ? (int)KYCProgressStatus.Completed.Id : (int)KYCProgressStatus.Pending.Id;
                    details.StatusDesc = Status ? KYCProgressStatus.Completed.Name : KYCProgressStatus.Pending.Name;

                    summary.Add(details);
                }
            }

            return Result.Success(summary);
        }
    }
}
