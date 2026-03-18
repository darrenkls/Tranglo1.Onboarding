using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.Compliance, UACAction.View)]
    internal class GetKYCWatchlistProfileQuery : IRequest<KYCWatchlistProfileOutputDTO>
    {
        public long ScreeningInputCode { get; set; }

        public class GetKYCWatchlistProfileQueryHandler : IRequestHandler<GetKYCWatchlistProfileQuery, KYCWatchlistProfileOutputDTO>
        {
            private readonly IConfiguration _config;

            public GetKYCWatchlistProfileQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<KYCWatchlistProfileOutputDTO> Handle(GetKYCWatchlistProfileQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");


                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                       "GetCustomerInformation",
                       new
                       {
                           ScreeningInputCodeFilter = request.ScreeningInputCode
                       },
                       null, null, CommandType.StoredProcedure);

                    return await reader.ReadFirstOrDefaultAsync<KYCWatchlistProfileOutputDTO>();
                }
            }
        }
    }
}
