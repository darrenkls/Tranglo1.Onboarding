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
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementVerification.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementVerification.Action_View_Code })]
    internal class SaveCustomerVerificationWatermarkDocumentCommand : BaseCommand<Result<CustomerVerificationWatermarkDocumentOutputDTO>>
    {
        public long? CustomerVerificationDocumentCode { get; set; }
        public IFormFile UploadedFile { get; set; }

        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<CustomerVerificationWatermarkDocumentOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Adding/Updating Verification Document for Customer Verification Code: [{this.CustomerVerificationDocumentCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveCustomerVerificationWatermarkDocumentCommandHandler : IRequestHandler<SaveCustomerVerificationWatermarkDocumentCommand, Result<CustomerVerificationWatermarkDocumentOutputDTO>>
    {

        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<SaveCustomerVerificationWatermarkDocumentCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly StorageManager _storageManager;

        public SaveCustomerVerificationWatermarkDocumentCommandHandler(IBusinessProfileRepository repository, ILogger<SaveCustomerVerificationWatermarkDocumentCommandHandler>logger,
            TrangloUserManager userManager, StorageManager storageManager)
        {
            _repository = repository;
            _logger = logger;
            _userManager = userManager;
            _storageManager = storageManager;
        }
        public async Task<Result<CustomerVerificationWatermarkDocumentOutputDTO>> Handle(SaveCustomerVerificationWatermarkDocumentCommand request, CancellationToken cancellationToken)
        {
            var customerVerificationDocuments = await _repository.GetCustomerVerificationDocumentsByCustomerVerificationDocumentCode(request.CustomerVerificationDocumentCode);

            if (customerVerificationDocuments == null)
            {
                return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("Invalid Customer Verification Document Code");
            }
            if (request.UploadedFile == null || request.UploadedFile.Length == 0)
            {
                return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("No files were uploaded.");
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    await request.UploadedFile.CopyToAsync(ms);
                    ms.Position = 0;

                    var imageBytes = ms.ToArray();
                    var imageStream = new MemoryStream(imageBytes);

                    // Use alternative methods to get image width and height
                    using (var image = System.Drawing.Image.FromStream(imageStream))
                    {
                        var width = image.Width;
                        var height = image.Height;

                        var extension = Path.GetExtension(request.UploadedFile.FileName)?.ToLower();
                        var allowedExtensions = new[] { ".jpg", ".png" };
                        int maxFileSizeMB = 5;
                        var maxFileSize = maxFileSizeMB * 1024 * 1024;
                        var fileSize = request.UploadedFile.Length;

                        const int maxResolution = 8000;

                        if (width > maxResolution || height > maxResolution)
                        {
                            return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("Incorrect file resolution uploaded. Please reupload.");
                        }

                        if (fileSize > maxFileSize)
                        {
                            return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>($"Document file size exceeds {maxFileSizeMB} MB");
                        }

                        if (!allowedExtensions.Contains(extension))
                        {
                            return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("Document extension is not allowed");
                        }

                        var doc = await _storageManager.StoreAsync(ms, request.UploadedFile.FileName, request.UploadedFile.ContentType);

                        if (doc == null)
                        {
                            return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("An error occurred during file upload.");
                        }

                        if (customerVerificationDocuments == null)
                        {
                            return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("Customer verification documents not found.");
                        }

                        customerVerificationDocuments.WatermarkDocumentID = doc.DocumentId;
                        customerVerificationDocuments.WatermarkDocumentName = doc.FileName;

                        var updatedDocuments = await _repository.UpdateCustomerVerificationDocumentsAsync(customerVerificationDocuments);


                        CustomerVerificationWatermarkDocumentOutputDTO outputDTOs = new CustomerVerificationWatermarkDocumentOutputDTO()
                        {
                            CustomerVerificationDocumentCode = updatedDocuments.Id,
                            WatermarkDocumentID = updatedDocuments.WatermarkDocumentID,
                            WatermarkDocumentName = updatedDocuments.WatermarkDocumentName,
                            VerificationIDTypeSectionCode = updatedDocuments.VerificationIDTypeSection.Id,
                            VerificationIDTypeSectionDescription = updatedDocuments.VerificationIDTypeSection.Description,
                            VerificationIDType = updatedDocuments.VerificationIDTypeSection.VerificationIDType.Id,
                            VerificationIDTypeDescription = updatedDocuments.VerificationIDTypeSection.VerificationIDType.Name
                        };

                        return Result.Success<CustomerVerificationWatermarkDocumentOutputDTO>(outputDTOs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UploadCommentDocumentsStackTrace: {ex.StackTrace}", ex.Message);
                return Result.Failure<CustomerVerificationWatermarkDocumentOutputDTO>("An error occurred during file upload.");
            }

        }
    }
}
