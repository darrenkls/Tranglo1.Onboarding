using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetEnvironmentsQuery : IRequest<IEnumerable<EnvironmentsOutputDTO>>
    {
        public class GetEnvironmentsQueryHandler : IRequestHandler<GetEnvironmentsQuery, IEnumerable<EnvironmentsOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            public GetEnvironmentsQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<EnvironmentsOutputDTO>> Handle(GetEnvironmentsQuery query, CancellationToken cancellationToken)
            {
                return await _context.Environments.ProjectTo<EnvironmentsOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
