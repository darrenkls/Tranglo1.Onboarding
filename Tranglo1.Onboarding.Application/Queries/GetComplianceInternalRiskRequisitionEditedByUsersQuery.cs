using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetComplianceInternalRiskRequisitionEditedByUsersQuery : BaseQuery<IEnumerable<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO>>
    {
        public long ComplianceSettingTypeCode { get; set; }


        public override Task<string> GetAuditLogAsync(IEnumerable<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO> result)
        {


            string _description = $"Get Compliance Internal Risk Requisition EditedBy User Result for Product Setting Type Code: [{this.ComplianceSettingTypeCode}]";
            return Task.FromResult(_description);
        }

        public class GetComplianceInternalRiskRequisitionEditedByUsersQueryHandler : IRequestHandler<GetComplianceInternalRiskRequisitionEditedByUsersQuery, IEnumerable<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetComplianceInternalRiskRequisitionEditedByUsersQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO>> Handle(GetComplianceInternalRiskRequisitionEditedByUsersQuery request, CancellationToken cancellationToken)
            {

                var _connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO> RBAEditedByResultsDTO;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetComplianceInternalRiskRequisitionEditedByUsers",
                        new
                        {
                            request.ComplianceSettingTypeCode
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    RBAEditedByResultsDTO = await reader.ReadAsync<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO>();
                }

                return RBAEditedByResultsDTO;
            }
        }
    }
}
