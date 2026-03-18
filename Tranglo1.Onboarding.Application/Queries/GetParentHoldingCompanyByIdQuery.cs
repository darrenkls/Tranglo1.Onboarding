using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.ParentHoldingCompany;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.View)]
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetParentHoldingCompanyByIdQuery : BaseQuery<IEnumerable<ParentHoldingCompanyOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<ParentHoldingCompanyOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Parent Holding Companies for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Parent Holding Companies for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetParentHoldingCompanyByIdQueryHandler : IRequestHandler<GetParentHoldingCompanyByIdQuery, IEnumerable<ParentHoldingCompanyOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly BusinessProfileService _businessProfileService;

            public GetParentHoldingCompanyByIdQueryHandler( IMapper mapper, BusinessProfileService businessProfileService)
            {
                _mapper = mapper;
                _businessProfileService = businessProfileService;
            }

            public async Task<IEnumerable<ParentHoldingCompanyOutputDTO>> Handle(GetParentHoldingCompanyByIdQuery query, CancellationToken cancellationToken)
            {

                var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);

                var parent = await _businessProfileService.GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile.Value);

                var _isParentHoldingCompleted = await _businessProfileService.IsOwnershipParentHoldingsCompleted(businessProfile.Value.Id);

                var parentHoldingDTO =  _mapper.Map<IEnumerable<ParentHoldingCompany>, IEnumerable<ParentHoldingCompanyOutputDTO>>(parent.Value);

                for (int i = 0; i < parentHoldingDTO.Count(); i++)
                {
                    parentHoldingDTO.ElementAt(i).isCompleted = _isParentHoldingCompleted[i];
                }


                foreach (var parentOuput in parentHoldingDTO)
                {
                    // Set the concurrency token from BusinessProfile
                    parentOuput.ParentHoldingsConcurrencyToken = businessProfile.Value.ParentHoldingsConcurrencyToken;
                }

                return parentHoldingDTO;
            }
        }
    }
}
