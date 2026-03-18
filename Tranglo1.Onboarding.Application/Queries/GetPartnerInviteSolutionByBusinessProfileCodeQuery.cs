using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.DTO.SignUpCode;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerInviteSolutionByBusinessProfileCodeQuery : BaseCommand<Result<PartnerInviteSolutionByBusinessProfileOutputDTO>>
    {
        public long BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerInviteSolutionByBusinessProfileOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get Partner Invite by Solution for business profile : [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetPartnerInviteSolutionByBusinessProfileCodeQueryHandler : IRequestHandler<GetPartnerInviteSolutionByBusinessProfileCodeQuery, Result<PartnerInviteSolutionByBusinessProfileOutputDTO>>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<GetPartnerInviteSolutionByBusinessProfileCodeQuery> _logger;


        public GetPartnerInviteSolutionByBusinessProfileCodeQueryHandler(IBusinessProfileRepository businessProfileRepository,
        IPartnerRepository partnerRepository, ILogger<GetPartnerInviteSolutionByBusinessProfileCodeQuery> logger)
        {
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
            _logger = logger;
        }

        public async Task<Result<PartnerInviteSolutionByBusinessProfileOutputDTO>> Handle(GetPartnerInviteSolutionByBusinessProfileCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var businessProfileInfo = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
                var partnerInfo = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync((int)request.BusinessProfileCode);
                var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerInfo.Id);
                var isTrangloBusinessExist = partnerSubInfo.Any(x => x.Solution == Solution.Business);
                var isTrangloConnectExist = partnerSubInfo.Any(x => x.Solution == Solution.Connect);

                PartnerInviteSolutionByBusinessProfileOutputDTO outputDTO = new PartnerInviteSolutionByBusinessProfileOutputDTO
                {
                    CompanyName = businessProfileInfo.CompanyName,
                    BusinessProfileCode = businessProfileInfo.Id,
                    IsTrangloBusinessExist = isTrangloBusinessExist,
                    IsTrangloConnectExist = isTrangloConnectExist
                };

                return Result.Success(outputDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetPartnerInviteSolutionByBusinessProfileCodeQuery] {ex.Message}");
            }
            return Result.Failure<PartnerInviteSolutionByBusinessProfileOutputDTO>(
                        $"Get partner invite failed for {request.BusinessProfileCode}."
                    );
            
        }
    }
}
