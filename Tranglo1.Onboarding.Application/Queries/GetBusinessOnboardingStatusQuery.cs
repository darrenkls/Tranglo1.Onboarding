using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetBusinessOnboardingStatusQuery : BaseQuery<Result<BusinessOnboardingStatusOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string UserBearerToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<BusinessOnboardingStatusOutputDTO> result)
        {
            string _description = $"Get Business Onboarding Status for BusinessProfileCode: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetBusinessOnboardingStatusQueryHandler : IRequestHandler<GetBusinessOnboardingStatusQuery, Result<BusinessOnboardingStatusOutputDTO>>
    {
        private readonly PartnerService _partnerService;

        public GetBusinessOnboardingStatusQueryHandler(PartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        public async Task<Result<BusinessOnboardingStatusOutputDTO>> Handle(GetBusinessOnboardingStatusQuery request, CancellationToken cancellationToken)
        {
            var businessKYCStatus = await _partnerService.GetCustomerBusinessKYCStatus(request.BusinessProfileCode);

            var outputDTO = new BusinessOnboardingStatusOutputDTO();
            outputDTO.BusinessOnboardingStatus = businessKYCStatus;

            return Result.Success(outputDTO);
        }
    }
}