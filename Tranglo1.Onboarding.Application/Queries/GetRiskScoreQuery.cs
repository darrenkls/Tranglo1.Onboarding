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
    public class GetRiskScoreQuery : IRequest<IEnumerable<RiskScoreOutputDTO>>
    {
        public class GetRiskScoreQueryHandler : IRequestHandler<GetRiskScoreQuery, IEnumerable<RiskScoreOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            
            public GetRiskScoreQueryHandler(IBusinessProfileRepository repository)
            {
                _repository = repository;
            }


            public async Task<IEnumerable<RiskScoreOutputDTO>> Handle(GetRiskScoreQuery request, CancellationToken cancellationToken)
            {
                var riskScore = await _repository.GetAllRiskScores();

                IEnumerable<RiskScoreOutputDTO> outputDTO = riskScore.Select(a => new RiskScoreOutputDTO
                {
                    RiskScoreCode = a.Id,
                    RiskScoreDescription = a.Name,
                    HighRange = a.HighRange,
                    LowRange = a.LowRange
                });

                return outputDTO;
            }
        }


    }
}
