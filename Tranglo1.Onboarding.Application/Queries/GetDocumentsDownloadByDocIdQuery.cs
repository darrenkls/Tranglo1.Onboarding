using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    internal class GetDocumentsDownloadByDocIdQuery : BaseQuery<DocumentsDownloadDTO>
    {
        public Guid DocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(DocumentsDownloadDTO result)
        {
            string _description = $"Get Documents Download for Document ID: [{this.DocumentId}]";
            return Task.FromResult(_description);
        }
    }
    internal class GetDocumentsDownloadByDocIdQueryHandler : IRequestHandler<GetDocumentsDownloadByDocIdQuery, DocumentsDownloadDTO>
    {
        private readonly StorageManager _storageManager;

        public GetDocumentsDownloadByDocIdQueryHandler(StorageManager storageManager)
        {
            _storageManager = storageManager;
        }
        public async Task<DocumentsDownloadDTO> Handle(GetDocumentsDownloadByDocIdQuery request, CancellationToken cancellationToken)
        {
            var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

            if (document != null)
            {
                var ms = new MemoryStream();
                await _storageManager.CopyToAsync(document.DocumentId, ms);
                ms.Position = 0;
                return new DocumentsDownloadDTO()
                {
                    File = ms,
                    ContentType = document.ContentType,
                    FileName = document.FileName
                };
            }
            return null;
        }
    }
}
