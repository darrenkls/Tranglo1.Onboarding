
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetRBAApprovalResultsQuery : BaseQuery<IEnumerable<RBAApprovalResultsOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string TrangloEntityCode { get; set; }


        public override Task<string> GetAuditLogAsync(IEnumerable<RBAApprovalResultsOutputDTO> result)
        {
     

            string _description = $"Get RBA Approval Result for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetRBAApprovalResultsQueryHandler : IRequestHandler<GetRBAApprovalResultsQuery, IEnumerable<RBAApprovalResultsOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetRBAApprovalResultsQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<RBAApprovalResultsOutputDTO>> Handle(GetRBAApprovalResultsQuery request, CancellationToken cancellationToken)
            {

                var _connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<RBAApprovalResultsOutputDTO> RBAApprovalResultsDTO;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetRBARequisitionsApprovedOrRejected",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode,
                            TrangloEntity = request.TrangloEntityCode
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    RBAApprovalResultsDTO = await reader.ReadAsync<RBAApprovalResultsOutputDTO>();
                }

                return RBAApprovalResultsDTO;
            }
        }
    }
}
