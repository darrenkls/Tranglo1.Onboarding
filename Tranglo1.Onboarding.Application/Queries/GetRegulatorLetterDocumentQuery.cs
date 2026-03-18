using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.LicenseInformation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetRegulatorLetterDocumentQuery : BaseQuery<DownloadRegulatorLetterDocumentOutputDTO>
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(DownloadRegulatorLetterDocumentOutputDTO result)
        {
            string _description = $"Get Regulator Letter Document for DocumentId: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetRegulatorLetterDocumentQueryHandler : IRequestHandler<GetRegulatorLetterDocumentQuery, DownloadRegulatorLetterDocumentOutputDTO>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<GetRegulatorLetterDocumentQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetRegulatorLetterDocumentQueryHandler(BusinessProfileService businessProfileService,
                                             ILogger<GetRegulatorLetterDocumentQueryHandler> logger,
                                             StorageManager storageManager)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
        }
        public async Task<DownloadRegulatorLetterDocumentOutputDTO> Handle(GetRegulatorLetterDocumentQuery request, CancellationToken cancellationToken)
        {
            var licenseInformation = await _businessProfileService.GetLicenseInfoByBusinessCode(request.BusinessProfileCode);
            if (licenseInformation == null)
            {
                return null;
            }
            if (licenseInformation.RegulatorDocumentId == request.DocumentId)
            {
                var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

                if (document != null)
                {
                    var ms = new MemoryStream();
                    await _storageManager.CopyToAsync(document.DocumentId, ms);
                    ms.Position = 0;
                    return new DownloadRegulatorLetterDocumentOutputDTO()
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
