using AutoMapper;
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
    public class GetRequisitionTypeQuery : IRequest<IEnumerable<RequisitionTypeOutputDTO>>
    {
        public class GetRequisitionTypeQueryHandler : IRequestHandler<GetRequisitionTypeQuery, IEnumerable<RequisitionTypeOutputDTO>>
        {
            private readonly IRBARepository _repository;

            public GetRequisitionTypeQueryHandler(IRBARepository repository)
            {
                _repository = repository;
            }
            public async Task<IEnumerable<RequisitionTypeOutputDTO>> Handle(GetRequisitionTypeQuery request, CancellationToken cancellationToken)
            {
                var requisitionTypes = await _repository.GetAllComplianceRequisitionType();

                IEnumerable<RequisitionTypeOutputDTO> outputDTO = requisitionTypes.Select(a => new RequisitionTypeOutputDTO
                {
                    RequisitionTypeCode = a.Id,
                    Description = a.Name
                });

                return outputDTO;
            }
        }
    }
}

