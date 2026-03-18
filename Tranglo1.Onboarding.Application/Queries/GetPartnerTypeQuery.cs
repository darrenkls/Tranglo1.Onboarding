using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetPartnerTypeQuery : IRequest<IEnumerable<PartnerTypeOutputDTO>>
    {
        public class GetPartnerTypeQueryHandler : IRequestHandler<GetPartnerTypeQuery, IEnumerable<PartnerTypeOutputDTO>>
        {
            private readonly IPartnerRepository _repository;
            private readonly IMapper _mapper;

            public GetPartnerTypeQueryHandler(IPartnerRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            async Task<IEnumerable<PartnerTypeOutputDTO>> IRequestHandler<GetPartnerTypeQuery, IEnumerable<PartnerTypeOutputDTO>>.Handle(GetPartnerTypeQuery request, CancellationToken cancellationToken)
            {
                Specification<PartnerType> spec = Specification<PartnerType>.All;
                var partnerTypes = await _repository.GetPartnerTypes(spec);
                return _mapper.Map<IEnumerable<PartnerType>, IEnumerable<PartnerTypeOutputDTO>>(partnerTypes);
            }
        }
    }
}
