using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetBusinessProfileIDTypeListQuery : IRequest<IEnumerable<BusinessProfileIDTypeListOutputDTO>>
    {
        public class GetBusinessProfileIDTypeListQueryHandler : IRequestHandler<GetBusinessProfileIDTypeListQuery, IEnumerable<BusinessProfileIDTypeListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            public GetBusinessProfileIDTypeListQueryHandler(IMapper mapper, IBusinessProfileRepository repository)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<BusinessProfileIDTypeListOutputDTO>> Handle(GetBusinessProfileIDTypeListQuery request, CancellationToken cancellationToken)
            {
                Specification<BusinessProfileIDType> spec = Specification<BusinessProfileIDType>.All;

                var idTypes = await _repository.GetBusinessProfileIDTypesAsync(spec);
                return _mapper.Map<IEnumerable<BusinessProfileIDType>, IEnumerable<BusinessProfileIDTypeListOutputDTO>>(idTypes);
            }
        }
    }
}
