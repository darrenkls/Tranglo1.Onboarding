using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetCompanyUserStatusQuery : IRequest<IEnumerable<CompanyUserStatusOutputDTO>>
    {
        public class GetCompanyUserStatusQueryHandler : IRequestHandler<GetCompanyUserStatusQuery, IEnumerable<CompanyUserStatusOutputDTO>>
        {
            private readonly IConfiguration _config;
            public GetCompanyUserStatusQueryHandler(IConfiguration config)
            {
                _config = config;
            }
            public async Task<IEnumerable<CompanyUserStatusOutputDTO>> Handle(GetCompanyUserStatusQuery query, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                IEnumerable<CompanyUserStatusOutputDTO> combinedAccountStatuses;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetCombinedCompanyUserStatus",
                        CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    combinedAccountStatuses = await reader.ReadAsync<CompanyUserStatusOutputDTO>();
                }
                return combinedAccountStatuses;
            }
        }
    }
}
