using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Command
{
    public class KYCUnSubscriptionCommand : IRequest<bool>
    {
        public long PartnerSubscriptionCode { get; set; }
    }

    public class KYCUnSubscriptionCommandHandler : IRequestHandler<KYCUnSubscriptionCommand, bool>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        public KYCUnSubscriptionCommandHandler(IPartnerRepository partnerRepository, IBusinessProfileRepository businessProfileRepository)
        {
            _partnerRepository = partnerRepository;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<bool> Handle(KYCUnSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var psInfo = await _partnerRepository.GetPartnerSubscriptionByCodeAsync(request.PartnerSubscriptionCode);
            var prInfo = await _partnerRepository.GetPartnerRegistrationByCodeAsync(psInfo.PartnerCode);
            var bpInfo = await _businessProfileRepository.GetBusinessProfileByCodeAsync(prInfo.BusinessProfileCode);

            if(psInfo == null)
            {
                return false;
            }

            if (bpInfo.RegistrationDate < DateTime.UtcNow && bpInfo.RegistrationDate >= DateTime.UtcNow.AddDays(-30))
            {
                psInfo.KYCReminderSubscription = KYCReminderSubscription.Unsubscribed;

                var updatePartner = await _partnerRepository.UpdatePartnerSubscriptionsAsync(psInfo);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
