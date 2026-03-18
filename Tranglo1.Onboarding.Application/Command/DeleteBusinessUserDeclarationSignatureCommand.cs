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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class DeleteBusinessUserDeclarationSignatureCommand : BaseCommand<Result<GetBusinessUserDeclarationOutputDTO>>
    {
        public long? BusinessUserDeclarationCode { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<GetBusinessUserDeclarationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Delete Documents for Business User Declaration Code: [{this.BusinessUserDeclarationCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class DeleteBusinessUserDeclarationSignatureCommandHandler : IRequestHandler<DeleteBusinessUserDeclarationSignatureCommand, Result<GetBusinessUserDeclarationOutputDTO>>
    {

        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<DeleteBusinessUserDeclarationSignatureCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;

        public DeleteBusinessUserDeclarationSignatureCommandHandler(
            IBusinessProfileRepository repository,
            ILogger<DeleteBusinessUserDeclarationSignatureCommandHandler>logger,
            StorageManager storageManager,
            TrangloUserManager userManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
        }

        public async Task<Result<GetBusinessUserDeclarationOutputDTO>> Handle(DeleteBusinessUserDeclarationSignatureCommand request, CancellationToken cancellationToken)
        {
            Result<GetBusinessUserDeclarationOutputDTO> result = null;

            var businessUserDeclaration = await _repository.GetBusinessUserDeclarationByBusinessUserDeclarationCode(request.BusinessUserDeclarationCode);

            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                        $"Connect Customer user is unable to update for Business User Declaration {request.BusinessUserDeclarationCode}."
                    );
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    result = await DeleteBusinessUserDeclarationSignature(request, businessUserDeclaration); 

                    if (result.IsFailure)
                    {
                        return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                            $"Customer user is unable to update for  Business User Declaration{request.BusinessUserDeclarationCode}. {result.Error}"
                        );
                    }
                }
                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                        $"Admin user is unable to update for Connect User to Business User Declaration: {request.BusinessUserDeclarationCode}."
                    );
                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    result = await DeleteBusinessUserDeclarationSignature(request, businessUserDeclaration); 

                    if (result.IsFailure)
                    {
                        return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                            $"Admin user is unable to update for Business User Declaration{request.BusinessUserDeclarationCode}. {result.Error}"
                        );
                    }
                }
                else
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                        $"Unable to update for BusinessProfileCode {request.BusinessUserDeclarationCode}."
                    );
                }

                return result;
            }
            else
            {

                return Result.Failure<GetBusinessUserDeclarationOutputDTO>("Invalid request");
            }
        }

        private async Task<Result<GetBusinessUserDeclarationOutputDTO>> DeleteBusinessUserDeclarationSignature(DeleteBusinessUserDeclarationSignatureCommand request, BusinessUserDeclaration businessUserDeclaration)
        {
            try
            {
                if (businessUserDeclaration == null)
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>("Business User file upload not found.");
                }

                var documentId = businessUserDeclaration.DocumentId.Value;
                // Perform the removal
                await _storageManager.RemoveAsync(documentId);

                // Verify the deletion by attempting to retrieve the document metadata
                var deletedDocument = await _storageManager.GetDocumentMetadataAsync(documentId);

                if (deletedDocument != null)
                {
                    // The document removal failed
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>("Failed to delete document.");
                }

                var result = await _repository.DeleteBusinessUserDeclarationSignatureAsync(documentId);
                if (result != null)
                {
                    return Result.Success<GetBusinessUserDeclarationOutputDTO>(new GetBusinessUserDeclarationOutputDTO() 
                    {
                        BusinessProfileCode = businessUserDeclaration.BusinessProfileCode,
                        BusinessUserDeclarationCode = businessUserDeclaration.Id,
                        isNotRemittancePartner = businessUserDeclaration.IsAuthorized,
                        isAuthorized = businessUserDeclaration.IsAuthorized,
                        isInformationTrue = businessUserDeclaration.IsInformationTrue,
                        isAgreeTermsOfService = businessUserDeclaration.IsAgreedTermsOfService,
                        isDeclareTransactionTax = businessUserDeclaration.IsDeclareTransactionTax,
                        SigneeName = businessUserDeclaration.SigneeName,
                        Designation = businessUserDeclaration.Designation,
                        DocumentId = businessUserDeclaration.DocumentId ?? null
                    });
                }
                else
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>("Failed to delete document.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteBusinessUserDeclarationSignatureCommandHandler: {ex.StackTrace}", ex.Message);
                return Result.Failure<GetBusinessUserDeclarationOutputDTO>("An error occurred while deleting the document.");
            }


        }

    }

}
