using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.LegalEntitiy;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.View)]
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetUltimateBeneficialOwnersByCode : BaseQuery<IEnumerable<LegalEntitiyOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<LegalEntitiyOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Legal Entities for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Ultimate Beneficial Owner(s) for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetUltimateBeneficialOwnersByCodeHandler : IRequestHandler<GetUltimateBeneficialOwnersByCode, IEnumerable<LegalEntitiyOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly BusinessProfileService _businessProfileService;

            public GetUltimateBeneficialOwnersByCodeHandler( IMapper mapper, BusinessProfileService businessProfileService)
            {
                _mapper = mapper;
                _businessProfileService = businessProfileService;
            }

            public async Task<IEnumerable<LegalEntitiyOutputDTO>> Handle(GetUltimateBeneficialOwnersByCode query, CancellationToken cancellationToken)
            {
                var solution = Solution.Connect;

                if ((query?.CustomerSolution == null && query.CustomerSolution == ClaimCode.Business) || 
                    (query?.AdminSolution == null && query.AdminSolution == Solution.Business.Id)){
                    solution = Solution.Business;
                }

                var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);

                var legalEntityIndividual = await _businessProfileService.GetIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);

                var legalEntityCompany = await _businessProfileService.GetCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile.Value);

                var individualLegalEntityDTO = _mapper.Map<IEnumerable<IndividualLegalEntity>, IEnumerable<LegalEntitiyOutputDTO>>(legalEntityIndividual.Value).ToList();

                var companyLegalEntityDTO = _mapper.Map<IEnumerable<CompanyLegalEntity>, IEnumerable<LegalEntitiyOutputDTO>>(legalEntityCompany.Value).ToList();

                var _isUltimateBeneficialOwnerCompleted = await _businessProfileService.IsOwnershipLegalEntitiesCompleted(businessProfile.Value.Id, solution);


                for (int i = 0; i < individualLegalEntityDTO.Count(); i++)
                {
                    individualLegalEntityDTO.ElementAt(i).isCompleted = _isUltimateBeneficialOwnerCompleted[i];
                }


                for (int i = 0; i < companyLegalEntityDTO.Count(); i++)
                {
                    companyLegalEntityDTO.ElementAt(i).isCompleted = _isUltimateBeneficialOwnerCompleted[i];
                }

                // Set common properties outside the loops
                foreach (var legalEntitiesDto in individualLegalEntityDTO.Union(companyLegalEntityDTO))
                {
                    legalEntitiesDto.LegalEntityConcurrencyToken = businessProfile.Value.LegalEntityConcurrencyToken;
                }

                var legalEntityDTO = individualLegalEntityDTO.Union(companyLegalEntityDTO);

                return legalEntityDTO;
            }
        }
    }
}
