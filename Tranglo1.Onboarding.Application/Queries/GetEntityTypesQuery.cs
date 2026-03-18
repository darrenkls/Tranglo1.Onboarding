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
    public class GetEntityTypesQuery : IRequest<IEnumerable<EntityTypesOutputDTO>>
    {
        public class GetEntityTypesQueryHandler : IRequestHandler<GetEntityTypesQuery, IEnumerable<EntityTypesOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetEntityTypesQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<IEnumerable<EntityTypesOutputDTO>> Handle(GetEntityTypesQuery query, CancellationToken cancellationToken)
            {
                return await _context.EntityTypes.ProjectTo<EntityTypesOutputDTO>(_mapper.ConfigurationProvider)
                     .ToListAsync(cancellationToken);
            }
        }
    }
}
