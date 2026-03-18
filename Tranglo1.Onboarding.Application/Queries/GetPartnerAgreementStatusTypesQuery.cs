using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetPartnerAgreementStatusTypesQuery : IRequest<IEnumerable<PartnerAgreementStatusTypeOutputDTO>>
    {
        public class GetPartnerAgreementStatusTypesQueryHandler : IRequestHandler<GetPartnerAgreementStatusTypesQuery, IEnumerable<PartnerAgreementStatusTypeOutputDTO>>
        {
            private readonly PartnerDBContext _context;
            private readonly IMapper _mapper;
            public GetPartnerAgreementStatusTypesQueryHandler(PartnerDBContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<PartnerAgreementStatusTypeOutputDTO>> Handle(GetPartnerAgreementStatusTypesQuery query, CancellationToken cancellationToken)
            {
                return await _context.PartnerAgreementStatus.ProjectTo<PartnerAgreementStatusTypeOutputDTO>(_mapper.ConfigurationProvider)
                      .ToListAsync(cancellationToken);
            }
        }
    }
}
