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
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class DeleteCustomerVerificationTemplateCommand : BaseCommand<Result<CustomerVerificationTemplateOutputDTO>>
    {
        public long? CustomerVerificationCode { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<CustomerVerificationTemplateOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Delete Documents for CUstomer Verification Document Code: [{this.CustomerVerificationCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class DeleteCustomerVerificationTemplateCommandHandler : IRequestHandler<DeleteCustomerVerificationTemplateCommand, Result<CustomerVerificationTemplateOutputDTO>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<DeleteCustomerVerificationTemplateCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;

        public DeleteCustomerVerificationTemplateCommandHandler (IBusinessProfileRepository repository, ILogger<DeleteCustomerVerificationTemplateCommandHandler> logger,
            StorageManager storageManager, TrangloUserManager userManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
        }

        public async Task<Result<CustomerVerificationTemplateOutputDTO>> Handle(DeleteCustomerVerificationTemplateCommand request, CancellationToken cancellationToken)
        {
            var customerVerification = await _repository.GetCustomerVerificationbyCustomerVerificationCodeAsync(request.CustomerVerificationCode);

            try
            {
                if (customerVerification == null)
                {
                    return Result.Failure<CustomerVerificationTemplateOutputDTO>("Customer verification upload not found.");
                }

                if (!customerVerification.TemplateID.HasValue)
                {
                    return Result.Failure<CustomerVerificationTemplateOutputDTO>("No template ID found for the customer verification.");
                }

                var documentId = customerVerification.TemplateID.Value;

                // Perform the removal
                await _storageManager.RemoveAsync(documentId);

                // Verify the deletion by attempting to retrieve the document metadata
                var deletedDocument = await _storageManager.GetDocumentMetadataAsync(documentId);

                if (deletedDocument != null)
                {
                    // The document removal failed
                    return Result.Failure<CustomerVerificationTemplateOutputDTO>("Failed to delete document.");
                }

                // Delete the entire row from the repository
                var result = await _repository.DeleteCustomerVerificationTemplateAsync(request.CustomerVerificationCode);

                if (result != null)
                {
                    return Result.Success<CustomerVerificationTemplateOutputDTO>(new CustomerVerificationTemplateOutputDTO());
                }
                else
                {
                    return Result.Failure<CustomerVerificationTemplateOutputDTO>("Failed to delete document.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteCustomerVerificationDocumentCommandHandler: {ex.StackTrace}", ex.Message);
                return Result.Failure<CustomerVerificationTemplateOutputDTO>("An error occurred while deleting the document.");
            }
        }
    }
}


