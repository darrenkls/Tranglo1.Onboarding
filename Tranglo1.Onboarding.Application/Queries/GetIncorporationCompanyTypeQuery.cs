using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetIncorporationCompanyTypeQuery : IRequest<IEnumerable<IncorporationCompanyTypeOutputDTO>>
    {
        public class GetIncorporationCompanyTypeQueryHandler : IRequestHandler<GetIncorporationCompanyTypeQuery, IEnumerable<IncorporationCompanyTypeOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            private readonly IConfiguration _configuration;

            public GetIncorporationCompanyTypeQueryHandler(ApplicationUserDbContext context, IMapper mapper, IConfiguration configuration)
            {
                _context = context;
                _mapper = mapper;
                _configuration = configuration;
            }

            public async Task<IEnumerable<IncorporationCompanyTypeOutputDTO>> Handle(GetIncorporationCompanyTypeQuery query, CancellationToken cancellationToken)
            {
                var incorporationCompanyTypes = await _context.IncorporationCompanyTypes
                    .OrderBy(x => x.Name)
                    .ProjectTo<IncorporationCompanyTypeOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Filter out Foundation/Trust if the feature is not ready
                if (!_configuration.GetValue<bool>("IsFoundationTrustReady"))
                {
                    incorporationCompanyTypes = incorporationCompanyTypes
                        .Where(x => x.IncorporationCompanyTypeCode != (int)IncorporationCompanyType.Foundation_Trust.Id)
                        .ToList();
                }

                return incorporationCompanyTypes;
            }
        }
    }
}
