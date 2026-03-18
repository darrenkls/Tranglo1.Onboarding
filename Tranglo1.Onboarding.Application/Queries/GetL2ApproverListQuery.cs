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

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetL2ApproverListQuery : IRequest<Result<IEnumerable<GetL2ApproverListOutputDTO>>>
    {
        public class GetL2ApproverListQueryHandler : IRequestHandler<GetL2ApproverListQuery, Result<IEnumerable<GetL2ApproverListOutputDTO>>>
        {

            private readonly IConfiguration _config;

            public GetL2ApproverListQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<Result<IEnumerable<GetL2ApproverListOutputDTO>>> Handle(GetL2ApproverListQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<GetL2ApproverListOutputDTO> getL2ApproverListOutputs;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "dbo.GetL2ApproverList",
                       new
                       {

                       },
                       null, null, CommandType.StoredProcedure);
                    getL2ApproverListOutputs = await reader.ReadAsync<GetL2ApproverListOutputDTO>();

                }

                return Result.Success<IEnumerable<GetL2ApproverListOutputDTO>>(getL2ApproverListOutputs);
            }
        }
    }
}
