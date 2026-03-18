using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    [Permission(Permission.KYCManagementDocumentation.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] {  })]
    internal class GetBusinessDocumentsCategoriesListQuery : BaseQuery<IEnumerable<BusinessDocumentCategoryListOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public int SolutionCode { get; set; }
        public int TrangloEntityCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<BusinessDocumentCategoryListOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Documents Categories List for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Business Documents Categories List for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetBusinessDocumentsCategoriesListQueryHandler : IRequestHandler<GetBusinessDocumentsCategoriesListQuery, IEnumerable<BusinessDocumentCategoryListOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IBusinessProfileRepository _businessProfileRepository;

            public GetBusinessDocumentsCategoriesListQueryHandler(IConfiguration config, IPartnerRepository partnerRepository, IBusinessProfileRepository businessProfileRepository)
            {
                _config = config;
                _partnerRepository = partnerRepository;
                _businessProfileRepository = businessProfileRepository;
            }

            public async Task<IEnumerable<BusinessDocumentCategoryListOutputDTO>>Handle(GetBusinessDocumentsCategoriesListQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistration.Id);
                var telEntity = partnerSubInfo.Any(x => x.TrangloEntity == "TEL");
                bool rspTEL = false;
                var uboInfo = await _businessProfileRepository.GetLegalEntityAsync(request.BusinessProfileCode);
                bool IsUboAbove25Percent = false;

                if (uboInfo != null) 
                {
                    if (decimal.TryParse(uboInfo.EffectiveShareholding, out var shareholding) && shareholding >= 25)
                    {
                        IsUboAbove25Percent = true;
                    }

                }


                if (partnerSubInfo.Count == 1 && telEntity == true && partnerRegistration.CustomerType == CustomerType.Remittance_Partner)
                {
                    rspTEL = true;
                }


                IEnumerable<BusinessDocumentCategoryListOutputDTO> documentsCategoryListOutputDtos;
                IEnumerable<DocumentCategoryTemplateOutputDTO> documentCategoryTemplateOutputDTOs;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetBusinessDocumentCategoriesByBusinessProfile",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode,
                            SolutionCode = request.SolutionCode,
                            CustomerTypeCode = partnerRegistration.CustomerTypeCode,
                            RSPTEL = rspTEL,
                            TrangloEntityCode = request.TrangloEntityCode,
                            IsUboAbove25Percent = IsUboAbove25Percent
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    documentsCategoryListOutputDtos = await reader.ReadAsync<BusinessDocumentCategoryListOutputDTO>();
                    documentCategoryTemplateOutputDTOs = await reader.ReadAsync<DocumentCategoryTemplateOutputDTO>();

                  
                        foreach (var documentCategoryDTO in documentsCategoryListOutputDtos)
                    {
                        var templates = documentCategoryTemplateOutputDTOs
                                            .Where(x => x.DocumentCategoryCode == documentCategoryDTO.CategoryId)
                                            .ToList();
                   
                        documentCategoryDTO.TemplateIds = templates.Select(x => x.DocumentId).ToArray();
                        documentCategoryDTO.FileName = templates.Select(x => x.FileName).ToArray();
                   
                    }
                }
                return documentsCategoryListOutputDtos;
            }

        }
    }
}
