using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetRiskRatingQuery : IRequest<IEnumerable<RiskRatingOutputDTO>>
    {
        public class GetRiskRatingQueryHandler : IRequestHandler<GetRiskRatingQuery, IEnumerable<RiskRatingOutputDTO>>
        {
            private readonly IRBARepository _repository;

            public GetRiskRatingQueryHandler(IRBARepository repository)
            {
                _repository = repository;
            }
            public async Task<IEnumerable<RiskRatingOutputDTO>> Handle(GetRiskRatingQuery request, CancellationToken cancellationToken)
            {
                var riskTypes = await _repository.GetAllRiskRatings();

                IEnumerable<RiskRatingOutputDTO> outputDTO = riskTypes.Select(a => new RiskRatingOutputDTO
                {
                    RiskRatingCode = a.Id,
                    Description = a.Name
                });

                return outputDTO;
            }
        }
    }
}