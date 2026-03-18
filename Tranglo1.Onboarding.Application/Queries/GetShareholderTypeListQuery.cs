using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetShareholderTypeListQuery : IRequest<IEnumerable<ShareholderTypeListOutputDTO>>
	{
        public class GetShareholderTypeListQueryHandler : IRequestHandler<GetShareholderTypeListQuery, IEnumerable<ShareholderTypeListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly BusinessProfileDbContext _context;
            private readonly IMapper _mapper;
            public GetShareholderTypeListQueryHandler(BusinessProfileDbContext context, IMapper mapper, IBusinessProfileRepository repository)
            {
                _repository = repository;
                _context = context;
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
