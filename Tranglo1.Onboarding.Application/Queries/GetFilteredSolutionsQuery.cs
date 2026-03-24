using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetFilteredSolutionsQuery : IRequest<IEnumerable<SolutionListOutputDTO>>
    {
        public class GetFilteredSolutionsQueryHandler : IRequestHandler<GetFilteredSolutionsQuery, IEnumerable<SolutionListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetFilteredSolutionsQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<SolutionListOutputDTO>> Handle(GetFilteredSolutionsQuery query, CancellationToken cancellationToken)
            {
                // Filter to only TB (Business, Id=2) and TC (Connect, Id=1) solutions
                return await _context.Solutions
                    .Where(x => x.Id == 1 || x.Id == 2)
                    .ProjectTo<SolutionListOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
