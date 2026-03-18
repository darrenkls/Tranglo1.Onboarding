using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.DocumentStorage;
using System.Security.Claims;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_DocumentRecordsRemove_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]
    internal class RemoveSignedPartnerAgreementCommand : BaseCommand<Result<SignedPartnerAgreement>>
    {
        public long PartnerCode { get; set; }
        public Guid SignedDocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<SignedPartnerAgreement> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Remove signed agreement for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class RemoveSignedPartnerAgreementCommandHandler : IRequestHandler<RemoveSignedPartnerAgreementCommand, Result<SignedPartnerAgreement>>
    {
        private readonly PartnerService _partnerService;
        private readonly IIdentityContext _identityContext;
        private readonly ILogger<RemoveSignedPartnerAgreementCommandHandler> _logger;

        public RemoveSignedPartnerAgreementCommandHandler(PartnerService partnerService, IIdentityContext identityContext, ILogger<RemoveSignedPartnerAgreementCommandHandler> logger)
        {
            _partnerService = partnerService;
            _identityContext = identityContext;
            _logger = logger;
        }

        public async Task<Result<SignedPartnerAgreement>> Handle(RemoveSignedPartnerAgreementCommand request, CancellationToken cancellationToken)
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
                    signedDocResult.IsRemoved = true;
                    signedDocResult.RemovedDate = DateTime.UtcNow;
                    signedDocResult.RemovedBy = _identityContext.CurrentUser.GetUserId().Value;
                    var result = await _partnerService.RemoveSignedPartnerAgreementAsync(signedDocResult);
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RemoveSignedPartnerAgreementCommand] {ex.Message}");
            }
            return Result.Failure<SignedPartnerAgreement>(
                                $"Remove signed partner agreement failed for {request.PartnerCode}."
                            );
        }
    }
}
