using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
	internal class GetPartnerGoLiveIsProcessedQuery : IRequest <Result<bool>>
	{
		public long PartnerCode { get; set; }
		public long PartnerSubscriptionCode { get; set; }

		public class GetPartnerGoLiveIsProcessedQueryHandler : IRequestHandler<GetPartnerGoLiveIsProcessedQuery, Result<bool>>
		{
			private readonly IPartnerRepository _partnerRepository;

			public GetPartnerGoLiveIsProcessedQueryHandler(IPartnerRepository partnerRepository)
			{
				_partnerRepository = partnerRepository;
			}
			public async Task<Result<bool>> Handle(GetPartnerGoLiveIsProcessedQuery request, CancellationToken cancellationToken)
			{
				var partnerCMSIntegrationDetails = await _partnerRepository.GetPartnerCMSIntegrationByPartnerSubscriptionCodeAsync(request.PartnerSubscriptionCode);
				var cmsWalletIntegrationDetails = await _partnerRepository.GetPartnerWalletIntegrationByPartnerSubscriptionCodeAsync(request.PartnerSubscriptionCode);
				var partnerSubscription = await _partnerRepository.GetPartnerSubscriptionByCodeAsync(request.PartnerSubscriptionCode);
				bool result = false;

				if (partnerSubscription.Solution == Solution.Connect && partnerSubscription.Environment == Environment.Production
					&& partnerCMSIntegrationDetails != null && cmsWalletIntegrationDetails != null)
				{
					result = true;
				}

				return Result.Success(result);
			}
		}
	}
}
