using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.MediatR;
using CSharpFunctionalExtensions;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCCOInformation, UACAction.View)]
    [Permission(Permission.KYCManagementCOInformation.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetCoInformationByIdQuery : BaseQuery<Result<ComplianceOfficersOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<ComplianceOfficersOutputDTO> result)
        {
            
            if (result.IsSuccess)
            {
                string _description = $"Get CO Information for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            /*

            string _description = $"Get CO Information for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description); 
            */
        }

        public class GetCoInformationByIdQueryHandler : IRequestHandler<GetCoInformationByIdQuery, Result<ComplianceOfficersOutputDTO>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly IMapper _mapper;
            public GetCoInformationByIdQueryHandler(BusinessProfileService businessProfileService, IMapper mapper)
            {
                _businessProfileService = businessProfileService;
                _mapper = mapper;
            }

            public async Task<Result<ComplianceOfficersOutputDTO>> Handle(GetCoInformationByIdQuery request, CancellationToken cancellationToken)
            {
                var coInfodetails = await _businessProfileService.GetCOInfoByBusinessCode(request.BusinessProfileCode);

                var result = _mapper.Map<ComplianceOfficersOutputDTO>(coInfodetails);

                return Result.Success(result);
            }

        }
    }
}
