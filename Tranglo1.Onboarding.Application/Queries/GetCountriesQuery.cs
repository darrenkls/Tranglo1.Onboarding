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
    public class GetCountriesQuery : IRequest<IEnumerable<CountryListOutputDTO>>
    {
        public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, IEnumerable<CountryListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetCountriesQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<CountryListOutputDTO>> Handle(GetCountriesQuery query, CancellationToken cancellationToken)
            {
                return await _context.CountryMetas
                    .ProjectTo<CountryListOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
