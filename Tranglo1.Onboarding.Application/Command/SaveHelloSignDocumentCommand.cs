using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_HelloSignAdd_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerPartnerDocuments.Action_View_Code })]
    internal class SaveHelloSignDocumentCommand : BaseCommand<Result<HelloSignDocument>>
    {
        public long PartnerCode { get; set; }
        public string DocumentName { get; set; }
        public DateTime SentDate { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<HelloSignDocument> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Uploaded HelloSign document";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveHelloSignDocumentCommandHandler : IRequestHandler<SaveHelloSignDocumentCommand, Result<HelloSignDocument>>
    {
        private readonly PartnerService _partnerService;
        private readonly ILogger<SaveHelloSignDocumentCommandHandler> _logger;

        public SaveHelloSignDocumentCommandHandler(
          PartnerService partnerService, ILogger<SaveHelloSignDocumentCommandHandler> logger, IMapper mapper)
        {
            _partnerService = partnerService;
            _logger = logger;
        }

        public async Task<Result<HelloSignDocument>> Handle(SaveHelloSignDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.SentDate.Date > DateTime.UtcNow.Date)
                {
                    return Result.Failure<HelloSignDocument>($"Sent Date cannot be later than today's date.");
                }
                if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
                { 
                HelloSignDocument hellosignDoc = new HelloSignDocument()
                {
                    PartnerCode = request.PartnerCode,
                    DocumentName = request.DocumentName,
                    SentDate = request.SentDate,
                    SolutionCode = Solution.Connect.Id,
                    IsRemoved = false
                };

                var result = await _partnerService.AddHelloSignDocumentAsync(hellosignDoc);
                return Result.Success(result);
                }
                if (ClaimCode.Business == request.CustomerSolution || Solution.Business.Id == request.AdminSolution)
                {
                    HelloSignDocument hellosignDoc = new HelloSignDocument()
                    {
                        PartnerCode = request.PartnerCode,
                        DocumentName = request.DocumentName,
                        SentDate = request.SentDate,
                        SolutionCode = Solution.Business.Id,
                        IsRemoved = false
                    };

                    var result = await _partnerService.AddHelloSignDocumentAsync(hellosignDoc);
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SaveHelloSignDocumentCommand] {ex.Message}");
            }
            return Result.Failure<HelloSignDocument>(
                            $"Save helloSign document name failed for {request.PartnerCode}."
                        );
        }
    }
}

