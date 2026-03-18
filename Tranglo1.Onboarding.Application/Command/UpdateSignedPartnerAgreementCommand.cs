using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    internal class UpdateSignedPartnerAgreementCommand : BaseCommand<Result<SignedPartnerAgreement>>
    {
        public long PartnerCode { get; set; }
        public Guid SignedDocumentId { get; set; }
        public bool IsDisplay { get; set; }

        public override Task<string> GetAuditLogAsync(Result<SignedPartnerAgreement> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update signed agreement for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdateSignedPartnerAgreementCommandHandler : IRequestHandler<UpdateSignedPartnerAgreementCommand, Result<SignedPartnerAgreement>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<UpdateSignedPartnerAgreementCommand> _logger;

        public UpdateSignedPartnerAgreementCommandHandler(PartnerService partnerService, ILogger<UpdateSignedPartnerAgreementCommand> logger)
        {
            _partnerService = partnerService;
            _logger = logger;
        }

        public async Task<Result<SignedPartnerAgreement>> Handle(UpdateSignedPartnerAgreementCommand request, CancellationToken cancellationToken)
        {
            try
            {
                SignedPartnerAgreement checkDoc = new SignedPartnerAgreement()
                {
                    PartnerCode = request.PartnerCode,
                    SignedDocumentId = request.SignedDocumentId,
                };

                var signedDocResult = await _partnerService.GetSignedPartnerAgreementBySignedDocumentIdAsync(checkDoc);
                if (signedDocResult != null)
                {
                    signedDocResult.IsDisplay = request.IsDisplay;
                    var result = await _partnerService.UpdateSignedPartnerAgreementAsync(signedDocResult);
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdateSignedPartnerAgreementCommand] {ex.Message}");
            }
            return Result.Failure<SignedPartnerAgreement>(
                    $"Update signed partner agreement failed for {request.PartnerCode}."
                    );
        }
    }
}
