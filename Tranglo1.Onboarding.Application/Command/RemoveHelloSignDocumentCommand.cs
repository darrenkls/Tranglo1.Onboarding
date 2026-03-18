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
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_HelloSignRemove_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]

    internal class RemoveHelloSignDocumentCommand: BaseCommand<Result<HelloSignDocument>>
    {
        public long PartnerCode { get; set; }
        public long HelloSignDocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<HelloSignDocument> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Remove hellosign document name for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class RemoveHelloSignDocumentCommandHandler : IRequestHandler<RemoveHelloSignDocumentCommand, Result<HelloSignDocument>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<RemoveHelloSignDocumentCommandHandler> _logger;

        public RemoveHelloSignDocumentCommandHandler(PartnerService partnerService, ILogger<RemoveHelloSignDocumentCommandHandler> logger)
        {
            _partnerService = partnerService;
            _logger = logger;
        }

        public async Task<Result<HelloSignDocument>> Handle(RemoveHelloSignDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var checkResult = await _partnerService.GetHelloSignDocumentByHelloSignDocumentIdAsync(request.HelloSignDocumentId);
                if (checkResult != null)
                {
                    checkResult.PartnerCode = request.PartnerCode;
                    checkResult.IsRemoved = true;
                    var result = await _partnerService.RemoveHelloSignDocumentAsync(checkResult);
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RemoveHelloSignDocumentCommand] {ex.Message}");
            }
            return Result.Failure<HelloSignDocument>(
                                $"Remove hellosign document name failed for {request.PartnerCode}."
                            );

        }
    }
}
