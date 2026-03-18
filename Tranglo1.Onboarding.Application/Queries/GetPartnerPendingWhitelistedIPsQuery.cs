using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
	//[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.View)]
    internal class GetPartnerPendingWhitelistedIPsQuery : BaseQuery<Result<List<WhitelistIPAddressOutputDTO>>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }

        public class GetPartnerPendingWhitelistedIPsQueryHandler : IRequestHandler<GetPartnerPendingWhitelistedIPsQuery, Result<List<WhitelistIPAddressOutputDTO>>>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GetPartnerPendingWhitelistedIPsQueryHandler> _logger;
        public GetPartnerPendingWhitelistedIPsQueryHandler(IConfiguration config, ILogger<GetPartnerPendingWhitelistedIPsQueryHandler> logger)
        {
            _config = config;
            _logger = logger;
        }

            public async Task<Result<List<WhitelistIPAddressOutputDTO>>> Handle(GetPartnerPendingWhitelistedIPsQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetPartnerPendingWhitelistedIPAddresses",
                           new
                           {
                               PartnerCode = request.PartnerCode,
                               PartnerSubscriptionCode = request.PartnerSubscriptionCode
                           },
                           null, null, CommandType.StoredProcedure);
                        var result = (List<WhitelistIPAddressOutputDTO>)await reader.ReadAsync<WhitelistIPAddressOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetPartnerPendingWhitelistedIPsQuery] {ex.Message}");
                }

                return Result.Failure<List<WhitelistIPAddressOutputDTO>>(
                            $"Get partner pending whitelisted IP addresses failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
                        );
            }
        }
    }
}
