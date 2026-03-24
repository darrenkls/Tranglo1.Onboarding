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
    public class GetL1ApproverListQuery : IRequest<Result<IEnumerable<GetL1ApproverListOutputDTO>>>
    {
        public class GetL1ApproverListQueryHandler : IRequestHandler<GetL1ApproverListQuery, Result<IEnumerable<GetL1ApproverListOutputDTO>>>
        {

            private readonly IConfiguration _config;

            public GetL1ApproverListQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<Result<IEnumerable<GetL1ApproverListOutputDTO>>> Handle(GetL1ApproverListQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<GetL1ApproverListOutputDTO> getL1ApproverListOutputs;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "dbo.GetL1ApproverList",
                       new
                       {

                       },
                       null, null, CommandType.StoredProcedure);
                    getL1ApproverListOutputs = await reader.ReadAsync<GetL1ApproverListOutputDTO>();

                }

                return Result.Success<IEnumerable<GetL1ApproverListOutputDTO>>(getL1ApproverListOutputs);
            }
        }
    }
}
