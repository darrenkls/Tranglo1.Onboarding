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
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_DocumentToBeSignedRemove_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]
    internal class RemovePartnerAgreementTemplateCommand: BaseCommand<Result<PartnerAgreementTemplate>>
    {
        public long PartnerCode { get; set; }
        public Guid TemplateId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerAgreementTemplate> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Remove agreement template for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class RemovePartnerAgreementTemplateCommandHandler : IRequestHandler<RemovePartnerAgreementTemplateCommand, Result<PartnerAgreementTemplate>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<RemovePartnerAgreementTemplateCommandHandler> _logger;

        public RemovePartnerAgreementTemplateCommandHandler(PartnerService partnerService, ILogger<RemovePartnerAgreementTemplateCommandHandler> logger)
        {
            _partnerService = partnerService;
            _logger = logger;
        }

        public async Task<Result<PartnerAgreementTemplate>> Handle(RemovePartnerAgreementTemplateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                PartnerAgreementTemplate checkTemplate = new PartnerAgreementTemplate()
                {
                    PartnerCode = request.PartnerCode,
                    TemplateId = request.TemplateId,
                };

                var templateResult = await _partnerService.GetPartnerAgreementTemplateByTemplateIdAsync(checkTemplate);
                if (templateResult != null)
                {
                    templateResult.IsRemoved = true;
                    var result = await _partnerService.RemovePartnerAgreementTemplateAsync(templateResult);
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RemovePartnerAgreementTemplateCommand] {ex.Message}");
            }
            return Result.Failure<PartnerAgreementTemplate>(
                                $"Remove partner agreement template failed for {request.PartnerCode}."
                            );
        }
    }
}
