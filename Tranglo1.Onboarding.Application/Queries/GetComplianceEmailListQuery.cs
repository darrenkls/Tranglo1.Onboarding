using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.TrangloRole;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetComplianceEmailListQuery : IRequest<Result<IEnumerable<GetComplianceEmailListQueryOutputDTO>>>
    {
        public int businessProfileCode { get; set; }
        public class GetComplianceEmailListQueryHandler : IRequestHandler<GetComplianceEmailListQuery, Result<IEnumerable<GetComplianceEmailListQueryOutputDTO>>>
        {

            private readonly IConfiguration _config;

            public GetComplianceEmailListQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<Result<IEnumerable<GetComplianceEmailListQueryOutputDTO>>> Handle(GetComplianceEmailListQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<GetComplianceEmailListQueryOutputDTO> GetComplianceEmailListQueryOutputs;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "dbo.GetComplianceUserEmailByBusinessProfileCode",
                       new
                       {
                           businessProfileCode = request.businessProfileCode
                       },
                       null, null, CommandType.StoredProcedure);
                    GetComplianceEmailListQueryOutputs = await reader.ReadAsync<GetComplianceEmailListQueryOutputDTO>();

                }

                return Result.Success<IEnumerable<GetComplianceEmailListQueryOutputDTO>>(GetComplianceEmailListQueryOutputs);
            }
        }
    }
}
