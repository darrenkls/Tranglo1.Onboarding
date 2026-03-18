using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetConnectOnboardingStatusQuery : BaseQuery<Result<ConnectOnboardingStatusOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<ConnectOnboardingStatusOutputDTO> result)
        {
            string _description = $"Get Connect Onboarding Status for BusinessProfileCode: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetConnectOnboardingStatusQueryHandler : IRequestHandler<GetConnectOnboardingStatusQuery, Result<ConnectOnboardingStatusOutputDTO>>
    {
        private readonly PartnerService _partnerService;

        public GetConnectOnboardingStatusQueryHandler(PartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        public async Task<Result<ConnectOnboardingStatusOutputDTO>> Handle(GetConnectOnboardingStatusQuery request, CancellationToken cancellationToken)
        {
            var onboardingStatusResult = await _partnerService.GetCustomerConnectKYCStatus(request.BusinessProfileCode);

            if (onboardingStatusResult.IsFailure)
            {
                return Result.Failure<ConnectOnboardingStatusOutputDTO>(onboardingStatusResult.Error);
            }

            var outputDTO = new ConnectOnboardingStatusOutputDTO();
            outputDTO.OnboardStatusDesc = onboardingStatusResult.Value.ToString();
            outputDTO.OnboardStatus = (int)onboardingStatusResult.Value;

            return Result.Success(outputDTO);
        }
    }
}