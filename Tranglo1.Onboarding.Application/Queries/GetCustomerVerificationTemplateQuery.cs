using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetCustomerVerificationTemplateQuery : BaseQuery<GetCustomerVerificationDocumentTemplatesOutputDTO>
    {
        public long? CustomerVerificationCode { get; set; }

        public override Task<string> GetAuditLogAsync(GetCustomerVerificationDocumentTemplatesOutputDTO result)
        {
            string _description = $"Get Document Details for Customer Verification Code: [{this.CustomerVerificationCode}]";
            return Task.FromResult(_description);
        }
    }


    internal class GetCustomerVerificationTemplateQueryHandler : IRequestHandler<GetCustomerVerificationTemplateQuery, GetCustomerVerificationDocumentTemplatesOutputDTO>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<GetCustomerVerificationTemplateQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetCustomerVerificationTemplateQueryHandler(IBusinessProfileRepository repository,ILogger<GetCustomerVerificationTemplateQueryHandler> logger,
            StorageManager storageManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<GetCustomerVerificationDocumentTemplatesOutputDTO> Handle(GetCustomerVerificationTemplateQuery request, CancellationToken cancellationToken)
        {
            var customerVerification = await _repository.GetCustomerVerificationbyCustomerVerificationCodeAsync(request.CustomerVerificationCode);
            var template = customerVerification.TemplateID;
            var document = await _storageManager.GetDocumentMetadataAsync(template.Value);

            if (document != null)
            {
                var ms = new MemoryStream();
                await _storageManager.CopyToAsync(document.DocumentId, ms);

                // Check if any data was copied to the MemoryStream
                if (ms.Length > 0)
                {
                    // Set the MemoryStream position to the beginning before assigning it to the File property
                    ms.Position = 0;

                    return new GetCustomerVerificationDocumentTemplatesOutputDTO()
                    {
                        File = ms,
                        ContentType = document.ContentType,
                        FileName = document.FileName,
                        FileSize = ms.Length
                    };
                }
                else
                {
                    return Result.Failure<GetCustomerVerificationDocumentTemplatesOutputDTO>("Failed to retrieve document data.").Value;
                }
            }

            return null;
        }
    }
}

