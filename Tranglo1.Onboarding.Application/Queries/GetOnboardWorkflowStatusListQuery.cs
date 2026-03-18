using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerOnboardProgress, UACAction.View)]
    public class GetOnboardWorkflowStatusListQuery : IRequest<IEnumerable<OnboardWorkflowStatusListOutputDTO>>
    {
        public class GetOnboardWorkflowStatusListQueryHandler : IRequestHandler<GetOnboardWorkflowStatusListQuery, IEnumerable<OnboardWorkflowStatusListOutputDTO>>
        {
            private readonly IPartnerRepository _partnerRepository;
            private readonly IMapper _mapper;


            public GetOnboardWorkflowStatusListQueryHandler(IPartnerRepository partnerRepository, IMapper mapper)
            {
                _partnerRepository = partnerRepository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<OnboardWorkflowStatusListOutputDTO>> Handle (GetOnboardWorkflowStatusListQuery query, CancellationToken cancellationToken)
            {
                return _mapper.Map<IEnumerable<OnboardWorkflowStatus>, IEnumerable<OnboardWorkflowStatusListOutputDTO>>(await _partnerRepository.GetOnboardWorkflowStatusAsync());

            }
        }
    }
}
