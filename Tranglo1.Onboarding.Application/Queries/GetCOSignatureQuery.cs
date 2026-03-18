using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDeclaration, UACAction.View)]

    internal class GetCOSignatureQuery : BaseQuery<COSignatureOutputDTO>
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(COSignatureOutputDTO result)
        {
            string _description = $"Get Declarations Signature for Document ID: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetCOSignatureQueryHandler : IRequestHandler<GetCOSignatureQuery, COSignatureOutputDTO>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<GetCOSignatureQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetCOSignatureQueryHandler(BusinessProfileService businessProfileService,
                                             ILogger<GetCOSignatureQueryHandler> logger,
                                             StorageManager storageManager)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
        }
        public async Task<COSignatureOutputDTO> Handle(GetCOSignatureQuery request, CancellationToken cancellationToken)
        {
            var _coInfo = await _businessProfileService.GetCOInfoByBusinessCode(request.BusinessProfileCode);
            if (_coInfo == null)
            {
                return null;
            }
            if (_coInfo.CoSignatureDocumentId == request.DocumentId)
            {
                var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

                if (document != null)
                {
                    var ms = new MemoryStream();
                    await _storageManager.CopyToAsync(document.DocumentId, ms);
                    ms.Position = 0;
                    return new COSignatureOutputDTO()
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
