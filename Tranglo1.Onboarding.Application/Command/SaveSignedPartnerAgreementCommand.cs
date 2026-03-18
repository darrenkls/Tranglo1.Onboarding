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
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_DocumentRecordsUpload_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]
    internal class SaveSignedPartnerAgreementCommand: BaseCommand<Result<Guid>>
    {
        public long PartnerCode { get; set; }
        public IFormFile UploadedFile { get; set; }
        public CustomerIdentity.Infrastructure.Services.UserType UserType { get; set; }
        public string CustomerSolution { get; set; }
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

    internal class SaveSignedPartnerAgreementCommandHandler : IRequestHandler<SaveSignedPartnerAgreementCommand, Result<Guid>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<SaveSignedPartnerAgreementCommandHandler> _logger;
        private readonly StorageManager _storageManager;

        public SaveSignedPartnerAgreementCommandHandler(PartnerService partnerService, ILogger<SaveSignedPartnerAgreementCommandHandler> logger, StorageManager storageManager)
        {
            _partnerService = partnerService;
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<Result<Guid>> Handle(SaveSignedPartnerAgreementCommand request, CancellationToken cancellationToken)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    request.UploadedFile.CopyTo(ms);
                    ms.Position = 0;
                    var doc = await _storageManager.StoreAsync(ms, request.UploadedFile.FileName, request.UploadedFile.ContentType);

                    if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
                    {
                        SignedPartnerAgreement signedPartnerAgreement = new SignedPartnerAgreement()
                        {
                            PartnerCode = request.PartnerCode,
                            SignedDocumentId = doc.DocumentId,
                            IsDisplay = true,
                            SolutionCode = Solution.Connect.Id,
                            IsRemoved = false
                        };

                        await _partnerService.AddSignedPartnerAgreementUploadAsync(signedPartnerAgreement);
                        return Result.Success<Guid>(doc.DocumentId);
                    }
                    if (ClaimCode.Business == request.CustomerSolution || Solution.Business.Id == request.AdminSolution)
                    {
                        SignedPartnerAgreement signedPartnerAgreement = new SignedPartnerAgreement()
                        {
                            PartnerCode = request.PartnerCode,
                            SignedDocumentId = doc.DocumentId,
                            IsDisplay = true,
                            SolutionCode = Solution.Business.Id,
                            IsRemoved = false
                        };

                        await _partnerService.AddSignedPartnerAgreementUploadAsync(signedPartnerAgreement);
                        return Result.Success<Guid>(doc.DocumentId);
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"[SaveSignedPartnerAgreementCommand] {ex.Message}");
            }
            return Result.Failure<Guid>(
                            $"Unable to upload signed partner agreement for {request.PartnerCode}."
                        );
        }
    }
}