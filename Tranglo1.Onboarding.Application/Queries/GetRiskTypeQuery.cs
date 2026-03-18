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
    public class GetRiskTypeQuery : IRequest<IEnumerable<RiskTypeOutputDTO>>
    {
        public class GetRiskTypeQueryHandler : IRequestHandler<GetRiskTypeQuery, IEnumerable<RiskTypeOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;

            public GetRiskTypeQueryHandler(IBusinessProfileRepository repository)
            {
                _repository = repository;
            }
            public async Task<IEnumerable<RiskTypeOutputDTO>> Handle(GetRiskTypeQuery request, CancellationToken cancellationToken)
            {
                var riskTypes = await _repository.GetAllRiskTypes();

                IEnumerable<RiskTypeOutputDTO> outputDTO = riskTypes.Select(a => new RiskTypeOutputDTO
                {
                    RiskTypeCode = a.Id,
                    RiskTypeDescription = a.Name
                });

                return outputDTO;
            }
        }
    }
}
