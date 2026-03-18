using CSharpFunctionalExtensions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementReviewSummary.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetHasPendingReviewKYCDocumentByBusinessProfileQuery : BaseQuery<Result<bool>>
    {
        public int BusinessProfileCode { get; set; }

        public class GetHasPendingReviewKYCDocumentByBusinessProfileHandler : IRequestHandler<GetHasPendingReviewKYCDocumentByBusinessProfileQuery, Result<bool>>
        {
            private readonly BusinessProfileService _businessProfileService;

            public GetHasPendingReviewKYCDocumentByBusinessProfileHandler(BusinessProfileService businessProfileService)
            {
                _businessProfileService = businessProfileService;
            }

            public async Task<Result<bool>> Handle(GetHasPendingReviewKYCDocumentByBusinessProfileQuery request, CancellationToken cancellationToken)
            {
                var categoryBPList = await _businessProfileService.GetCategoryBPyCategoryCodeAsync(request.BusinessProfileCode);
                if (categoryBPList.IsFailure)
                    return Result.Failure<bool>(categoryBPList.Error);

                return categoryBPList.Value?.Any(x => x.DocumentCategoryBPStatusCode == DocumentCategoryBPStatus.PendingReview.Id) ?? false;
            }
        }
    }
}
