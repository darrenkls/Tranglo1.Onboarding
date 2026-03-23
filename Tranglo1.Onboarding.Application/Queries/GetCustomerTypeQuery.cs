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
    public class GetCustomerTypeQuery : IRequest<IEnumerable<CustomerTypeOutputDTO>>
    {
        public class GetCustomerTypeQueryHandler : IRequestHandler<GetCustomerTypeQuery, IEnumerable<CustomerTypeOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;

            public GetCustomerTypeQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<CustomerTypeOutputDTO>> Handle(GetCustomerTypeQuery query, CancellationToken cancellationToken)
            {
                return await _context.CustomerTypes
                    .ProjectTo<CustomerTypeOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
