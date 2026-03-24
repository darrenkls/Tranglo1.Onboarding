using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.TrangloRole;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetComplianceL2ApproverListQuery : IRequest<Result<IEnumerable<GetComplianceL2ApproverListOutputDTO>>>
    {
        public string trangloEntity { get; set; }
        public class GetComplianceL2ApproverListQueryHandler : IRequestHandler<GetComplianceL2ApproverListQuery, Result<IEnumerable<GetComplianceL2ApproverListOutputDTO>>>
        {

            private readonly IConfiguration _config;

            public GetComplianceL2ApproverListQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<Result<IEnumerable<GetComplianceL2ApproverListOutputDTO>>> Handle(GetComplianceL2ApproverListQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<GetComplianceL2ApproverListOutputDTO> getComplianceL2ApproverListOutputs;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "dbo.GetComplianceL2ApproverList",
                       new
                       {
                           trangloEntity = request.trangloEntity
                       },
                       null, null, CommandType.StoredProcedure);
                    getComplianceL2ApproverListOutputs = await reader.ReadAsync<GetComplianceL2ApproverListOutputDTO>();

                }

                return Result.Success<IEnumerable<GetComplianceL2ApproverListOutputDTO>>(getComplianceL2ApproverListOutputs);
            }
        }
    }
}
