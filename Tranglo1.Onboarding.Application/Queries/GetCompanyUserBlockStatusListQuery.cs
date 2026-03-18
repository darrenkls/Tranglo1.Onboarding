using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetCompanyUserBlockStatusListQuery : IRequest<IEnumerable<CompanyUserBlockStatusOutputDTO>>
    {
        public class GetCompanyUserAccountStatusListQueryHandler : IRequestHandler<GetCompanyUserBlockStatusListQuery, IEnumerable<CompanyUserBlockStatusOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            public GetCompanyUserAccountStatusListQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<IEnumerable<CompanyUserBlockStatusOutputDTO>> Handle(GetCompanyUserBlockStatusListQuery query, CancellationToken cancellationToken)
            {
                return await _context.CompanyUserBlockStatus.ProjectTo<CompanyUserBlockStatusOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
