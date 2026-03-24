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
    public class GetUserRolesQuery : IRequest<IEnumerable<UserRolesOutputDTO>>
    {
        public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IEnumerable<UserRolesOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetUserRolesQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<UserRolesOutputDTO>> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
            {
                return await _context.UserRoles
                    .ProjectTo<UserRolesOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
