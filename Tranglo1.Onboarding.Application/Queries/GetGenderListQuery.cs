using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetGenderListQuery : IRequest<IEnumerable<GenderListOutputDTO>>
	{
        public class GetGenderListQueryHandler : IRequestHandler<GetGenderListQuery, IEnumerable<GenderListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            public GetGenderListQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<GenderListOutputDTO>> Handle(GetGenderListQuery query, CancellationToken cancellationToken)
            {
                return await _context.Genders.ProjectTo<GenderListOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
