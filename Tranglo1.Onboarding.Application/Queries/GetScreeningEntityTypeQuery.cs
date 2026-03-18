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
    public class GetScreeningEntityTypeQuery : IRequest<IEnumerable<ScreeningEntityTypeListOutputDTO>>
    {
        public class GetScreeningEntityTypeQueryHandler : IRequestHandler<GetScreeningEntityTypeQuery, IEnumerable<ScreeningEntityTypeListOutputDTO>>
        {
            private readonly ScreeningDBContext _context;
            private readonly IMapper _mapper;
            public GetScreeningEntityTypeQueryHandler(ScreeningDBContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ScreeningEntityTypeListOutputDTO>> Handle(GetScreeningEntityTypeQuery query, CancellationToken cancellationToken)
            {
                return await _context.ScreeningEntityTypes.ProjectTo<ScreeningEntityTypeListOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
