using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetDocumentDetailsQuery : BaseQuery<DocumentDetailsOutputDTO>
    {
        public Guid DocumentId { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(DocumentDetailsOutputDTO result)
        {
            string _description = $"Get Document Details for Document ID: [{this.DocumentId}]";
            return Task.FromResult(_description);
        }
    }
    internal class GetDocumentDetailsQueryHandler : IRequestHandler<GetDocumentDetailsQuery, DocumentDetailsOutputDTO>
    {
      
        private readonly ILogger<GetDocumentDetailsQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetDocumentDetailsQueryHandler(ILogger<GetDocumentDetailsQueryHandler> logger,
                                             StorageManager storageManager)
        {
            _logger = logger;
            _storageManager = storageManager;
        }
        public async Task<DocumentDetailsOutputDTO> Handle(GetDocumentDetailsQuery request, CancellationToken cancellationToken)
        {
                var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

                if (document != null)
                {
                    var ms = new MemoryStream();
                    await _storageManager.CopyToAsync(document.DocumentId, ms);
                    ms.Position = 0;
                    return new DocumentDetailsOutputDTO()
                    {
                        ContentType = document.ContentType,
                        FileName = document.FileName,
                        FileSize = document.Length
                    };
                }
            
            return null;
        }
    }
}


