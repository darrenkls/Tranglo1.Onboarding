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
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetKYCComplianceOfficerQuery : BaseQuery<Result<IEnumerable<KYCComplianceOfficerOutputDTO>>>
    {

        public string LoginID { get; set; }
        public string Name { get; set; }

        //public override Task<string> GetAuditLogAsync(IEnumerable<KYCComplianceOfficerOutputDTO> result)
        //{
        //    string _description = $"Get KYC Compliance Officer: ";
        //    return Task.FromResult(_description);
        //}


        public class GetKYCComplianceOfficerQueryHandler : IRequestHandler<GetKYCComplianceOfficerQuery, Result<IEnumerable<KYCComplianceOfficerOutputDTO>>>
        {
            private readonly IConfiguration _config;

            public GetKYCComplianceOfficerQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<Result<IEnumerable<KYCComplianceOfficerOutputDTO>>> Handle(GetKYCComplianceOfficerQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "GetKYCComplianceOfficers",
                        new
                        {

                        },
                        null, null, CommandType.StoredProcedure);
                    var result = await reader.ReadAsync<KYCComplianceOfficerOutputDTO>();
                    result = result.OrderBy(x => x.ComplianceOfficerAssignedName).ToList();

                    return Result.Success<IEnumerable<KYCComplianceOfficerOutputDTO>>(result);
                }
            }
        }
    }
}
