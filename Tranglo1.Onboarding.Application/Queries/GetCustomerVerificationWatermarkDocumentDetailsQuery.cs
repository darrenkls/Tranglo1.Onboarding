using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementVerification.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] {  })]
    internal class GetCustomerVerificationWatermarkDocumentDetailsQuery : BaseQuery<GetCustomerVerificationDocumentDetailsOutputDTO>
    {
        public long? CustomerVerificationDocumentCode { get; set; }

        public override Task<string> GetAuditLogAsync(GetCustomerVerificationDocumentDetailsOutputDTO result)
        {
            string _description = $"Get WatermarkDocument Details for Document ID: [{this.CustomerVerificationDocumentCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetCustomerVerificationWatermarkDocumentDetailsQueryHandler : IRequestHandler<GetCustomerVerificationWatermarkDocumentDetailsQuery, GetCustomerVerificationDocumentDetailsOutputDTO>
    {

        private readonly ILogger<GetCustomerVerificationDocumentDetailsOutputDTO> _logger;
        private readonly StorageManager _storageManager;
        private readonly IBusinessProfileRepository _repository;

        public GetCustomerVerificationWatermarkDocumentDetailsQueryHandler(
            ILogger<GetCustomerVerificationDocumentDetailsOutputDTO> logger,
            StorageManager storageManager,
            IBusinessProfileRepository repository)
        {
            _logger = logger;
            _storageManager = storageManager;
            _repository = repository;
        }

        public async Task<GetCustomerVerificationDocumentDetailsOutputDTO> Handle(GetCustomerVerificationWatermarkDocumentDetailsQuery request, CancellationToken cancellationToken)
        {
            var customerVerificationDocument = await _repository.GetCustomerVerificationDocumentsByCustomerVerificationDocumentCode(request.CustomerVerificationDocumentCode); 
            var document = await _storageManager.GetDocumentMetadataAsync(customerVerificationDocument.WatermarkDocumentID.Value);
            if (document != null)
            {
                var ms = new MemoryStream();
                await _storageManager.CopyToAsync(document.DocumentId, ms);
                ms.Position = 0;

                return new GetCustomerVerificationDocumentDetailsOutputDTO()
                {

                    File = ms,
                    ContentType = document.ContentType,
                    FileName = document.FileName,
                    FileSize = document.Length
                };
            }

            return null;
        }
    }
}
