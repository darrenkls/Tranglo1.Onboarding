using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class DeleteDeclarationSignatureCommand : BaseCommand<Result<DeclarationOutputDTO>>
    {
        public long? DeclarationCode { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<DeclarationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Delete Documents for Business User Declaration Code: [{this.DeclarationCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class DeleteDeclarationSignatureCommandHandler : IRequestHandler<DeleteDeclarationSignatureCommand, Result<DeclarationOutputDTO>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<DeleteDeclarationSignatureCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;

        public DeleteDeclarationSignatureCommandHandler(
            IBusinessProfileRepository repository,
            ILogger<DeleteDeclarationSignatureCommandHandler> logger,
            StorageManager storageManager,
            TrangloUserManager userManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
        }

        public async Task<Result<DeclarationOutputDTO>> Handle(DeleteDeclarationSignatureCommand request, CancellationToken cancellationToken)
        {
            Result<DeclarationOutputDTO> result = null;

            var userDeclaration = await _repository.GetCustomerConnectDeclarationByDeclarationCode(request.DeclarationCode);

            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    result = await DeleteDeclarationSignature(request, userDeclaration);

                    if (result.IsFailure)
                    {
                        return Result.Failure<DeclarationOutputDTO>(
                            $"Customer user is unable to update for  Connect User Declaration {request.DeclarationCode}. {result.Error}"
                        );
                    }

                    
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    return Result.Failure<DeclarationOutputDTO>(
                          $"Connect Customer user is unable to update for Connect User Declaration {request.DeclarationCode}."
                      );
                }
                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    result = await DeleteDeclarationSignature(request, userDeclaration);

                    if (result.IsFailure)
                    {
                        return Result.Failure<DeclarationOutputDTO>(
                            $"Admin user is unable to update for Connect User Declaration {request.DeclarationCode}. {result.Error}"
                        );
                    }

                }
                else if (Solution.Business.Id == request.AdminSolution)
                {

                    return Result.Failure<DeclarationOutputDTO>(
                        $"Admin user is unable to update for Connect User  Declaration: {request.DeclarationCode}."
                    );
                }
                else
                {
                    return Result.Failure<DeclarationOutputDTO>(
                        $"Unable to update for BusinessProfileCode {request.DeclarationCode}."
                    );
                }

                return result;
            }
            else
            {

                return Result.Failure<DeclarationOutputDTO>("Invalid request");
            }
        }

        private async Task<Result<DeclarationOutputDTO>> DeleteDeclarationSignature(DeleteDeclarationSignatureCommand request, Declaration userDeclaration)
        {
            try
            {
                if (userDeclaration == null)
                {
                    return Result.Failure<DeclarationOutputDTO>("Business User file upload not found.");
                }

                var documentId = userDeclaration.DocumentId.Value;
                // Perform the removal
                await _storageManager.RemoveAsync(documentId);

                // Verify the deletion by attempting to retrieve the document metadata
                var deletedDocument = await _storageManager.GetDocumentMetadataAsync(documentId);

                if (deletedDocument != null)
                {
                    // The document removal failed
                    return Result.Failure<DeclarationOutputDTO>("Failed to delete document.");
                }

                var result = await _repository.DeleteConnectUserDeclarationSignatureAsync(documentId);
                if (result != null)
                {
                    return Result.Success<DeclarationOutputDTO>(
                        new DeclarationOutputDTO() 
                        { 
                        BusinessProfileCode = userDeclaration.BusinessProfile.Id,
                        DeclarationCode = userDeclaration.Id,
                        isAuthorized = userDeclaration.IsAuthorized,
                        isInformationTrue = userDeclaration.IsInformationTrue,
                        isAgreeTermsOfService = userDeclaration.IsAgreedTermsOfService,
                        isDeclareTransactionTax = userDeclaration.IsDeclareTransactionTax,
                        SigneeName = userDeclaration.SigneeName,
                        Designation = userDeclaration.Designation,
                        DocumentId = userDeclaration.DocumentId ?? null
                        });
                }
                else
                {
                    return Result.Failure<DeclarationOutputDTO>("Failed to delete document.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteDeclarationSignatureCommandHandler: {ex.StackTrace}", ex.Message);
                return Result.Failure<DeclarationOutputDTO>("An error occurred while deleting the document.");
            }


        }
    }
}
