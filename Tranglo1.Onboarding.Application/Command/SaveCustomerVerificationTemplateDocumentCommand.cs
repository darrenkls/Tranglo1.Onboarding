using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class SaveCustomerVerificationTemplateDocumentCommand : BaseCommand<Result<CustomerVerificationDocumentTemplatesOutputDTO>>
    {
        public long? CustomerVerificationCode { get; set; }
        public IFormFile UploadedFile { get; set; }

        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<CustomerVerificationDocumentTemplatesOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string description = $"Adding/Updating Verification Document for Customer Verification Code: [{this.CustomerVerificationCode}]";
                return Task.FromResult(description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveCustomerVerificationTemplateDocumentCommandHandler : IRequestHandler<SaveCustomerVerificationTemplateDocumentCommand, Result<CustomerVerificationDocumentTemplatesOutputDTO>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<SaveCustomerVerificationTemplateDocumentCommandHandler> _logger;
        private readonly StorageManager _storageManager;

        public SaveCustomerVerificationTemplateDocumentCommandHandler(IBusinessProfileRepository repository, ILogger<SaveCustomerVerificationTemplateDocumentCommandHandler> logger, StorageManager storageManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
        }


        public async Task<Result<CustomerVerificationDocumentTemplatesOutputDTO>> Handle(SaveCustomerVerificationTemplateDocumentCommand request, CancellationToken cancellationToken)
        {
            var customerVerification = await _repository.GetCustomerVerificationbyCustomerVerificationCodeAsync(request.CustomerVerificationCode);

            if (customerVerification == null)
            {
                return Result.Failure<CustomerVerificationDocumentTemplatesOutputDTO>("Invalid Customer Verification Code");
            }

            if (request.UploadedFile == null || request.UploadedFile.Length == 0)
            {
                return Result.Failure<CustomerVerificationDocumentTemplatesOutputDTO>("No files were uploaded.");
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    await request.UploadedFile.CopyToAsync(ms);
                    ms.Position = 0;

                    var doc = await _storageManager.StoreAsync(ms, request.UploadedFile.FileName, request.UploadedFile.ContentType);

                    if (doc == null)
                    {
                        return Result.Failure<CustomerVerificationDocumentTemplatesOutputDTO>("An error occurred during file upload.");
                    }

                    customerVerification.TemplateID = doc.DocumentId;

                    var addedDocuments = await _repository.UpdateCustomerVerificationAsync(customerVerification);

                    CustomerVerificationDocumentTemplatesOutputDTO outputDTOs = new CustomerVerificationDocumentTemplatesOutputDTO()
                    {
                        CustomerVerificationCode = customerVerification.Id,
                        TemplateID = addedDocuments.TemplateID
                    };

                    return Result.Success<CustomerVerificationDocumentTemplatesOutputDTO>(outputDTOs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UploadCommentDocumentsStackTrace: {ex.StackTrace}", ex.Message);
                return Result.Failure<CustomerVerificationDocumentTemplatesOutputDTO>("An error occurred during file upload.");
            }
        }
    }
}
