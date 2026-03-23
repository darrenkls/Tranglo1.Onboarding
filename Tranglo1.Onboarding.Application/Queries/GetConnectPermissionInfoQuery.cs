using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.TrangloRole;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetConnectPermissionInfoQuery : IRequest<IEnumerable<ConnectPermissionInfoOutputDTO>>
    {
        public long SolutionCode { get; set; }

        public class GetConnectPermissionInfoQueryHandler : IRequestHandler<GetConnectPermissionInfoQuery, IEnumerable<ConnectPermissionInfoOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetConnectPermissionInfoQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<ConnectPermissionInfoOutputDTO>> Handle(GetConnectPermissionInfoQuery request, CancellationToken cancellationToken)
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<GetConnectPermissionInfoOutputDTO> rawResults;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetConnectPermissionInfo",
                        new { SolutionCode = request.SolutionCode },
                        commandType: CommandType.StoredProcedure);

                    rawResults = await reader.ReadAsync<GetConnectPermissionInfoOutputDTO>();
                }

                var grouped = rawResults
                    .GroupBy(x => x.Menu)
                    .Select(g => new ConnectPermissionInfoOutputDTO
                    {
                        MainMenu = g.Key,
                        PermissionInfoActions = g.Select(x => new PermissionInfoAction2
                        {
                            PermissionGroup = x.PermissionGroup,
                            View = x.IsView,
                            Create = x.IsCreate,
                            Edit = x.IsEdit,
                            Approve = x.IsApprove,
                            Menu = x.Menu
                        }).ToList()
                    });

                return grouped;
            }
        }
    }
}
