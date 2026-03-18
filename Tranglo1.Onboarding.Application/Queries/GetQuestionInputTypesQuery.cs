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
    public class GetQuestionInputTypesQuery : IRequest<IEnumerable<QuestionInputTypeOutputDTO>>
    {
        public class GetQuestionInputTypesQueryHandler : IRequestHandler<GetQuestionInputTypesQuery, IEnumerable<QuestionInputTypeOutputDTO>>
        {
            private readonly BusinessProfileDbContext _context;
            private readonly IMapper _mapper;

            public GetQuestionInputTypesQueryHandler(BusinessProfileDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<IEnumerable<QuestionInputTypeOutputDTO>> Handle(GetQuestionInputTypesQuery query, CancellationToken cancellationToken)
            {
                return await _context.QuestionInputTypes.ProjectTo<QuestionInputTypeOutputDTO>(_mapper.ConfigurationProvider)
                     .ToListAsync(cancellationToken);
            }
        }
    }
}
