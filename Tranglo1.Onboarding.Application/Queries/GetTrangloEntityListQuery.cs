using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetTrangloEntityListQuery : IRequest<IEnumerable<TrangloEntityListOutputDTO>>
    {
        public class GetTrangloEntityListQueryHandler : IRequestHandler<GetTrangloEntityListQuery, IEnumerable<TrangloEntityListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            public GetTrangloEntityListQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<IEnumerable<TrangloEntityListOutputDTO>> Handle(GetTrangloEntityListQuery query, CancellationToken cancellationToken)
            {
                return await _context.TrangloEntity.ProjectTo<TrangloEntityListOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
