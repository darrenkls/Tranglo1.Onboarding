using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.Partner;

namespace Tranglo1.Onboarding.Application.Command
{
    public class UpdatePartnerTermsAndConditionsAcceptanceDateCommand : BaseCommand<Result<UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO>>
    {
        public long PartnerCode { get; set; }
       
        public override Task<string> GetAuditLogAsync(Result<UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Partner Terms And Condition Date for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
        internal class UpdatePartnerTermsAndConditionsAcceptanceDateCommandHandler : IRequestHandler<UpdatePartnerTermsAndConditionsAcceptanceDateCommand, Result<UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO>>
        {
            private readonly PartnerService _partnerService;

            private readonly ILogger<UpdatePartnerTermsAndConditionsAcceptanceDateCommandHandler> _logger;

            public UpdatePartnerTermsAndConditionsAcceptanceDateCommandHandler(
                                    PartnerService partnerService,
                                    ILogger<UpdatePartnerTermsAndConditionsAcceptanceDateCommandHandler> logger)

            {
                _partnerService = partnerService;
                _logger = logger;
            }


            public async Task<Result<UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO>> Handle(UpdatePartnerTermsAndConditionsAcceptanceDateCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var partnerDetails = await GetPartnerDetailsAsync(request.PartnerCode);

                    if (partnerDetails != null)
                    {
                        partnerDetails.TermsAcceptanceDate = DateTime.UtcNow.Date;
                        await UpdatePartnerDetailsAsync(partnerDetails);

                        UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO result = CreateCommandOutput(partnerDetails);
                        return Result.Success(result);
                    }
                    else
                    {
                        return Result.Failure<UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO>("Partner details not found.");
                    }
                }
                catch (Exception ex)
                {
                    return Result.Failure<UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO>($"An error occurred: {ex.Message}");
                }
            }

            private async Task<PartnerRegistration> GetPartnerDetailsAsync(long partnerCode)
            {
                return await _partnerService.GetPartnerRegistrationByCodeAsync(partnerCode);
            }

            private async Task UpdatePartnerDetailsAsync(PartnerRegistration partnerDetails)
            {
                await _partnerService.UpdatePartnerRegistrationAsync(partnerDetails);
            }

            private UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO CreateCommandOutput(PartnerRegistration partnerDetails)
            {
                return new UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO()
                {
                    PartnerCode = partnerDetails.Id,
                    TermsAcceptanceDate = partnerDetails.TermsAcceptanceDate
                };
            }
        }
    }

}


