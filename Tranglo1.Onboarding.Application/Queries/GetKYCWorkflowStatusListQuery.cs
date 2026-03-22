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
    public class GetKYCWorkflowStatusListQuery : IRequest<IEnumerable<WorkflowStatusListOutputDTO>>
    {
        public class GetKYCWorkflowStatusListQueryHandler : IRequestHandler<GetKYCWorkflowStatusListQuery, IEnumerable<WorkflowStatusListOutputDTO>>
        {
            private readonly IConfiguration _config;
            public GetKYCWorkflowStatusListQueryHandler(IConfiguration config)
            {
                _config = config;
            }

           

            public async Task<IEnumerable<WorkflowStatusListOutputDTO>> Handle(GetKYCWorkflowStatusListQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");


                IEnumerable<WorkflowStatusListOutputDTO> kycWorkflowStatusListOutputDtos;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetKYCWorkflowStatus",
                        CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    kycWorkflowStatusListOutputDtos = await reader.ReadAsync<WorkflowStatusListOutputDTO>();


                }
                return kycWorkflowStatusListOutputDtos;
            }
        }
    }
}
