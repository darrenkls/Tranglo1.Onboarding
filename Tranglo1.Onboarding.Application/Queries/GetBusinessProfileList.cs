using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetBusinessProfileList: BaseQuery<List<BusinessProfileListOutputDTO>>
    {
        public override Task<string> GetAuditLogAsync(List<BusinessProfileListOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Business Profile List";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Business Profile List";
            return Task.FromResult(_description);                 
        }

        public class GetBusinessProfileTestHandler : IRequestHandler<GetBusinessProfileList, List<BusinessProfileListOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetBusinessProfileTestHandler(IConfiguration config)
            {
                _config = config;
            }
            public async Task<List<BusinessProfileListOutputDTO>> Handle(GetBusinessProfileList request, CancellationToken cancellationToken)
            {

                List<BusinessProfileListOutputDTO> result = new List<BusinessProfileListOutputDTO>();
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
             
                    var reader = await connection.QueryMultipleAsync(
                        "GetBusinessProfileList",
                        new
                        {

                        },
                        null, null, CommandType.StoredProcedure);
                    result = (List<BusinessProfileListOutputDTO>)await reader.ReadAsync<BusinessProfileListOutputDTO>();
                }
                result = result.OrderBy(x => x.CompanyName).ToList();
                return result;
            }
        }
    }
}
