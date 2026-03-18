using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementBusinessDeclaration.Action_Unblock_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementBusinessDeclaration.Action_View_Code })]

    internal class UnblockCustomerBusinessDeclarationCommand : BaseCommand<Result>
    {
        public int BusinessProfileCode { get; set; }
        public long CustomerBusinessDeclarationCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Unblock Customer Business Declaration for BusinessProfileCode: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }
            return Task.FromResult<string>(null);
        }
    }

    internal class UnblockCustomerBusinessDeclarationCommandHandler : IRequestHandler<UnblockCustomerBusinessDeclarationCommand, Result>
    {
        private readonly ILogger<UnblockCustomerBusinessDeclarationCommand> _logger;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public UnblockCustomerBusinessDeclarationCommandHandler(
            ILogger<UnblockCustomerBusinessDeclarationCommand> logger, IBusinessProfileRepository businessProfileRepository, IPartnerRepository partnerRepository)
        {
            _logger = logger;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result> Handle(UnblockCustomerBusinessDeclarationCommand request, CancellationToken cancellationToken)
        {
            var customerBusinessDeclaration = await _businessProfileRepository.GetCustomerBusinessDeclarationByCodeAsync(request.CustomerBusinessDeclarationCode);

            if (customerBusinessDeclaration.BusinessDeclarationStatus == BusinessDeclarationStatus.Blocked)
            {
                var businessDeclarationPendingStatus = await _businessProfileRepository.GetBusinessDeclarationStatus(BusinessDeclarationStatus.Pending.Id);
                customerBusinessDeclaration.BusinessDeclarationStatus = businessDeclarationPendingStatus;
                customerBusinessDeclaration.RedoCount = 3; // Reset RedoCount after unblock
            }

            customerBusinessDeclaration = await _businessProfileRepository.UpdateCustomerBusinessDeclarationAsync(customerBusinessDeclaration);

            return Result.Success();
        }
    }
}