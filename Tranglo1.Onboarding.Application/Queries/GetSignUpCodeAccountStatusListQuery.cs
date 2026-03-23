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
    public class GetSignUpCodeAccountStatusListQuery : IRequest<IEnumerable<SignUpCodeAccountStatusOutputDTO>>
    {
        public class GetSignUpCodeAccountStatusListQueryHandler : IRequestHandler<GetSignUpCodeAccountStatusListQuery, IEnumerable<SignUpCodeAccountStatusOutputDTO>>
        {
            private readonly SignUpCodeDBContext _context;
            private readonly IMapper _mapper;

            public GetSignUpCodeAccountStatusListQueryHandler(SignUpCodeDBContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IEnumerable<SignUpCodeAccountStatusOutputDTO>> Handle(GetSignUpCodeAccountStatusListQuery query, CancellationToken cancellationToken)
            {
                return await _context.SignUpAccountStatuses
                    .ProjectTo<SignUpCodeAccountStatusOutputDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
