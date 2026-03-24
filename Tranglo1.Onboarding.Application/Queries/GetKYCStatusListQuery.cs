using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetKYCStatusListQuery : IRequest<IEnumerable<KYCStatusListOutputDTO>>
	{
        public class GetKYCStatusListQueryHandler : IRequestHandler<GetKYCStatusListQuery, IEnumerable<KYCStatusListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            public GetKYCStatusListQueryHandler(IBusinessProfileRepository repo, IMapper mapper)
            {
                _repository = repo;
                _mapper = mapper;
            }

            public async Task<IEnumerable<KYCStatusListOutputDTO>> Handle(GetKYCStatusListQuery query, CancellationToken cancellationToken)
            {
                Specification<KYCStatus> spec = Specification<KYCStatus>.All;
                return _mapper.Map<IEnumerable<KYCStatus>, IEnumerable<KYCStatusListOutputDTO>>(await _repository.GetKYCStatusesAsync(spec));
            
            }
        }
    }
}
