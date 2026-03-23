using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.SignUpCode;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetSignUpCodeLeadsOriginListQuery : IRequest<IEnumerable<SignUpCodesLeadsOriginOutputDTO>>
    {
        public class GetSignUpCodeLeadsOriginListQueryHandler : IRequestHandler<GetSignUpCodeLeadsOriginListQuery, IEnumerable<SignUpCodesLeadsOriginOutputDTO>>
        {
            private readonly SignUpCodeDBContext _context;
            private readonly IMapper _mapper;

            public GetSignUpCodeLeadsOriginListQueryHandler(SignUpCodeDBContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<SignUpCodesLeadsOriginOutputDTO>> Handle(GetSignUpCodeLeadsOriginListQuery query, CancellationToken cancellationToken)
            {
                return await _context.LeadsOrigins
                    .ProjectTo<SignUpCodesLeadsOriginOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
