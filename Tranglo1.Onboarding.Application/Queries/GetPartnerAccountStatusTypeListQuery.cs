using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetPartnerAccountStatusTypeListQuery : IRequest<IEnumerable<PartnerAccountStatusTypeOutputDTO>>
    {
        public class GetPartnerAccountStatusTypeListQueryHandler :IRequestHandler<GetPartnerAccountStatusTypeListQuery, IEnumerable<PartnerAccountStatusTypeOutputDTO>>
        {
            private readonly IPartnerRepository _repository;
            private readonly IMapper _mapper;
            public GetPartnerAccountStatusTypeListQueryHandler(IPartnerRepository repo, IMapper mapper)
            {
                _repository = repo;
                _mapper = mapper;
            }

            async Task<IEnumerable<PartnerAccountStatusTypeOutputDTO>> IRequestHandler<GetPartnerAccountStatusTypeListQuery, IEnumerable<PartnerAccountStatusTypeOutputDTO>>.Handle(GetPartnerAccountStatusTypeListQuery request, CancellationToken cancellationToken)
            {
                return _mapper.Map<IEnumerable<PartnerAccountStatusType>, IEnumerable<PartnerAccountStatusTypeOutputDTO>>(await _repository.GetPartnerAccountStatusTypeAsync());
            }

        }
    }
}

