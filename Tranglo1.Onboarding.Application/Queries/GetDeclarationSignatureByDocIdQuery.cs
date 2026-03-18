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
    //[Permission(PermissionGroupCode.KYCDeclaration, UACAction.View)]
    internal class GetDeclarationSignatureByDocIdQuery : BaseQuery<DeclarationSignatureOutputDto>
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(DeclarationSignatureOutputDto result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Declarations Signature for Document ID: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Declarations Signature for Document ID: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }
    internal class GetDeclarationSignatureByDocIdQueryHandler : IRequestHandler<GetDeclarationSignatureByDocIdQuery, DeclarationSignatureOutputDto>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<GetDeclarationSignatureByDocIdQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetDeclarationSignatureByDocIdQueryHandler(BusinessProfileService businessProfileService,
                                             ILogger<GetDeclarationSignatureByDocIdQueryHandler> logger,
                                             StorageManager storageManager)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
        }
        public async Task<DeclarationSignatureOutputDto> Handle(GetDeclarationSignatureByDocIdQuery request, CancellationToken cancellationToken)
        {
            var _declarationInfo = await _businessProfileService.GetKYCDeclarationInfoAsync(request.BusinessProfileCode);
            if (_declarationInfo == null)
            {
                return null;
            }
            if (_declarationInfo.DocumentId == request.DocumentId)
            {
                var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

                if (document != null)
                {
                    var ms = new MemoryStream();
                    await _storageManager.CopyToAsync(document.DocumentId, ms);
                    ms.Position = 0;
                    return new DeclarationSignatureOutputDto()
                    {
                        File = ms,
                        ContentType = document.ContentType,
                        FileName=document.FileName
                    };
                }
            }
            return null;
        }
    }

}
