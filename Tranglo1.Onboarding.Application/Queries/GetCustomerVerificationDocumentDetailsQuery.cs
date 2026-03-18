using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
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
        new string[] { })]
    internal class GetCustomerVerificationDocumentDetailsQuery : BaseQuery<List<GetCustomerVerificationDocumentDetailsOutputDTO>>
    {
        public Guid? DocumentId { get; set; }
        public int? BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(List<GetCustomerVerificationDocumentDetailsOutputDTO> result)
        {
            string _description = $"Get Document Details for Document ID: [{this.DocumentId}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetCustomerVerificationDocumentDetailsQueryHandler : IRequestHandler<GetCustomerVerificationDocumentDetailsQuery, List<GetCustomerVerificationDocumentDetailsOutputDTO>>
    {
        private readonly ILogger<List<GetCustomerVerificationDocumentDetailsOutputDTO>> _logger;
        private readonly StorageManager _storageManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public GetCustomerVerificationDocumentDetailsQueryHandler (ILogger<List<GetCustomerVerificationDocumentDetailsOutputDTO>> logger, 
            StorageManager storageManager, 
            IBusinessProfileRepository businessProfileRepository,
            IPartnerRepository partnerRepository)
        {
            _logger = logger;
            _storageManager = storageManager;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task<List<GetCustomerVerificationDocumentDetailsOutputDTO>> Handle(GetCustomerVerificationDocumentDetailsQuery request, CancellationToken cancellationToken)
        {
            var output = new List<GetCustomerVerificationDocumentDetailsOutputDTO>();
            var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId.Value);

            if (document != null)
            {
                var ms = new MemoryStream();
                await _storageManager.CopyToAsync(document.DocumentId, ms);
                ms.Position = 0;

                var file = new GetCustomerVerificationDocumentDetailsOutputDTO()
                {
                    File = ms,
                    ContentType = document.ContentType,
                    FileName = document.FileName,
                    FileSize = document.Length
                };

                output.Add(file);
            }
            else if (request.DocumentId is null || request.DocumentId == Guid.Empty)
            {
                if (request.BusinessProfileCode is null)
                {
                    return output;
                }

                var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.BusinessProfileCode.Value);
                if (partner is null)
                {
                    return output;
                }

                var subscriptions = await _partnerRepository.GetPartnerSubscriptionListAsync(partner.Id);
                if (subscriptions is null)
                {
                    return output;
                }

                var f2FVerificationTemplates = new List<DefaultTemplate>();
                var hasTSB = subscriptions.Any(x => x.TrangloEntity == TrangloEntity.TSB.TrangloEntityCode);
                var hasNonTSB = subscriptions.Any(x => x.TrangloEntity != TrangloEntity.TSB.TrangloEntityCode);

                if (hasTSB && hasNonTSB)
                {
                    f2FVerificationTemplates.Add(DefaultTemplate.F2FVerificationTemplateMY);
                    f2FVerificationTemplates.Add(DefaultTemplate.F2FVerificationTemplateNonMY);
                }
                else if (hasTSB && !hasNonTSB)
                {
                    f2FVerificationTemplates.Add(DefaultTemplate.F2FVerificationTemplateMY);
                }
                else if (!hasTSB && hasNonTSB)
                {
                    f2FVerificationTemplates.Add(DefaultTemplate.F2FVerificationTemplateNonMY);
                }

                foreach (var template in f2FVerificationTemplates)
                {
                    var defaultTemplateDocument = await _businessProfileRepository.GetDefaultTemplateDocumentAsync(template.Id);
                    if (defaultTemplateDocument != null)
                    {
                        document = await _storageManager.GetDocumentMetadataAsync(defaultTemplateDocument.DocumentId.Value);

                        var ms = new MemoryStream();
                        await _storageManager.CopyToAsync(defaultTemplateDocument.DocumentId.Value, ms);
                        ms.Position = 0;

                        var file = new GetCustomerVerificationDocumentDetailsOutputDTO()
                        {
                            File = ms,
                            ContentType = document.ContentType,
                            FileName = document.FileName,
                            FileSize = document.Length
                        };

                        output.Add(file);
                    }
                }                
            }

            return output;
        }
    }
}