using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetSubscriptionListQuery : BaseQuery<Result<GetPartnerSubscriptionOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public string TrangloEntity { get; set; }
        public string UserBearerToken { get; set; }
    }
    internal class GetSubscriptionListQueryHandler : IRequestHandler<GetSubscriptionListQuery, Result<GetPartnerSubscriptionOutputDTO>>
    {
        private readonly IPartnerRepository _partnerRepository;

        public GetSubscriptionListQueryHandler(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<GetPartnerSubscriptionOutputDTO>> Handle(GetSubscriptionListQuery request, CancellationToken cancellationToken)
        {
            var outputDTO = new GetPartnerSubscriptionOutputDTO();
            var subscriptions = new List<SubscriptionList>();

            outputDTO.PartnerCode = request.PartnerCode;
            outputDTO.TrangloEntity = request.TrangloEntity;

			var partnerSubscriptions = _partnerRepository.GetSubscriptionWithNullEntityAsync(request.PartnerCode, request.TrangloEntity).Result;

            if (partnerSubscriptions != null)
            {
                foreach (var s in partnerSubscriptions)
                {
					SubscriptionList subscription = new SubscriptionList
					{
						PartnerSubscriptionCode = s.Id,
						PartnerType = s.PartnerType?.Id,
						Solution = s.Solution?.Id,
						Currency = s.SettlementCurrencyCode
					};

					var partnerCMSIntegrationDetails = await _partnerRepository.GetPartnerCMSIntegrationByPartnerSubscriptionCodeAsync(s.Id);
                    var cmsWalletIntegrationDetails = await _partnerRepository.GetPartnerWalletIntegrationByPartnerSubscriptionCodeAsync(s.Id);

                    if (partnerCMSIntegrationDetails != null && cmsWalletIntegrationDetails != null || s.Environment == Domain.Entities.Environment.Production)
                    {
                        subscription.isLive = true;
                    }
                    else
                    {
                        subscription.isLive = false;
                    }

                    subscriptions.Add(subscription);
                }
            }

            outputDTO.SubscriptionList = subscriptions;
            return Result.Success(outputDTO);
        }
    }
}
