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
using Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.View)]
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] {})]
    internal class GetAffiliateAndSubsidiaryByIdQuery : BaseQuery<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<AffiliateAndSubsidiaryOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Affiliate and Subsidiaries for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Affiliate and Subsidiaries for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetAffiliateAndSubsidiaryByIdQueryHandler : IRequestHandler<GetAffiliateAndSubsidiaryByIdQuery, IEnumerable<AffiliateAndSubsidiaryOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly BusinessProfileService _businessProfileService;

            public GetAffiliateAndSubsidiaryByIdQueryHandler(IMapper mapper, BusinessProfileService businessProfileService)
            {
                _mapper = mapper;
                _businessProfileService = businessProfileService;
            }

            public async Task<IEnumerable<AffiliateAndSubsidiaryOutputDTO>> Handle(GetAffiliateAndSubsidiaryByIdQuery query, CancellationToken cancellationToken)
            {                
                var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);

                var subsidiary = await _businessProfileService.GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile.Value);

                var _isSubsidiaryCompleted = await _businessProfileService.IsOwnershipSubsidiariesCompleted(businessProfile.Value.Id);
                bool isCompletedValue = _isSubsidiaryCompleted.FirstOrDefault();


                var subsidiaryDTO = _mapper.Map<IEnumerable<AffiliateAndSubsidiary>,IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(subsidiary.Value);
                for( int i = 0; i < subsidiaryDTO.Count(); i++)
                {
                    subsidiaryDTO.ElementAt(i).isCompleted = _isSubsidiaryCompleted[i];
                }

                foreach (var subOutput in subsidiaryDTO)
                {
                    // Set the concurrency token from BusinessProfile
                    subOutput.AffiliatesAndSubsidiariesConcurrencyToken = businessProfile.Value.AffiliatesAndSubsidiariesConcurrencyToken;
                }

                return subsidiaryDTO;
            }
        }
    }
}
