using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetShareholderTypeListQuery : IRequest<IEnumerable<ShareholderTypeListOutputDTO>>
	{
        public class GetShareholderTypeListQueryHandler : IRequestHandler<GetShareholderTypeListQuery, IEnumerable<ShareholderTypeListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            public GetShareholderTypeListQueryHandler(IMapper mapper, IBusinessProfileRepository repository)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ShareholderTypeListOutputDTO>> Handle(GetShareholderTypeListQuery query, CancellationToken cancellationToken)
            {
                Specification<ShareholderType> spec = Specification<ShareholderType>.All;

                var shareholderTypes = await _repository.GetShareholderTypesAsync(spec);
                return _mapper.Map<IEnumerable<ShareholderType>, IEnumerable<ShareholderTypeListOutputDTO>>(shareholderTypes);

                
            }
        }
    }
}
