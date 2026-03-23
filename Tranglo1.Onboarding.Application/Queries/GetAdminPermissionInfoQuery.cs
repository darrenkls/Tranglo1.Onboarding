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
    public class GetAdminPermissionInfoQuery : IRequest<IEnumerable<AdminPermissionInfoOutputDTO>>
    {
        public class GetAdminPermissionInfoQueryHandler : IRequestHandler<GetAdminPermissionInfoQuery, IEnumerable<AdminPermissionInfoOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetAdminPermissionInfoQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<AdminPermissionInfoOutputDTO>> Handle(GetAdminPermissionInfoQuery request, CancellationToken cancellationToken)
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<GetAdminPermissionInfoOutputDTO> rawResults;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetAdminPermissionInfo",
                        commandType: CommandType.StoredProcedure);

                    rawResults = await reader.ReadAsync<GetAdminPermissionInfoOutputDTO>();
                }

                var grouped = rawResults
                    .GroupBy(x => x.Menu)
                    .Select(g => new AdminPermissionInfoOutputDTO
                    {
                        MainMenu = g.Key,
                        PermissionInfoActions = g.Select(x => new PermissionInfoAction
                        {
                            PermissionGroup = x.PermissionGroup,
                            View = x.IsView,
                            Create = x.IsCreate,
                            Edit = x.IsEdit,
                            Approve = x.IsApprove
                        }).ToList()
                    });

                return grouped;
            }
        }
    }
}
