using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetUserStatusQuery : IRequest<IEnumerable<UserStatusListOutputDTO>>
    {
        public class GetUserStatusQueryHandler : IRequestHandler<GetUserStatusQuery, IEnumerable<UserStatusListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetUserStatusQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<UserStatusListOutputDTO>> Handle(GetUserStatusQuery query, CancellationToken cancellationToken)
            {
                return await _context.AccountStatuses
                    .ProjectTo<UserStatusListOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
