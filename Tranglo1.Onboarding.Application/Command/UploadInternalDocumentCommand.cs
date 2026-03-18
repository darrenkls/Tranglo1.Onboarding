using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;
using Environment = System.Environment;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_InternalDocument_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class UploadInternalDocumentCommand : BaseCommand<Result<string>>
    {
        public int BusinessProfileCode { get; set; }
        public CustomerIdentity.Infrastructure.Services.UserType UserType { get; set; }
        public long RequestId { get; set; }
        public string LoginId { get; set; }
        public List<IFormFile> uploadedFile { get; set; }


        public override Task<string> GetAuditLogAsync(Result<string> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Upload Internal Document for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

    }
    internal class UploadInternalDocumentCommandHandler : IRequestHandler<UploadInternalDocumentCommand, Result<string>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<UploadInternalDocumentCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        // private readonly TrangloUserManager _userManager;


        public UploadInternalDocumentCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<UploadInternalDocumentCommandHandler> logger,
                                                      StorageManager storageManager,
                                                      TrangloUserManager userManager,
                                                      PartnerService partnerService
                                                      // TrangloUserManager userManager

                                                      )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
            // _userManager = userManager;
            _userManager = userManager;
            _partnerService = partnerService;
        }

        public async Task<Result<string>> Handle(UploadInternalDocumentCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            try
            {
                var documentErrorMessage = "";
                foreach (var multipleFiles in request.uploadedFile)
                {
                    var extension = Path.GetExtension(multipleFiles.FileName);
                    var allowedExtensions = new[] { ".jpg", ".JPG", ".jpeg", ".JPEG", ".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".PNG", ".zip" };

                    var fileSize = multipleFiles.Length;
                    int maxFileSizeMB = 30;
                    var maxFileSize = maxFileSizeMB * 1024 * 1024;

                    if (fileSize > maxFileSize)
                    {
                        _logger.LogError($"File {multipleFiles.FileName} exceeds the maximum file size allocated.");
                        documentErrorMessage = (documentErrorMessage + Environment.NewLine + $"Document {multipleFiles.FileName} current length is {multipleFiles.Length}mb which exceeds the current file upload size limit of {maxFileSizeMB}mb");
                        continue;
                    }

                    if (!allowedExtensions.Contains(extension))
                    {
                        _logger.LogError($"File {multipleFiles.FileName} extensions is not allowed.");
                        documentErrorMessage = (documentErrorMessage + Environment.NewLine + $"Document {multipleFiles.FileName} extension is not allowed");
                        continue;
                    }

                    using (var ms = new MemoryStream())
                    {

                        multipleFiles.CopyTo(ms);
                        ms.Position = 0;
                        var doc = await _storageManager.StoreAsync(ms, multipleFiles.FileName, multipleFiles.ContentType);
                        if (doc != null)
                        {
                            {
                                InternalDocumentUpload internalDocument = new InternalDocumentUpload()
                                {
                                    DocumentId = doc.DocumentId,
                                    BusinessProfileCode = request.BusinessProfileCode,
                                    IsDisplay = true,
                                    IsRemoved = false
                                };

                                await _businessProfileService.AddInternalDocumentUploadAsync(internalDocument);

                            }
                        }
                    }
                }

                if (documentErrorMessage != string.Empty)
                {
                    return Result.Failure<string>(documentErrorMessage);
                }
                return Result.Success<string>("Document Successfully Uploaded");

            }
            catch (Exception ex)
            {
                _logger.LogError($"[UploadInternalDocumentsStackTrace: {ex.StackTrace}", ex.Message);
            }

            return Result.Failure<string>("Unable to Upload Internal Document.");

        }

    }
}
