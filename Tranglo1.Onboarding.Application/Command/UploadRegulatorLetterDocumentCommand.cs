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
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class UploadRegulatorLetterDocumentCommand : BaseCommand<Result<UploadRegulatorLetterDocumentOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public IFormFile UploadedFile { get; set; }

        public override Task<string> GetAuditLogAsync(Result<UploadRegulatorLetterDocumentOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Upload Regulator Letter Document for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UploadRegulatorLetterDocumentCommandHandler : IRequestHandler<UploadRegulatorLetterDocumentCommand, Result<UploadRegulatorLetterDocumentOutputDTO>>
    {
        private readonly ILogger<UploadRegulatorLetterDocumentCommand> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public UploadRegulatorLetterDocumentCommandHandler(ILogger<UploadRegulatorLetterDocumentCommand> logger, StorageManager storageManager,
                                                      TrangloUserManager userManager, IBusinessProfileRepository businessProfileRepository)
        {
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<UploadRegulatorLetterDocumentOutputDTO>> Handle(UploadRegulatorLetterDocumentCommand request, CancellationToken cancellationToken)
        {
            var licenseInformation = await _businessProfileRepository.GetLicenseInfoByBusinessCode(request.BusinessProfileCode);
            if (licenseInformation is null)
            {
                return Result.Failure<UploadRegulatorLetterDocumentOutputDTO>($"License Information for BusinessProfileCode: {request.BusinessProfileCode} does not exist.");
            }

            //Add new document & has existing document
            else if (licenseInformation.RegulatorDocumentId.HasValue && !(request.UploadedFile is null))
            {                
                try
                {
                    //1) Delete existing document
                    licenseInformation = await DeleteDocumentAsync(licenseInformation);

                    //2) Add new document
                    var result = await AddDocumentAsync(licenseInformation, request.UploadedFile);
                    licenseInformation = result.Value;
                }
                catch (Exception ex)
                {
                    return Result.Failure<UploadRegulatorLetterDocumentOutputDTO>($"Failed to add document. {ex.Message}");
                }                
            }

            //Delete existing document
            else if (licenseInformation.RegulatorDocumentId.HasValue && request.UploadedFile is null)
            {
                try
                {
                    licenseInformation = await DeleteDocumentAsync(licenseInformation);
                }
                catch (Exception ex)
                {
                    return Result.Failure<UploadRegulatorLetterDocumentOutputDTO>($"Deletion of existing DocumentId: {licenseInformation.RegulatorDocumentId} failed. {ex.Message}");
                }
            }

            //Add new document & no existing documentUploadRegulatorLetterDocumentOutputDTO
            else if (licenseInformation.RegulatorDocumentId is null && !(request.UploadedFile is null))
            {
                try
                {
                    var result = await AddDocumentAsync(licenseInformation, request.UploadedFile);
                    if (result.IsSuccess)
                    {
                        licenseInformation = result.Value;
                    }                    
                }
                catch (Exception ex)
                {
                    return Result.Failure<UploadRegulatorLetterDocumentOutputDTO>($"Failed to add document. {ex.Message}");
                }
            }

            var outputDTO = new UploadRegulatorLetterDocumentOutputDTO();
            outputDTO.BusinessProfileCode = request.BusinessProfileCode;
            outputDTO.DocumentId = licenseInformation.RegulatorDocumentId;
            outputDTO.DocumentName = licenseInformation.RegulatorDocumentName;

            return Result.Success(outputDTO);
        }

        public async Task<LicenseInformation> DeleteDocumentAsync(LicenseInformation licenseInformation)
        {
            try
            {
                var document = await _storageManager.GetDocumentMetadataAsync(licenseInformation.RegulatorDocumentId.Value);
                await _storageManager.RemoveAsync(document.DocumentId);
                licenseInformation.RegulatorDocumentName = null;
                licenseInformation.RegulatorDocumentId = null;
                var update = await _businessProfileRepository.UpdateLicenseInformationsAsync(licenseInformation);
                licenseInformation = update.Value;

                return licenseInformation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Result<LicenseInformation>> AddDocumentAsync(LicenseInformation licenseInformation, IFormFile uploadedFile)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var extension = Path.GetExtension(uploadedFile.FileName)?.ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".doc", ".docx", ".pdf", ".png",".xls",".xlsx",".zip" };
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
                        licenseInformation.RegulatorDocumentName = result.FileName;
                        licenseInformation.RegulatorDocumentId   = result.DocumentId;
                        var update = await _businessProfileRepository.UpdateLicenseInformationsAsync(licenseInformation);
                        licenseInformation = update.Value;
                    }
                }

                return licenseInformation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}