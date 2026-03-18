using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.Edit)]
    internal class SaveWhitelistIPAddressCommand : BaseQuery<Result<WhitelistIPAddressOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public List<IPAddress> Staging { get; set; }
        public List<IPAddress> Production { get; set; }

        public override Task<string> GetAuditLogAsync(Result<WhitelistIPAddressOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Added pending whitelist IP Address for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveWhitelistIPAddressCommandHandler : IRequestHandler<SaveWhitelistIPAddressCommand, Result<WhitelistIPAddressOutputDTO>>
    {
        private readonly ILogger<SaveWhitelistIPAddressCommandHandler> _logger;

        public SaveWhitelistIPAddressCommandHandler(ILogger<SaveWhitelistIPAddressCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Result<WhitelistIPAddressOutputDTO>> Handle(SaveWhitelistIPAddressCommand request, CancellationToken cancellationToken)
        {
            //try
            //{
            var whitelistIP = new WhitelistIPAddressOutputDTO();
            //{
            //    //PartnerCode = request.PartnerCode
            //};

            //var result = await _partnerService.AddHelloSignDocumentAsync(APISettings);
            return Result.Success(whitelistIP);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError("SaveHelloSignDocumentCommand", ex.Message);
            //}
            //return Result.Failure<HelloSignDocument>(
            //                $"Save helloSign document name failed for {request.PartnerCode}."
            //            );
        }
    }
}
