using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;
namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetOwnershipStructureTypeQuery : IRequest<IEnumerable<KYCOwnershipStructureTypeOutputDTO>>
    {
        public class GetOwnershipStructureTypeQueryHandler : IRequestHandler<GetOwnershipStructureTypeQuery, IEnumerable<KYCOwnershipStructureTypeOutputDTO>>
        {
            private readonly ScreeningDBContext _context;
            private readonly IMapper _mapper;
            public GetOwnershipStructureTypeQueryHandler(ScreeningDBContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<KYCOwnershipStructureTypeOutputDTO>> Handle(GetOwnershipStructureTypeQuery query, CancellationToken cancellationToken)
            {
                // Filter out obsoleted ownership structure types
                var obsoletedOwnershipStructureTypeIds = OwnershipStrucureType.GetObsoletedOwnershipStructureTypes().Select(x => x.Id);

                return await _context.OwnershipStrucureTypes
                    .Where(x => !obsoletedOwnershipStructureTypeIds.Contains(x.Id))
                    .ProjectTo<KYCOwnershipStructureTypeOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
