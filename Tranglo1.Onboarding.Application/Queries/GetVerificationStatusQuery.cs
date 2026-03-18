using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetVerificationStatusQuery : IRequest<IEnumerable<VerificationStatusOutputDTO>>
    {
        public bool? IsAdmin { get; set; }
        public class GetVerificationStatusQueryHandler : IRequestHandler<GetVerificationStatusQuery, IEnumerable<VerificationStatusOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;

            public GetVerificationStatusQueryHandler(IBusinessProfileRepository repository)
            {
                _repository = repository;
            }


            public async Task<IEnumerable<VerificationStatusOutputDTO>> Handle(GetVerificationStatusQuery request, CancellationToken cancellationToken)
            {
                bool isAdminUser = request.IsAdmin ?? false;

                var verificationStatuses = await _repository.GetAllVerificationStatus();

                IEnumerable<VerificationStatusOutputDTO> outputDTO;

                if (isAdminUser)
                {
                    var allowedStatuses = new List<VerificationStatus>()
                    {
                        VerificationStatus.Pending,
                        VerificationStatus.Passed,
                        VerificationStatus.Rejected
                    };

                    outputDTO = verificationStatuses
                    .Where(a => allowedStatuses.Contains(a))
                    .Select(a => new VerificationStatusOutputDTO
                    {
                        VerificationStatusCode = a.Id,
                        VerificationStatusDescription = a.Name
                    });
                }
                else
                {
                    outputDTO = verificationStatuses.Select(a => new VerificationStatusOutputDTO
                    {
                        VerificationStatusCode = a.Id,
                        VerificationStatusDescription = a.Name
                    });
                }

                return outputDTO;
            }
        }
    }
}
