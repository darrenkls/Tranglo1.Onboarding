using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
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
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly PartnerService _partnerService;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IPartnerRepository _partnerRepository;

        public GetBusinessOnboardingStatusQueryHandler(IMapper mapper, IConfiguration config,
            PartnerService partnerService, BusinessProfileService businessProfileService, IPartnerRepository partnerRepository)
        {
            _mapper = mapper;
            _config = config;
            _partnerService = partnerService;
            _businessProfileService = businessProfileService;
            _partnerRepository = partnerRepository;
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