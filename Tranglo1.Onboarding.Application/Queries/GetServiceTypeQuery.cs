using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{

    public class GetServiceTypeQuery : IRequest<IReadOnlyList<ServiceTypeOutputDTO>>
    {
        public class GetServiceTypeQueryHandler : IRequestHandler<GetServiceTypeQuery, IReadOnlyList<ServiceTypeOutputDTO>>
        {
            private readonly BusinessProfileDbContext _businessProfileDbContext;
            private readonly IMapper _mapper;
            public GetServiceTypeQueryHandler(BusinessProfileDbContext businessProfileDbContext, IMapper mapper)
            {
                _businessProfileDbContext = businessProfileDbContext;
                _mapper = mapper;

            }
            public async Task<IReadOnlyList<ServiceTypeOutputDTO>> Handle(GetServiceTypeQuery query, CancellationToken cancellationToken)
            {
                return await _businessProfileDbContext.ServiceTypes.ProjectTo<ServiceTypeOutputDTO>(_mapper.ConfigurationProvider)
                   .ToListAsync(cancellationToken);
            }
        }
    }
}
