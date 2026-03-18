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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;
using System.Security.Claims;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_InternalDocument_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class RemoveInternalDocumentUploadCommand : BaseCommand<Result<InternalDocumentUpload>>
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<InternalDocumentUpload> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Remove Internal Document Upload for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class RemoveInternalDocumentUploadCommandHandler : IRequestHandler<RemoveInternalDocumentUploadCommand, Result<InternalDocumentUpload>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IIdentityContext _identityContext;
        private readonly ILogger<RemoveInternalDocumentUploadCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;

        public RemoveInternalDocumentUploadCommandHandler(BusinessProfileService businessProfileService, IIdentityContext identityContext, ILogger<RemoveInternalDocumentUploadCommandHandler> logger,TrangloUserManager userManager,
                                                      PartnerService partnerService)
        {
            _businessProfileService = businessProfileService;
            _identityContext = identityContext;
            _logger = logger;
            _userManager = userManager;
            _partnerService = partnerService;
        }

        public async Task<Result<InternalDocumentUpload>> Handle(RemoveInternalDocumentUploadCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            try
            {
                InternalDocumentUpload checkFileUpload = new InternalDocumentUpload()
                {
                    BusinessProfileCode = request.BusinessProfileCode,
                    DocumentId = request.DocumentId
                };

                var documentUploadResult = await _businessProfileService.GetInternalDocumentUploadByDocumentIdAsync(checkFileUpload);

                if (documentUploadResult != null)
                {
                    documentUploadResult.IsDisplay = false;
                    documentUploadResult.IsRemoved = true;
                    documentUploadResult.RemovedDate = DateTime.UtcNow;
                    documentUploadResult.RemovedBy = _identityContext.CurrentUser.GetUserId().Value;
                    var result = await _businessProfileService.RemoveInternalDocumentUploadAsync(documentUploadResult);


                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RemoveInternalDocumentUploadCommand] {ex.Message}");
            }
            return Result.Failure<InternalDocumentUpload>(
                                $"Removal of Internal Document Upload failed for Business Profile Code {request.BusinessProfileCode}."
                            );
        }
    }
}
