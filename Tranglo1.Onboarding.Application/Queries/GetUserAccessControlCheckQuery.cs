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
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetUserAccessControlCheckQuery : IRequest<Result<bool>>
    {
        public List<string> RoleCodes { get; set; }
        public string PermissionCode { get; set; }
        public string UserSolutionClaim { get; set; }
        public int[] PermissionPortalCodes { get; set; }

        public class GetUserAccessControlCheckQueryHandler : IRequestHandler<GetUserAccessControlCheckQuery, Result<bool>>
        {
            private readonly IConfiguration _config;
            private readonly AccessControlManager _accessControlManager;
            private readonly ILogger<GetUserAccessControlCheckQueryHandler> _logger;

            public GetUserAccessControlCheckQueryHandler(
                IConfiguration config,
                AccessControlManager accessControlManager,
                ILogger<GetUserAccessControlCheckQueryHandler> logger)
            {
                _config = config;
                _accessControlManager = accessControlManager;
                _logger = logger;
            }

            public async Task<Result<bool>> Handle(GetUserAccessControlCheckQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var uacConnectionString = _config.GetConnectionString("UACConnection");
                    var portalCodes = request.PermissionPortalCodes ?? Array.Empty<int>();

                    foreach (var roleCode in request.RoleCodes ?? Enumerable.Empty<string>())
                    {
                        var keyValuePairs = new Dictionary<string, string> { ["role"] = roleCode };
                        if (!string.IsNullOrEmpty(request.UserSolutionClaim))
                            keyValuePairs["solution"] = request.UserSolutionClaim;

                        var claimListing = _accessControlManager.GetClaimListing(keyValuePairs);

                        foreach (var portalCode in portalCodes)
                        {
                            using (var connection = new SqlConnection(uacConnectionString))
                            {
                                await connection.OpenAsync();

                                var reader = await connection.QueryMultipleAsync(
                                    "dbo.GetScreenAccessControlByClaims",
                                    new { Claims = claimListing, PortalCode = portalCode },
                                    null, null, CommandType.StoredProcedure);

                                var permissions = await reader.ReadAsync<PermissionResult>();

                                if (permissions.Any(p => p.PermissionInfoCode == request.PermissionCode))
                                    return Result.Success(true);
                            }
                        }
                    }

                    return Result.Success(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{0}]", nameof(GetUserAccessControlCheckQueryHandler));
                    return Result.Failure<bool>(ex.ToString());
                }
            }

            private sealed class PermissionResult
            {
                public string PermissionInfoCode { get; set; }
            }
        }
    }
}
