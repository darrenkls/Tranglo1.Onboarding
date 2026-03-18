using CSharpFunctionalExtensions;
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
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetKYCCategoriesStatusesQuery : BaseQuery<IEnumerable<KYCCategoriesStatusesOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        
        public long KYCCategoryCode { get; set; }
        public string KYCCategoryDescription { get; set; }
        public DateTime? UserUpdatedDate { get; set; }
        public DateTime? LastReviewedDate { get; set; }
        public long ReviewResultCode { get; set; }
        public string ReviewResultDescription { get; set; }
        public long SolutionCode { get; set; }
        public string EntityCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<KYCCategoriesStatusesOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Categories Statuses for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get KYC Categories Statuses for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetKYCCategoriesStatusesQueryHandler : IRequestHandler<GetKYCCategoriesStatusesQuery,IEnumerable<KYCCategoriesStatusesOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly BusinessProfileService _businessProfileService;



            public GetKYCCategoriesStatusesQueryHandler(IConfiguration config,
               BusinessProfileService businessProfileService)
            {
                _config = config;
                _businessProfileService = businessProfileService;

            }

            public async Task<IEnumerable<KYCCategoriesStatusesOutputDTO>> Handle(GetKYCCategoriesStatusesQuery request, CancellationToken cancellationToken)
            {
                //1. Get from AMLCFT Documentation based on the business profile
                //TODO: Temporary pass in Connect Solution for Sprint 4
                bool isExistUploadedAMLDocumentaton = await _businessProfileService.CheckHasUploadedAMLDocumentation(request.BusinessProfileCode, Solution.Connect);

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                IEnumerable<KYCCategoriesStatusesOutputDTO> kycCategoriesStatusesDTOs;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "GetKYCCategoriesStatuses",
                       new
                       {
                           BusinessProfileCode = request.BusinessProfileCode,
                           isAMLCFTDocumentationUploaded = isExistUploadedAMLDocumentaton,
                           SolutionCode = request.SolutionCode,
                           EntityCode = request.EntityCode
                       },
                       null, null, CommandType.StoredProcedure);

                    kycCategoriesStatusesDTOs = await reader.ReadAsync<KYCCategoriesStatusesOutputDTO>();

                  
                }
                return kycCategoriesStatusesDTOs;
            }
        }

    }

    
   
}

