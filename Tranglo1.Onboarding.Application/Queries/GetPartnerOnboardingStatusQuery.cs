using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerOnboarding;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerOnboardProgress, UACAction.View)]
    [Permission(Permission.ManagePartnerOnboardProgress.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]
    internal class GetPartnerOnboardingStatusQuery : BaseQuery<Result<PartnerOnboardingOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public string UserBearerToken { get; set; }
		public PartnerOnboardingOutputDTO PartnerOnboardingStatus;

        public override Task<string> GetAuditLogAsync(Result<PartnerOnboardingOutputDTO> result)
        {  
            string _description = $"Get Partner Onboarding Status for Partner Code: [{this.PartnerCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetPartnerOnboardingStatusQueryHandler : IRequestHandler<GetPartnerOnboardingStatusQuery, Result<PartnerOnboardingOutputDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly PartnerService _partnerService;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IPartnerRepository _partnerRepository;

        public GetPartnerOnboardingStatusQueryHandler(IMapper mapper, IConfiguration config,
            PartnerService partnerService, BusinessProfileService businessProfileService, IPartnerRepository partnerRepository)
        {
            _mapper = mapper;
            _config = config;
            _partnerService = partnerService;
            _businessProfileService = businessProfileService;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<PartnerOnboardingOutputDTO>> Handle (GetPartnerOnboardingStatusQuery query, CancellationToken cancellationToken)
        {
            var partnerProfile = await _partnerService.GetPartnerRegistrationByCodeAsync(query.PartnerCode);
            var subscription = await _partnerRepository.GetSubscriptionAsync(query.PartnerSubscriptionCode);

            var partnerOnboarding = new PartnerOnboardingOutputDTO();

            if (subscription.Solution == Solution.Connect)
            {
                var partnerKYCProfile = await _partnerService.GetPartnerKYCStatus(query.PartnerCode);
                var profileStatus = await _partnerService.GetPartnerProfileStatus(query.PartnerCode, query.PartnerSubscriptionCode);

                partnerOnboarding.ProfileOnboardWorkflowStatusCode = profileStatus.Value.Id;
                partnerOnboarding.KYCOnboardWorkflowStatusCode = partnerKYCProfile.Id;
                partnerOnboarding.AgreementOnboardWorkflowCode = partnerProfile.AgreementOnboardWorkflowStatusCode;
                partnerOnboarding.APIIntegrationOnboardWorkflowCode = subscription.APIIntegrationOnboardWorkflowStatusCode;
                partnerOnboarding.AgreementStatus = partnerProfile.AgreementStatus;
                partnerOnboarding.AgreementStartDate = partnerProfile.AgreementStartDate;
                partnerOnboarding.AgreementEndDate = partnerProfile.AgreementEndDate;

                if (partnerOnboarding.AgreementStatus != null && partnerOnboarding.AgreementStartDate != null && partnerOnboarding.AgreementEndDate != null)
                {
                    partnerOnboarding.CheckingOnAgreementStatusStartEndDate = true;
                }
            }
            else if (subscription.Solution == Solution.Business)
            {
                var partnerKYCProfile = await _partnerService.GetAdminBusinessKYCStatus(query.PartnerCode);
                var profileStatus = await _partnerService.GetBusinessPartnerProfileStatus(query.PartnerCode, query.PartnerSubscriptionCode);

                partnerOnboarding.ProfileOnboardWorkflowStatusCode = profileStatus.Value.Id;
                partnerOnboarding.KYCOnboardWorkflowStatusCode = partnerKYCProfile.Id;
                partnerOnboarding.AgreementOnboardWorkflowCode = null;
                partnerOnboarding.APIIntegrationOnboardWorkflowCode = null;
                partnerOnboarding.AgreementStatus = null;
                partnerOnboarding.AgreementStartDate = null;
                partnerOnboarding.AgreementEndDate = null;
            }
            
            return Result.Success(partnerOnboarding);
        }
    }
}