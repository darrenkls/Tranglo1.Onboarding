using MediatR;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Threading;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.MediatR;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetComplianceRequisitionRequestedByUsersQuery : BaseQuery<IEnumerable<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO>>
    {
        public long ComplianceSettingTypeCode { get; set; }


        public override Task<string> GetAuditLogAsync(IEnumerable<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO> result)
        {


            string _description = $"Get Compliance Internal Risk Requisition RequestedBy User Result for Product Setting Type Code: [{this.ComplianceSettingTypeCode}]";
            return Task.FromResult(_description);
        }

        public class GetComplianceRequisitionRequestedByUsersQueryHandler : IRequestHandler<GetComplianceRequisitionRequestedByUsersQuery, IEnumerable<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetComplianceRequisitionRequestedByUsersQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO>> Handle(GetComplianceRequisitionRequestedByUsersQuery request, CancellationToken cancellationToken)
            {

                var _connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO> RBArequestedByResultsDTO;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetComplianceInternalRiskRequisitionRequestedByUsers",
                        new
                        {
                            request.ComplianceSettingTypeCode
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    RBArequestedByResultsDTO = await reader.ReadAsync<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO>();
                }

                return RBArequestedByResultsDTO;
            }
        }
    }
}
