using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    [Permission(Permission.KYCManagementDocumentation.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class GetDocumentsCategoriesListQuery : BaseQuery<IEnumerable<DocumentsCategoryListOutputDto>>
    {
        public int BusinessProfileCode { get; set; }
        public int SolutionCode { get; set; }
        public int TrangloEntityCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<DocumentsCategoryListOutputDto> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Documents Categories List for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Documents Categories List for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetDocumentsCategoriesListQueryHandler : IRequestHandler<GetDocumentsCategoriesListQuery, IEnumerable<DocumentsCategoryListOutputDto>>
        {
            private readonly IConfiguration _config;
            private readonly IPartnerRepository _partnerRepository;

            public GetDocumentsCategoriesListQueryHandler(IConfiguration config, IPartnerRepository partnerRepository)
            {
                _config = config;
                _partnerRepository = partnerRepository;
            }

            public async Task<IEnumerable<DocumentsCategoryListOutputDto>> Handle(GetDocumentsCategoriesListQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var partnerSubscription = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistration.Id);


                IEnumerable<DocumentsCategoryListOutputDto> documentsCategoryListOutputDtos;
                IEnumerable<DocumentCategoryTemplateOutputDTO> documentCategoryTemplateOutputDTOs;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetDocumentCategoriesByBusinessProfile",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode,
                            SolutionCode = request.SolutionCode,
                            CustomerTypeCode = partnerRegistration.CustomerTypeCode,
                            TrangloEntityCode = request.TrangloEntityCode
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    documentsCategoryListOutputDtos = await reader.ReadAsync<DocumentsCategoryListOutputDto>();
                    documentCategoryTemplateOutputDTOs = await reader.ReadAsync<DocumentCategoryTemplateOutputDTO>();

                    var list = documentsCategoryListOutputDtos.ToList();

                    
                    var categoryUBO = list.FirstOrDefault(x => x.CategoryId == 64);

                    
                    var categorySSM = list.FindIndex(x => x.CategoryId == 24);

                    if (categoryUBO != null && categorySSM >= 0)
                    {
                        
                        list.Remove(categoryUBO);

                        
                        list.Insert(categorySSM, categoryUBO);
                    }

                    foreach (var documentCategoryDTO in list)
                    {
                        var templates = documentCategoryTemplateOutputDTOs
                                            .Where(x => x.DocumentCategoryCode == documentCategoryDTO.CategoryId)
                                            .ToList();

                        documentCategoryDTO.TemplateIds = templates.Select(x => x.DocumentId).ToArray();

                    }
                    documentsCategoryListOutputDtos = list;
                }
                return documentsCategoryListOutputDtos;
            }
        }
    }
}
