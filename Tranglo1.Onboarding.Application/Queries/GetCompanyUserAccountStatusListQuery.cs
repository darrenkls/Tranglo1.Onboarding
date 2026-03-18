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
    public class GetCompanyUserAccountStatusListQuery : IRequest<IEnumerable<CompanyUserAccountStatusOutputDTO>>
    {
        public class GetCompanyUserAccountStatusListQueryHandler : IRequestHandler<GetCompanyUserAccountStatusListQuery, IEnumerable<CompanyUserAccountStatusOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            public GetCompanyUserAccountStatusListQueryHandler(ApplicationUserDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<IEnumerable<CompanyUserAccountStatusOutputDTO>> Handle(GetCompanyUserAccountStatusListQuery query, CancellationToken cancellationToken)
            {
                return await _context.CompanyUserAccountStatus.ProjectTo<CompanyUserAccountStatusOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
