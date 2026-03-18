using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetSubmissionResultQuery : IRequest<IEnumerable<SubmissionResultOutputDTO>>
    {
        public class GetSubmissionResultQueryHandler : IRequestHandler<GetSubmissionResultQuery, IEnumerable<SubmissionResultOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;

            public GetSubmissionResultQueryHandler(IBusinessProfileRepository repository)
            {
                _repository = repository;
            }
            public async Task<IEnumerable<SubmissionResultOutputDTO>> Handle(GetSubmissionResultQuery request, CancellationToken cancellationToken)
            {
                var submissionResults = await _repository.GetAllSubmissionResult();

                IEnumerable<SubmissionResultOutputDTO> outputDTO = submissionResults.Select(a => new SubmissionResultOutputDTO
                {
                    SubmissionResultCode = a.Id,
                    SubmissionResultDescription = a.Name
                });

                return outputDTO;
            }
        }
    }
}
