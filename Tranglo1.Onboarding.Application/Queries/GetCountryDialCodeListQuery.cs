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
    public class GetCountryDialCodeListQuery : IRequest<IEnumerable<CountryDialCodeOutputDTO>>
    {
        public class GetCountryDialCodeListQueryHandler : IRequestHandler<GetCountryDialCodeListQuery, IEnumerable<CountryDialCodeOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetCountryDialCodeListQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<CountryDialCodeOutputDTO>> Handle(GetCountryDialCodeListQuery query, CancellationToken cancellationToken)
            {
                return await _context.CountryMetas
                    .ProjectTo<CountryDialCodeOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
