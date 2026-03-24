using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO;
using System.Linq;
using CSharpFunctionalExtensions;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetKYCConnectCategoriesQuery : IRequest<Result<List<KYCConnectCategoriesOutputDTO>>>
    {
        public class GetKYCConnectCategoriesQueryHandler : IRequestHandler<GetKYCConnectCategoriesQuery, Result<List<KYCConnectCategoriesOutputDTO>>>
        {
            public GetKYCConnectCategoriesQueryHandler()
            {

            }

            public async Task<Result<List<KYCConnectCategoriesOutputDTO>>> Handle(GetKYCConnectCategoriesQuery request, CancellationToken cancellationToken)
            {
                var outputDTO = new List<KYCConnectCategoriesOutputDTO>();

                var kycCategories = Enumeration.GetAll<KYCCategory>();

                outputDTO = kycCategories.Where(o => o.SolutionCode == Solution.Connect.Id).Select(o => new KYCConnectCategoriesOutputDTO()
                {
                    KYCCategoryCode = o.Id,
                    Description = o.PortalDisplayName
                }).OrderBy(o => o.KYCCategoryCode).ToList();

                return Result.Success(outputDTO);
            }
        }
    }
}