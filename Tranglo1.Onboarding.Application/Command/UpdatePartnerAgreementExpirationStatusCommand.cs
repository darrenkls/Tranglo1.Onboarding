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
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    internal class UpdatePartnerAgreementExpirationStatusCommand : BaseCommand<Result<PartnerRegistration>>
    {
        public long PartnerCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerRegistration> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Agreement Details for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdatePartnerAgreementExpirationStatusCommandHandler : IRequestHandler<UpdatePartnerAgreementExpirationStatusCommand, Result<PartnerRegistration>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<UpdatePartnerAgreementExpirationStatusCommandHandler> _logger;

        public UpdatePartnerAgreementExpirationStatusCommandHandler(PartnerService partnerService, ILogger<UpdatePartnerAgreementExpirationStatusCommandHandler> logger)
        {
            _partnerService = partnerService;
            _logger = logger;
        }

        public async Task<Result<PartnerRegistration>> Handle(UpdatePartnerAgreementExpirationStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var partner = await _partnerService.GetPartnerAgreementDetailsAsync(request.PartnerCode);
                if (partner != null)
                {
                    if (partner.AgreementEndDate < DateTime.Now)
                    {
                        partner.AgreementStatus = 2;
                        var checkResult = await _partnerService.UpdatePartnerAgreementDetailsAsync(partner);
                        return Result.Success(checkResult);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdatePartnerAgreementExpirationStatusCommand] {ex.Message}");
            }
            return Result.Failure<PartnerRegistration>(
                $"Update partner agreement expiration status failed for {request.PartnerCode}."
            );
        }
    }
}
