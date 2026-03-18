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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.BusinessDeclaration;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class UploadBusinessDeclarationDocumentCommand : BaseCommand<Result<UploadBusinessDeclarationDocumentOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public long CustomerBusinessDeclarationAnswerCode { get; set; }
        public IFormFile UploadedFile { get; set; }

        public override Task<string> GetAuditLogAsync(Result<UploadBusinessDeclarationDocumentOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Review Remarks Document for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UploadBusinessDeclarationDocumentCommandHandler : IRequestHandler<UploadBusinessDeclarationDocumentCommand, Result<UploadBusinessDeclarationDocumentOutputDTO>>
    {
        private readonly ILogger<UploadBusinessDeclarationDocumentCommand> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public UploadBusinessDeclarationDocumentCommandHandler(ILogger<UploadBusinessDeclarationDocumentCommand> logger, StorageManager storageManager,
                                                      TrangloUserManager userManager, IBusinessProfileRepository businessProfileRepository)
        {
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<UploadBusinessDeclarationDocumentOutputDTO>> Handle(UploadBusinessDeclarationDocumentCommand request, CancellationToken cancellationToken)
        {
            var customerBusinessDeclarationAnswer = await _businessProfileRepository.GetCustomerBusinessDeclarationAnswerByCodeAsync(request.CustomerBusinessDeclarationAnswerCode);
            if (customerBusinessDeclarationAnswer is null)
            {
                return Result.Failure<UploadBusinessDeclarationDocumentOutputDTO>($"CustomerBusinessDeclarationAnswerCode {request.CustomerBusinessDeclarationAnswerCode} does not exist.");
            }

            //Add new document & has existing document
            else if (customerBusinessDeclarationAnswer.DocumentId.HasValue && !(request.UploadedFile is null))
            {                
                //1) Delete existing document
                try
                {
                    customerBusinessDeclarationAnswer = await DeleteDocumentAsync(customerBusinessDeclarationAnswer);
                }
                catch (Exception ex)
                {
                    return Result.Failure<UploadBusinessDeclarationDocumentOutputDTO>($"Deletion of existing DocumentId: {customerBusinessDeclarationAnswer.DocumentId} failed. {ex.Message}");
                }

                //2) Add new document
                await AddDocumentAsync(customerBusinessDeclarationAnswer, request.UploadedFile);
            }

            //Delete existing document
            else if (customerBusinessDeclarationAnswer.DocumentId.HasValue && request.UploadedFile is null)
            {                
                try
                {
                    customerBusinessDeclarationAnswer = await DeleteDocumentAsync(customerBusinessDeclarationAnswer);
                }
                catch (Exception ex)
                {
                    return Result.Failure<UploadBusinessDeclarationDocumentOutputDTO>($"Deletion of existing DocumentId: {customerBusinessDeclarationAnswer.DocumentId} failed. {ex.Message}");
                }                
            }

            //Add new document & no existing document
            else if (customerBusinessDeclarationAnswer.DocumentId is null && !(request.UploadedFile is null))
            {
                try
                {
                    var result = await AddDocumentAsync(customerBusinessDeclarationAnswer, request.UploadedFile);
                    if (result.IsSuccess)
                    {
                        customerBusinessDeclarationAnswer = result.Value;
                    }
                }
                catch (Exception ex)
                {
                    return Result.Failure<UploadBusinessDeclarationDocumentOutputDTO>($"Failed to add document. {ex.Message}");
                }
            }

            var outputDTO = new UploadBusinessDeclarationDocumentOutputDTO();
            outputDTO.BusinessProfileCode = request.BusinessProfileCode;
            outputDTO.CustomerBusinessDeclarationAnswerCode = request.CustomerBusinessDeclarationAnswerCode;
            outputDTO.DocumentId = customerBusinessDeclarationAnswer.DocumentId;
            outputDTO.DocumentName = customerBusinessDeclarationAnswer.DocumentName;

            return Result.Success(outputDTO);
        }

        public async Task<CustomerBusinessDeclarationAnswer> DeleteDocumentAsync(CustomerBusinessDeclarationAnswer customerBusinessDeclarationAnswer)
        {
            try
            {
                var document = await _storageManager.GetDocumentMetadataAsync(customerBusinessDeclarationAnswer.DocumentId.Value);
                await _storageManager.RemoveAsync(document.DocumentId);
                customerBusinessDeclarationAnswer.DocumentName = null;
                customerBusinessDeclarationAnswer.DocumentId = null;
                customerBusinessDeclarationAnswer = await _businessProfileRepository.UpdateCustomerBusinessDeclarationAnswerAsync(customerBusinessDeclarationAnswer);
            }
            catch (Exception ex)
            {                
                throw ex;
            }

            return customerBusinessDeclarationAnswer;
        }

        public async Task<Result<CustomerBusinessDeclarationAnswer>> AddDocumentAsync(CustomerBusinessDeclarationAnswer customerBusinessDeclarationAnswer, IFormFile uploadedFile)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var extension = Path.GetExtension(uploadedFile.FileName)?.ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".doc", ".docx", ".pdf", ".png" };
                    var fileSize = uploadedFile.Length;
                    int maxFileSizeMB = 30;
                    var maxFileSize = maxFileSizeMB * 1024 * 1024;

                    if (fileSize > maxFileSize)
                    {
                        throw new Exception($"Document file size exceeds {maxFileSizeMB}mb");
                    }
                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception($"Document extension: {extension} is not allowed");
                    }

                    await uploadedFile.CopyToAsync(ms);
                    ms.Position = 0;
                    var result = await _storageManager.StoreAsync(ms, uploadedFile.FileName, uploadedFile.ContentType);
                    if (result != null)
                    {
                        customerBusinessDeclarationAnswer.DocumentName = result.FileName;
                        customerBusinessDeclarationAnswer.DocumentId = result.DocumentId;
                        customerBusinessDeclarationAnswer = await _businessProfileRepository.UpdateCustomerBusinessDeclarationAnswerAsync(customerBusinessDeclarationAnswer);
                    }
                }
                return customerBusinessDeclarationAnswer;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            
        }
    }
}