using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.BusinessDeclaration;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class DownloadDeclarationDocumentQuery : BaseQuery<DownloadDeclarationDocumentOutputDTO>
    {
        public long CustomerBusinessDeclarationAnswerCode { get; set; }
        public Guid DocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(DownloadDeclarationDocumentOutputDTO result)
        {
            string _description = $"Get Declaration Document for DocumentId: [{this.DocumentId}] and CustomerBusinessDeclarationAnswerCode: [{this.CustomerBusinessDeclarationAnswerCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class DownloadDeclarationDocumentQueryHandler : IRequestHandler<DownloadDeclarationDocumentQuery, DownloadDeclarationDocumentOutputDTO>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ILogger<DownloadDeclarationDocumentQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public DownloadDeclarationDocumentQueryHandler(IBusinessProfileRepository businessProfileRepository,
                                             ILogger<DownloadDeclarationDocumentQueryHandler> logger,
                                             StorageManager storageManager)
        {
            _businessProfileRepository = businessProfileRepository;
            _logger = logger;
            _storageManager = storageManager;
        }
        public async Task<DownloadDeclarationDocumentOutputDTO> Handle(DownloadDeclarationDocumentQuery request, CancellationToken cancellationToken)
        {
            var customerBusinessDeclarationAnswer = await _businessProfileRepository.GetCustomerBusinessDeclarationAnswerByCodeAsync(request.CustomerBusinessDeclarationAnswerCode);
            if (customerBusinessDeclarationAnswer == null)
            {
                return null;
            }
            if (customerBusinessDeclarationAnswer.DocumentId == request.DocumentId)
            {
                var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

                if (document != null)
                {
                    var ms = new MemoryStream();
                    await _storageManager.CopyToAsync(document.DocumentId, ms);
                    ms.Position = 0;
                    return new DownloadDeclarationDocumentOutputDTO()
                    {
                        File = ms,
                        ContentType = document.ContentType,
                        FileName = document.FileName
                    };
                }
            }
            return null;
        }
    }
}
