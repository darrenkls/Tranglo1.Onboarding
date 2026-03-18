using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerOnboarding;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerRequestGoLive, UACAction.View)]
    [Permission(Permission.RequestGoLiveButton.Action_View_Code,
        new int[] { (int)PortalCode.Connect },
        new string[] { })]
    internal class GetPartnerWorkflowStatusQuery : BaseQuery<Result<PartnerOnboardingOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public PartnerOnboardingOutputDTO PartnerOnboardingStatus;

        public override Task<string> GetAuditLogAsync(Result<PartnerOnboardingOutputDTO> result)
        {
                string _description = $"Get Partner Workflow Status for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
        }
    }

    internal class GetPartnerWorkflowStatusQueryHandler : IRequestHandler<GetPartnerWorkflowStatusQuery, Result<PartnerOnboardingOutputDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;

        public GetPartnerWorkflowStatusQueryHandler(IMapper mapper, IConfiguration config,
            PartnerService partnerService, IPartnerRepository partnerRepository)
        {
            _mapper = mapper;
            _config = config;
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<PartnerOnboardingOutputDTO>> Handle (GetPartnerWorkflowStatusQuery query, CancellationToken cancellationToken)
        {
            var partnerProfile = await _partnerService.GetPartnerRegistrationByCodeAsync(query.PartnerCode);
            var subscription = await _partnerRepository.GetSubscriptionAsync(query.PartnerSubscriptionCode);
            var partnerKYCProfile = await _partnerService.GetPartnerKYCStatus(query.PartnerCode);
            var profileStatus = await _partnerService.GetPartnerProfileStatus(query.PartnerCode, query.PartnerSubscriptionCode);

            if (profileStatus.IsFailure) { return Result.Failure<PartnerOnboardingOutputDTO>(profileStatus.Error); }

            PartnerOnboardingOutputDTO partnerOnboarding = new PartnerOnboardingOutputDTO()
            {
                ProfileOnboardWorkflowStatusCode = profileStatus.Value.Id,
                KYCOnboardWorkflowStatusCode = partnerKYCProfile.Id,
                AgreementOnboardWorkflowCode = partnerProfile.AgreementOnboardWorkflowStatusCode,
                APIIntegrationOnboardWorkflowCode = subscription.APIIntegrationOnboardWorkflowStatusCode,
            };

            return Result.Success(partnerOnboarding);
        }
    }
}
