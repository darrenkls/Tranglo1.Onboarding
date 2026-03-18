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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.LegalEntitiy;
using Tranglo1.Onboarding.Application.DTO.Shareholder;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.View)]
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetShareholderByIdQuery : BaseQuery<IEnumerable<ShareholderOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<ShareholderOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Shareholders for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Shareholders for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetShareholderByIdQueryHandler : IRequestHandler<GetShareholderByIdQuery, IEnumerable<ShareholderOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly BusinessProfileService _businessProfileService;

            public GetShareholderByIdQueryHandler(IMapper mapper, BusinessProfileService businessProfileService)
            {
                _mapper = mapper;
                _businessProfileService = businessProfileService;
            }

            public async Task<IEnumerable<ShareholderOutputDTO>> Handle(GetShareholderByIdQuery query, CancellationToken cancellationToken)
            {
                var solution = Solution.Connect;

                if ((query?.CustomerSolution == null && query.CustomerSolution == ClaimCode.Business) ||
                    (query?.AdminSolution == null && query.AdminSolution == Solution.Business.Id))
                {
                    solution = Solution.Business;
                }

                var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);

                var shareholderIndividual = await _businessProfileService.GetIndividualShareholderByBusinessProfileCodeAsync(businessProfile.Value);

                var shareholderCompany = await _businessProfileService.GetCompanyShareholderByBusinessProfileCodeAsync(businessProfile.Value);

                List<ShareholderCompanyLegalEntity> ShareholderCompanyLegalEntityList = new List<ShareholderCompanyLegalEntity>();
                List<ShareholderIndividualLegalEntity> ShareholderIndividualLegalEntityList = new List<ShareholderIndividualLegalEntity>();
                {
                    var shareholderCompanyLegalEntityRecords = await _businessProfileService.GetShareholderByBusinessProfileCodeAsync(businessProfile.Value);
                    foreach (var a in shareholderCompanyLegalEntityRecords.Value)
                    {
                        var companylegalEntityList = await _businessProfileService.GetShareholderCompanyLegalEntity(a.Id);

                        if (companylegalEntityList != null)
                        {             
                            {
                                foreach (var c in companylegalEntityList)
                                {
                                    ShareholderCompanyLegalEntityList.Add(c);
                                }
                            }
                        }
                    }
                    var shareholderIndividuLegalEntityRecords = await _businessProfileService.GetShareholderByBusinessProfileCodeAsync(businessProfile.Value);
                    foreach (var b in shareholderIndividuLegalEntityRecords.Value)
                    {
                        var individualLegalEntityList = await _businessProfileService.GetShareholderIndividualLegalEntity(b.Id);
                        
                        if (individualLegalEntityList != null)
                        {
                            foreach (var e in individualLegalEntityList)
                            {    
                                ShareholderIndividualLegalEntityList.Add(e);
                            }
                        }
                    }
         
                    
                }

                var individualShareholderDTOs = _mapper.Map<IEnumerable<IndividualShareholder>, List<ShareholderOutputDTO>>(shareholderIndividual.Value);
                var companyShareholderDTOs = _mapper.Map<IEnumerable<CompanyShareholder>, List<ShareholderOutputDTO>>(shareholderCompany.Value);

                var shareholderIndividualLegalEntityDTOs = _mapper.Map<IEnumerable<ShareholderIndividualLegalEntity>, List<ShareholderIndividualLegalEntityOutputDTO>>(ShareholderIndividualLegalEntityList);
                var shareholderCompanyLegalEntityDTOs = _mapper.Map<IEnumerable<ShareholderCompanyLegalEntity>, List<ShareholderCompanyLegalEntityOutputDTO>>(ShareholderCompanyLegalEntityList);

                foreach (var dto in companyShareholderDTOs)
                {
                  
                    if (dto.ShareholderCode.Value != 0)
                    {
                        dto.ShareholderCompanyLegalEntityOutputDTOs = shareholderCompanyLegalEntityDTOs
                            .Where(e => e.ShareholderCode == dto.ShareholderCode)
                            .ToList();

                        dto.ShareholderIndividualLegalEntityOutputDTOs = shareholderIndividualLegalEntityDTOs
                            .Where(e => e.ShareholderCode == dto.ShareholderCode)
                            .ToList();
                    }  
                    else
                    {

                        dto.ShareholderIndividualLegalEntityOutputDTOs = new List<ShareholderIndividualLegalEntityOutputDTO>();
                        dto.ShareholderCompanyLegalEntityOutputDTOs = new List<ShareholderCompanyLegalEntityOutputDTO>();
                    }
                   
                }

                var allShareholdersDTO = new List<ShareholderOutputDTO>();
                allShareholdersDTO.AddRange(individualShareholderDTOs);
                allShareholdersDTO.AddRange(companyShareholderDTOs);

                var _isShareholderCompleted = await _businessProfileService.IsOwnershipShareholdersCompleted(businessProfile.Value.Id, solution);

                for (int i = 0; i < individualShareholderDTOs.Count; i++)
                {
                    individualShareholderDTOs[i].IsCompleted = _isShareholderCompleted.Where(x => x.shareholderCode == individualShareholderDTOs[i].ShareholderCode).Select(x => x.isCompleted).FirstOrDefault();
                }

                for (int i = 0; i < companyShareholderDTOs.Count; i++)
                {
                    companyShareholderDTOs[i].IsCompleted = _isShareholderCompleted.Where(x => x.shareholderCode == companyShareholderDTOs[i].ShareholderCode).Select(x => x.isCompleted).FirstOrDefault();
                }

                foreach (var shareholderDTO in allShareholdersDTO)
                {
                    shareholderDTO.ShareholderConcurrencyToken = businessProfile.Value.ShareholderConcurrencyToken;
                }

                return allShareholdersDTO;


            }
        }
    }
}
