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
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_StatusUpdate_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]
    internal class UpdatePartnerAgreementStatusCommand : BaseCommand<Result<UpdatePartnerAgreementStatusOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public DateTime AgreementStartDate { get; set; }      
        public DateTime AgreementEndDate { get; set; }      
        public int AgreementStatus { get; set; }

        public override Task<string> GetAuditLogAsync(Result<UpdatePartnerAgreementStatusOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Updated partner agreement status";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdatePartnerAgreementStatusCommandHandler : IRequestHandler<UpdatePartnerAgreementStatusCommand, Result<UpdatePartnerAgreementStatusOutputDTO>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<UpdatePartnerAgreementStatusCommandHandler> _logger;

        public UpdatePartnerAgreementStatusCommandHandler(PartnerService partnerService, ILogger<UpdatePartnerAgreementStatusCommandHandler> logger)
        {
            _partnerService = partnerService;
            _logger = logger;
        }

        public async Task<Result<UpdatePartnerAgreementStatusOutputDTO>> Handle(UpdatePartnerAgreementStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var partner = await _partnerService.GetPartnerAgreementDetailsAsync(request.PartnerCode);
                if (partner != null)
                {
                    if (request.AgreementStartDate > request.AgreementEndDate)
                    {
                        return Result.Failure<UpdatePartnerAgreementStatusOutputDTO>($"Agreement start date cannot not be later than agreement end date.");
                    }
                    partner.AgreementStartDate = request.AgreementStartDate;
                    partner.AgreementEndDate = request.AgreementEndDate;
                    partner.AgreementStatus = request.AgreementStatus;
                    var result = await _partnerService.UpdatePartnerAgreementDetailsAsync(partner);
                    return Result.Success(new UpdatePartnerAgreementStatusOutputDTO()
                    {
                        PartnerCode = partner.Id,
                        AgreementStartDate = partner.AgreementStartDate,
                        AgreementEndDate = partner.AgreementEndDate,
                        AgreementStatus = partner.AgreementStatus ?? 0
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdatePartnerAgreementStatusCommand] {ex.ToString()}");
            }
            return Result.Failure<UpdatePartnerAgreementStatusOutputDTO>(
                $"Update partner agreement status failed for {request.PartnerCode}."
            );
        }
    }
}
