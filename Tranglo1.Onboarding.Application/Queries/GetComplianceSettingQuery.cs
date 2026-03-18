using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetComplianceSettingQuery : IRequest<IEnumerable<ComplianceSettingOutputDTO>>
    {
        public class GetComplianceSettingQueryHandler : IRequestHandler<GetComplianceSettingQuery, IEnumerable<ComplianceSettingOutputDTO>>
        {
            private readonly IRBARepository _repository;

            public GetComplianceSettingQueryHandler(IRBARepository repository)
            {
                _repository = repository;
            }
            public async Task<IEnumerable<ComplianceSettingOutputDTO>> Handle(GetComplianceSettingQuery request, CancellationToken cancellationToken)
            {
                var riskTypes = await _repository.GetAllRiskRatings();

                IEnumerable<ComplianceSettingOutputDTO> outputDTO = riskTypes.Select(a => new ComplianceSettingOutputDTO
                {
                    ComplianceSettingCode = a.Id,
                    Description = a.Name
                });

                return outputDTO;
            }
        }
    }
}
