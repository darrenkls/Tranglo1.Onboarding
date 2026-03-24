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
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetReviewResultListQuery : IRequest<IEnumerable<ReviewResultListOutputDTO>>
	{
        public class GetReviewResultListQueryHandler : IRequestHandler<GetReviewResultListQuery, IEnumerable<ReviewResultListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            public GetReviewResultListQueryHandler(IBusinessProfileRepository repo, IMapper mapper)
            {
                _repository = repo;
                _mapper = mapper;
            }

            public async Task<IEnumerable<ReviewResultListOutputDTO>> Handle(GetReviewResultListQuery query, CancellationToken cancellationToken)
            {
                Specification<ReviewResult> spec = Specification<ReviewResult>.All;
                return _mapper.Map<IEnumerable<ReviewResult>, IEnumerable<ReviewResultListOutputDTO>>(await _repository.GetReviewResultsAsync(spec));
            
            }
        }
    }
}
