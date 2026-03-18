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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class AddBusinessUserDeclarationSignatureCommand : BaseCommand<Result<BusinessUserDeclarationSignatureOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public IFormFile uploadedFile { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }


        public override Task<string> GetAuditLogAsync(Result<BusinessUserDeclarationSignatureOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Business User Declarations Signature for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class AddBusinessUserDeclarationSignatureCommandHandler : IRequestHandler<AddBusinessUserDeclarationSignatureCommand, Result<BusinessUserDeclarationSignatureOutputDTO>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<AddBusinessUserDeclarationSignatureCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        public AddBusinessUserDeclarationSignatureCommandHandler(BusinessProfileService businessProfileService,
                                                    IBusinessProfileRepository repository,
                                                      ILogger<AddBusinessUserDeclarationSignatureCommandHandler> logger,
                                                      StorageManager storageManager,
                                                      TrangloUserManager userManager,
                                                      PartnerService partnerService
                                                      )
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
            _partnerService = partnerService;
        }

        public async Task<Result<BusinessUserDeclarationSignatureOutputDTO>> Handle(AddBusinessUserDeclarationSignatureCommand request, CancellationToken cancellationToken)
        {
            var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
            var businessUserDeclaration = await _repository.GetBusinessUserDeclarationByBusinessProfileCodeAsync(businessProfile.Id);

            Result<BusinessUserDeclarationSignatureOutputDTO> result = null; // Declare the result variable here

            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>(
                        $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                    );
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    result = await UploadBusinessUserDeclarationSignature(request, businessProfile, businessUserDeclaration); // Call UpdateCustomerVerification with null customerVerification

                    if (result.IsFailure)
                    {
                        return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>(
                            $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>(
                        $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                    );
                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    result = await UploadBusinessUserDeclarationSignature(request, businessProfile, businessUserDeclaration); // Call UpdateCustomerVerification with null customerVerification

                    if (result.IsFailure)
                    {
                        return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>(
                            $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else
                {
                    return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>(
                        $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                    );
                }

                return result;
            }
            else
            {

                return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>("Invalid request");
            }
        }
        private async Task<Result<BusinessUserDeclarationSignatureOutputDTO>> UploadBusinessUserDeclarationSignature(AddBusinessUserDeclarationSignatureCommand request, BusinessProfile businessProfile, BusinessUserDeclaration businessUserDeclaration)
        {
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            try
            {
                using (var ms = new MemoryStream())
                {
                    var extension = Path.GetExtension(request.uploadedFile.FileName)?.ToLower();
                    var allowedExtensions = new[] { ".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".zip", ".jpg", ".jpeg", ".JPG", ".JPEG" };

                    var fileSize = request.uploadedFile.Length;
                    int maxFileSizeMB = 15;
                    var maxFileSize = maxFileSizeMB * 1024 * 1024;

                    if (fileSize > maxFileSize)
                    {
                        return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>($"Document file size exceeds {maxFileSizeMB}mb");
                    }
                    if (!allowedExtensions.Contains(extension))
                    {
                        return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>("Document extension is not allowed");
                    }

                    // Check if there is an existing document for the user
                    var businessDeclaration = await _repository.GetBusinessUserDeclarationByBusinessProfileCodeAsync(request.BusinessProfileCode);
                    if (businessDeclaration != null && businessDeclaration.DocumentId != null)
                    {
                        // Perform the removal
                        await _storageManager.RemoveAsync(businessDeclaration.DocumentId.Value);

                        // Verify the deletion by attempting to retrieve the document metadata
                        var deletedDocument = await _storageManager.GetDocumentMetadataAsync(businessDeclaration.DocumentId.Value);

                        if (deletedDocument != null)
                        {
                            // The document removal failed
                            return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>("Failed to delete document.");
                        }
                        // User re-uploaded a new document, update the DocumentId
                        var result = await _repository.DeleteBusinessUserDeclarationSignatureAsync(businessDeclaration.DocumentId);

                    }

                    await request.uploadedFile.CopyToAsync(ms);
                    ms.Position = 0;
                    var doc = await _storageManager.StoreAsync(ms, request.uploadedFile.FileName, request.uploadedFile.ContentType);
                    if (doc != null)
                    {

                        if (businessDeclaration == null)
                        {
                            if (businessProfile != null)
                            {
                                var newBusinessDeclaration = new BusinessUserDeclaration(
                                businessProfile.Id,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                doc.DocumentId,
                                    null,
                                    null,
                                    null);
                              

                                var result = await _repository.AddBusinessUserDeclarationAsync(newBusinessDeclaration);

                               
                                return Result.Success<BusinessUserDeclarationSignatureOutputDTO>(new BusinessUserDeclarationSignatureOutputDTO
                                {
                                    BusinessProfileCode = businessProfile.Id,
                                    BusinessUserDeclarationCode = newBusinessDeclaration.Id,
                                    DocumentId = doc.DocumentId,
                                    FileName = doc.FileName
                                });
                                                       
                            }
                        }
                        else if(businessDeclaration != null && businessDeclaration.DocumentId == null)
                        {
                            businessDeclaration.DocumentId = doc.DocumentId;

                            var result = await _repository.UpdateBusinessUserDeclarationAsync(businessUserDeclaration);

                            return Result.Success<BusinessUserDeclarationSignatureOutputDTO>(new BusinessUserDeclarationSignatureOutputDTO
                            {
                                BusinessProfileCode = businessProfile.Id,
                                BusinessUserDeclarationCode = businessDeclaration.Id,
                                DocumentId = doc.DocumentId,
                                FileName = doc.FileName
                            });

                            
                        }
                        else
                        {
                            // If the document is null, handle the failure scenario
                            return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>("Unable to upload signature.");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"[AddDeclarationSignatureCommand] {ex.Message}");
            }

            return Result.Failure<BusinessUserDeclarationSignatureOutputDTO>("Unable to upload signature.");
        }


    }
    
}
