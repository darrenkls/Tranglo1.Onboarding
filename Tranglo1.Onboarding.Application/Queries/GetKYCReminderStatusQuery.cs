using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetKYCReminderStatusQuery : IRequest<IEnumerable<KYCReminderStatusOutputDTO>>
    {
        public class GetKYCReminderStatusQueryHandler : IRequestHandler<GetKYCReminderStatusQuery, IEnumerable<KYCReminderStatusOutputDTO>>
        {
            private readonly IPartnerRepository _partnerRepository;

            public GetKYCReminderStatusQueryHandler(IPartnerRepository repository)
            {
                _partnerRepository = repository;
            }
            
            public async Task<IEnumerable<KYCReminderStatusOutputDTO>> Handle(GetKYCReminderStatusQuery request, CancellationToken cancellationToken)
            {
                var kycReminderStatus = await _partnerRepository.GetKYCReminderStatusesAsync();

                IEnumerable<KYCReminderStatusOutputDTO> outputDTO = kycReminderStatus.Select(a => new KYCReminderStatusOutputDTO
                {
                    KYCReminderStatusCode = a.Id,
                    Description = a.Name
                });

                return outputDTO;
            }
        }
    }
}
