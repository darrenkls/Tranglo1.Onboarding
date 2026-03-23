using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetRoleStatusQuery : IRequest<IEnumerable<RoleStatusOutputDTO>>
    {
        public class GetRoleStatusQueryHandler : IRequestHandler<GetRoleStatusQuery, IEnumerable<RoleStatusOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetRoleStatusQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<RoleStatusOutputDTO>> Handle(GetRoleStatusQuery query, CancellationToken cancellationToken)
            {
                return await _context.RoleStatus
                    .ProjectTo<RoleStatusOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
