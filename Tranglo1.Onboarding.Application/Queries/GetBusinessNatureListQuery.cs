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
    public class GetBusinessNatureListQuery : IRequest<IEnumerable<BusinessNatureListOutputDTO>>
    {
        public class GetBusinessNatureListQueryHandler : IRequestHandler<GetBusinessNatureListQuery, IEnumerable<BusinessNatureListOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetBusinessNatureListQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<BusinessNatureListOutputDTO>> Handle(GetBusinessNatureListQuery query, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");


                IEnumerable<BusinessNatureListOutputDTO> businessNatureListOutputDtos;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetBusinessNatures",
                        CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    businessNatureListOutputDtos = await reader.ReadAsync<BusinessNatureListOutputDTO>();

                    
                }
                return businessNatureListOutputDtos;
            }
        }
    }
}
