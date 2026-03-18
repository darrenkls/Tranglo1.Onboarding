using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class DeleteCustomerVerificationDocumentCommand : BaseCommand<Result<CustomerVerificationDocumentOutputDTO>>
    {
        public long? CustomerVerificationDocumentCode { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<CustomerVerificationDocumentOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Delete Documents for CUstomer Verification Document Code: [{this.CustomerVerificationDocumentCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class DeleteCustomerVerificationDocumentCommandHandler : IRequestHandler<DeleteCustomerVerificationDocumentCommand, Result<CustomerVerificationDocumentOutputDTO>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<DeleteCustomerVerificationDocumentCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;

        public DeleteCustomerVerificationDocumentCommandHandler(IBusinessProfileRepository repository,ILogger<DeleteCustomerVerificationDocumentCommandHandler>logger,
            StorageManager storageManager, TrangloUserManager userManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
        }


        public async Task<Result<CustomerVerificationDocumentOutputDTO>> Handle(DeleteCustomerVerificationDocumentCommand request, CancellationToken cancellationToken)
        {
            var customerVerificationUpload = await _repository.GetCustomerVerificationDocumentsByCustomerVerificationDocumentCode(request.CustomerVerificationDocumentCode);

            try
            {
                if (customerVerificationUpload == null)
                {
                    return Result.Failure<CustomerVerificationDocumentOutputDTO>("Customer verification upload not found.");

                }
                // Retrieve all document IDs associated with the customer verification upload
                var documentIds = new List<Guid?>()
                {
                    customerVerificationUpload.RawDocumentID,
                    customerVerificationUpload.WatermarkDocumentID
                };

                // Perform the removal for each document ID
                foreach (var documentId in documentIds)
                {
                    if (documentId.HasValue)
                    {
                        // Perform the removal
                        await _storageManager.RemoveAsync(documentId.Value);

                        // Verify the deletion by attempting to retrieve the document metadata
                        var deletedDocument = await _storageManager.GetDocumentMetadataAsync(documentId.Value);

                        if (deletedDocument != null)
                        {
                            // The document removal failed
                            return Result.Failure<CustomerVerificationDocumentOutputDTO>("Failed to delete document.");
                        }
                    }
                }

                // Delete the entire row from the repository
                var result = await _repository.DeleteCustomerVerificationDocumentAsync(request.CustomerVerificationDocumentCode);

                if (result != null)
                {
                    return Result.Success<CustomerVerificationDocumentOutputDTO>(new CustomerVerificationDocumentOutputDTO());
                }
                else
                {
                    return Result.Failure<CustomerVerificationDocumentOutputDTO>("Failed to delete document.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteCustomerVerificationDocumentCommandHandler: {ex.StackTrace}", ex.Message);
                return Result.Failure<CustomerVerificationDocumentOutputDTO>("An error occurred while deleting the document.");
            }
        }

    }
}

