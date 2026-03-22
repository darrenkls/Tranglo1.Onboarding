using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetBusinessProfileListPerCustomerUserQuery : BaseQuery<List<GetBusinessProfileListPerCustomerUserOutputDTO>>
    {
        public string Email { get; set; }
        public int BusinessProfileCode { get; set; }
        public int CustomerUserRegistrationId { get; set; }

        public override Task<string> GetAuditLogAsync(List<GetBusinessProfileListPerCustomerUserOutputDTO> result)
        {
            string _description = $"Get all Business Profiles List";
            return Task.FromResult(_description);
        }        
    }

    internal class GetBusinessProfileListPerCustomerUserQueryHandler : IRequestHandler<GetBusinessProfileListPerCustomerUserQuery, List<GetBusinessProfileListPerCustomerUserOutputDTO>>
    {
        private readonly IConfiguration _config;

        public GetBusinessProfileListPerCustomerUserQueryHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<GetBusinessProfileListPerCustomerUserOutputDTO>> Handle(GetBusinessProfileListPerCustomerUserQuery request, CancellationToken cancellationToken)
        {
            List<GetBusinessProfileListPerCustomerUserOutputDTO> result = new List<GetBusinessProfileListPerCustomerUserOutputDTO>();
            var _connectionString = _config.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetBusinessProfileListPerCustomerUser",
                    new
                    {
                        Email = request.Email,
                        BusinessProfileCode = request.BusinessProfileCode,
                        CustomerUserRegistrationId = request.CustomerUserRegistrationId
                    },
                    null, null, CommandType.StoredProcedure);

                result = (List<GetBusinessProfileListPerCustomerUserOutputDTO>)await reader.ReadAsync<GetBusinessProfileListPerCustomerUserOutputDTO>();
            }

            return result;
        }
    }
}
