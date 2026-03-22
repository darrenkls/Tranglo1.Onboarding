using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetVerificationIDTypeQuery : IRequest<IEnumerable<VerificationIDTypeOutputDTO>>
    {
        public class GetVerificationIDTypeQueryHandler : IRequestHandler<GetVerificationIDTypeQuery, IEnumerable<VerificationIDTypeOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;

            public GetVerificationIDTypeQueryHandler(IBusinessProfileRepository repository)
            {
                _repository = repository;
            }

            public async Task<IEnumerable<VerificationIDTypeOutputDTO>> Handle(GetVerificationIDTypeQuery request, CancellationToken cancellationToken)
            {

                var verificationIDType = await _repository.GetAllVerificationIDTypes();

                IEnumerable<VerificationIDTypeOutputDTO> outputDTO = verificationIDType.Select(a => new VerificationIDTypeOutputDTO
                    {
                    VerificationIDTypeCode = a.Id,
                    VerificationIDTypeDescription = a.Name
                    });

                return outputDTO;


            }
        }
    }
}
