using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.ExternalUserRole;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetExternalUserRoleStatusQuery : IRequest<IEnumerable<ExternalUserRoleStatusOutputDTO>>
    {
        public class GetExternalUserRoleStatusQueryHandler : IRequestHandler<GetExternalUserRoleStatusQuery, IEnumerable<ExternalUserRoleStatusOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetExternalUserRoleStatusQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ExternalUserRoleStatusOutputDTO>> Handle(GetExternalUserRoleStatusQuery query, CancellationToken cancellationToken)
            {
                return await _context.ExternalUserRoleStatuses
                    .ProjectTo<ExternalUserRoleStatusOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
