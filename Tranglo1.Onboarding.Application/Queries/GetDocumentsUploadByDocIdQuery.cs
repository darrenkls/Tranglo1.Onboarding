using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    [Permission(Permission.KYCManagementDocumentation.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetDocumentsUploadByDocIdQuery : BaseQuery<DocumentsUploadDto>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(DocumentsUploadDto result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Documents Upload for Document ID: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Downloaded Template";
            return Task.FromResult(_description);
        }
    }
    internal class GetDocumentsUploadByDocIdQueryHandler : IRequestHandler<GetDocumentsUploadByDocIdQuery, DocumentsUploadDto>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<GetDocumentsUploadByDocIdQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetDocumentsUploadByDocIdQueryHandler(BusinessProfileService businessProfileService,
                                             ILogger<GetDocumentsUploadByDocIdQueryHandler> logger,
                                             StorageManager storageManager)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
        }
        public async Task<DocumentsUploadDto> Handle(GetDocumentsUploadByDocIdQuery request, CancellationToken cancellationToken)
        {
            var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

            if (document != null)
            {
                var ms = new MemoryStream();
                await _storageManager.CopyToAsync(document.DocumentId, ms);
                ms.Position = 0;
                return new DocumentsUploadDto()
                {
                    File = ms,
                    ContentType = document.ContentType,
                    FileName =document.FileName
                };
            }
            return null;
        }
    }
}
