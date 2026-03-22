using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetRelationshipTieUpQuery : IRequest<IEnumerable<RelationshipTieUpOutputDTO>>
    {
        public class GetRelationshipTieUpQueryHandler : IRequestHandler<GetRelationshipTieUpQuery, IEnumerable<RelationshipTieUpOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetRelationshipTieUpQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<RelationshipTieUpOutputDTO>> Handle(GetRelationshipTieUpQuery query, CancellationToken cancellationToken)
            {
                IEnumerable<RelationshipTieUpOutputDTO> outputDTO;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using(var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader =await connection.QueryMultipleAsync(
                        "dbo.GetRelationshipTieUp",
                        CommandType.StoredProcedure);

                    outputDTO = await reader.ReadAsync<RelationshipTieUpOutputDTO>();
                }
                return outputDTO;
            }
        }
    }
}
