using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetIDTypeListQuery : IRequest<IEnumerable<IDTypeListOutputDTO>>
	{
        public class GetIDTypeListQueryHandler : IRequestHandler<GetIDTypeListQuery, IEnumerable<IDTypeListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            public GetIDTypeListQueryHandler(IMapper mapper, IBusinessProfileRepository repository)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<IEnumerable<IDTypeListOutputDTO>> Handle(GetIDTypeListQuery query, CancellationToken cancellationToken)
            {
                Specification<IDType> spec = Specification<IDType>.All;

                var idTypes = await _repository.GetIDTypesAsync(spec);
                return _mapper.Map<IEnumerable<IDType>, IEnumerable<IDTypeListOutputDTO>>(idTypes);

            }
        }
    }
}
