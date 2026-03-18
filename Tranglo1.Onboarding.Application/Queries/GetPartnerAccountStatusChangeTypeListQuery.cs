using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetPartnerAccountStatusChangeTypeListQuery : IRequest<IEnumerable<PartnerAccountStatusChangeTypeOutputDTO>>
    {
        public class GetPartnerAccountStatusChangeTypeListQueryHandler : IRequestHandler<GetPartnerAccountStatusChangeTypeListQuery, IEnumerable<PartnerAccountStatusChangeTypeOutputDTO>>
        {
            private readonly IPartnerRepository _repository;
            private readonly IMapper _mapper;
            public GetPartnerAccountStatusChangeTypeListQueryHandler(IPartnerRepository repo, IMapper mapper)
            {
                _repository = repo;
                _mapper = mapper;
            }

            async Task<IEnumerable<PartnerAccountStatusChangeTypeOutputDTO>> IRequestHandler<GetPartnerAccountStatusChangeTypeListQuery, IEnumerable<PartnerAccountStatusChangeTypeOutputDTO>>.Handle(GetPartnerAccountStatusChangeTypeListQuery request, CancellationToken cancellationToken)
            {
                return _mapper.Map<IEnumerable<ChangeType>, IEnumerable<PartnerAccountStatusChangeTypeOutputDTO>>(await _repository.GetPartnerAccountStatusChangeTypeAsync());
            }
        }
    }
}
