using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_DocumentToBeSignedUpload_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]
    internal class SavePartnerAgreementTemplateCommand : BaseCommand<Result<Guid>>
    {
        public long PartnerCode { get; set; }
        public IFormFile UploadedFile { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Uploaded partner document";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePartnerAgreementCommandHandler : IRequestHandler<SavePartnerAgreementTemplateCommand, Result<Guid>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<SavePartnerAgreementCommandHandler> _logger;
        private readonly StorageManager _storageManager;

        public SavePartnerAgreementCommandHandler(PartnerService partnerService, ILogger<SavePartnerAgreementCommandHandler> logger,
                                                        StorageManager storageManager)
        {
            _partnerService = partnerService;
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<Result<Guid>> Handle(SavePartnerAgreementTemplateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    request.UploadedFile.CopyTo(ms);
                    ms.Position = 0;
                    var doc = await _storageManager.StoreAsync(ms, request.UploadedFile.FileName, request.UploadedFile.ContentType);

                    PartnerAgreementTemplate partnerAgreement = new PartnerAgreementTemplate()
                    {
                        SolutionCode = request.AdminSolution,
                        PartnerCode = request.PartnerCode,
                        TemplateId = doc.DocumentId
                    };

                    await _partnerService.AddPartnerAgreementTemplateUploadAsync(partnerAgreement);
                    return Result.Success<Guid>(doc.DocumentId);

                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"[SavePartnerAgreementTemplateCommand] {ex.Message}");
            }
            return Result.Failure<Guid>(
                            $"Save partner agreement template failed for {request.PartnerCode}."
                        );
        }      
    }    
}
