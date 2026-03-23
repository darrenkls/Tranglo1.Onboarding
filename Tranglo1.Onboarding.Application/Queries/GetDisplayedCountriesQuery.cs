using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetDisplayedCountriesQuery : IRequest<IEnumerable<CountryListOutputDTO>>
    {
        public class GetDisplayedCountriesQueryHandler : IRequestHandler<GetDisplayedCountriesQuery, IEnumerable<CountryListOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetDisplayedCountriesQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<CountryListOutputDTO>> Handle(GetDisplayedCountriesQuery query, CancellationToken cancellationToken)
            {
                return await _context.Set<CountrySetting>()
                    .Where(x => x.IsDisplay)
                    .Select(x => x.Country)
                    .ProjectTo<CountryListOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
