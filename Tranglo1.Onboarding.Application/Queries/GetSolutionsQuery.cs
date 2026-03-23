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
    public class GetSolutionsQuery : IRequest<IEnumerable<SolutionListOutputDTO>>
    {
        public long? CustomerTypeCode { get; set; }

        public class GetSolutionsQueryHandler : IRequestHandler<GetSolutionsQuery, IEnumerable<SolutionListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetSolutionsQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<SolutionListOutputDTO>> Handle(GetSolutionsQuery query, CancellationToken cancellationToken)
            {
                return await _context.Solutions
                    .ProjectTo<SolutionListOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
