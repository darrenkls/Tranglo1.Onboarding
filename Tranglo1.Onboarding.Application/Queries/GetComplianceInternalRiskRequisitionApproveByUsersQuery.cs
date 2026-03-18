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
    internal class GetComplianceInternalRiskRequisitionApproveByUsersQuery : BaseQuery<IEnumerable<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO>>
    {
        public long ComplianceSettingTypeCode { get; set; }


        public override Task<string> GetAuditLogAsync(IEnumerable<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO> result)
        {


            string _description = $"Get Compliance Internal Risk Requisition ApproveBy User Result for Product Setting Type Code: [{this.ComplianceSettingTypeCode}]";
            return Task.FromResult(_description);
        }

        public class GetComplianceInternalRiskRequisitionApproveByUsersQueryHandler : IRequestHandler<GetComplianceInternalRiskRequisitionApproveByUsersQuery, IEnumerable<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetComplianceInternalRiskRequisitionApproveByUsersQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO>> Handle(GetComplianceInternalRiskRequisitionApproveByUsersQuery request, CancellationToken cancellationToken)
            {

                var _connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO> RBApprovedByResultsDTO;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetComplianceInternalRiskRequisitionApprovedByUsers",
                        new
                        {
                            request.ComplianceSettingTypeCode
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    RBApprovedByResultsDTO = await reader.ReadAsync<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO>();
                }

                return RBApprovedByResultsDTO;
            }
        }
    }
}