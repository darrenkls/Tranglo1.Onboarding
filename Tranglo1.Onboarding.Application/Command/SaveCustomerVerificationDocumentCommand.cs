using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification;
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
    internal class SaveCustomerVerificationDocumentCommand : BaseCommand<Result<CustomerVerificationDocumentOutputDTO>>
    {
        public long? CustomerVerificationCode { get; set; }
        public long? VerificationIDTypeSectionCode { get; set; }
        public IFormFile UploadedFile { get; set; }

        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<CustomerVerificationDocumentOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Adding/Updating Verification Document for Customer Verification Code: [{this.CustomerVerificationCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveCustomerVerificationDocumentCommandHandler : IRequestHandler<SaveCustomerVerificationDocumentCommand, Result<CustomerVerificationDocumentOutputDTO>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<SaveCustomerVerificationDocumentCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly StorageManager _storageManager;

        public SaveCustomerVerificationDocumentCommandHandler(IBusinessProfileRepository repository, ILogger<SaveCustomerVerificationDocumentCommandHandler> logger,
            TrangloUserManager userManager, StorageManager storageManager)
        {
            _repository = repository;
            _logger = logger;
            _userManager = userManager;
            _storageManager = storageManager;
        }

        public async Task<Result<CustomerVerificationDocumentOutputDTO>> Handle(SaveCustomerVerificationDocumentCommand request, CancellationToken cancellationToken)
        {
            var customerVerification = await _repository.GetCustomerVerificationbyCustomerVerificationCodeAsync(request.CustomerVerificationCode);

            if (customerVerification == null)
            {
                return Result.Failure<CustomerVerificationDocumentOutputDTO>("Invalid Customer Verification Code");
            }

            var verificationIDType = await _repository.GetVerificationIDByCodeAsync(customerVerification.VerificationIDType.Id);

            if (verificationIDType == null)
            {
                return Result.Failure<CustomerVerificationDocumentOutputDTO>("Invalid Verification ID Type");
            }

            var verificationIDTypeSections = await _repository.GetVerificationIDTypeSectionsByCode(request.VerificationIDTypeSectionCode);

            if (verificationIDTypeSections == null)
            {
                return Result.Failure<CustomerVerificationDocumentOutputDTO>("Invalid Verification ID Type Section Code");
            }

            if (request.UploadedFile == null || request.UploadedFile.Length == 0)
            {
                return Result.Failure<CustomerVerificationDocumentOutputDTO>("No files were uploaded.");
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
                            return Result.Failure<CustomerVerificationDocumentOutputDTO>("Incorrect file resolution uploaded. Please reupload.");
                        }

                        if (fileSize > maxFileSize)
                        {
                            return Result.Failure<CustomerVerificationDocumentOutputDTO>($"Document file size exceeds {maxFileSizeMB} MB");
                        }

                        if (!allowedExtensions.Contains(extension))
                        {
                            return Result.Failure<CustomerVerificationDocumentOutputDTO>("Document extension is not allowed");
                        }

                        var doc = await _storageManager.StoreAsync(ms, request.UploadedFile.FileName, request.UploadedFile.ContentType);

                        if (doc == null)
                        {
                            return Result.Failure<CustomerVerificationDocumentOutputDTO>("An error occurred during file upload.");
                        }

                        var customerVerificationDocumentsUpload = new CustomerVerificationDocuments(
                            customerVerification,
                            doc.DocumentId,
                            doc.FileName,
                            null,
                            null,
                            verificationIDTypeSections,
                            null
                        );

                        var addedDocuments = await _repository.AddCustomerVerificationDocumentsAsync(customerVerificationDocumentsUpload);

                        CustomerVerificationDocumentOutputDTO outputDTOs = new CustomerVerificationDocumentOutputDTO()
                        {
                            CustomerVerificationDocumentCode = addedDocuments.Id,
                            RawDocumentID = addedDocuments.RawDocumentID,
                            RawDocumentName = addedDocuments.RawDocumentName,
                            VerificationIDTypeSectionCode = addedDocuments.VerificationIDTypeSection.Id,
                            VerificationIDTypeSectionDescription = addedDocuments.VerificationIDTypeSection.Description,
                            VerificationIDType = addedDocuments.VerificationIDTypeSection.VerificationIDType.Id,
                            VerificationIDTypeDescription = addedDocuments.VerificationIDTypeSection.VerificationIDType.Name
                        };

                        return Result.Success<CustomerVerificationDocumentOutputDTO>(outputDTOs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UploadCommentDocumentsStackTrace: {ex.StackTrace}", ex.Message);
                return Result.Failure<CustomerVerificationDocumentOutputDTO>("An error occurred during file upload.");
            }
        }
    }
}
