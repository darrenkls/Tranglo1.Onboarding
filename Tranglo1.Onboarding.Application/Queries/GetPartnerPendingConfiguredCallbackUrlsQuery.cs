using AutoMapper;
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
    internal class GetPartnerPendingConfiguredCallbackUrlsQuery : BaseQuery<Result<CallbackURLOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<CallbackURLOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get pending configured callback URLs";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);

        }
    }

    internal class GetPartnerPendingConfiguredCallbackUrlsQueryHandler : IRequestHandler<GetPartnerPendingConfiguredCallbackUrlsQuery, Result<CallbackURLOutputDTO>>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GetPartnerPendingConfiguredCallbackUrlsQueryHandler> _logger;
        public GetPartnerPendingConfiguredCallbackUrlsQueryHandler(IConfiguration config, ILogger<GetPartnerPendingConfiguredCallbackUrlsQueryHandler> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<Result<CallbackURLOutputDTO>> Handle(GetPartnerPendingConfiguredCallbackUrlsQuery request, CancellationToken cancellationToken)
        {
            CallbackURLOutputDTO result;
            var _connectionString = _config.GetConnectionString("DefaultConnection");
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "GetPartnerPendingConfiguredCallbackURL",
                       new
                       {
                           PartnerCode = request.PartnerCode,
                           PartnerSubscriptionCode = request.PartnerSubscriptionCode
                       },
                       null, null, CommandType.StoredProcedure); ;
                    result = reader.ReadFirstOrDefault<CallbackURLOutputDTO>();
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetPartnerPendingConfiguredCallbackUrlsQuery] {ex.Message}");
            }

            return Result.Failure<CallbackURLOutputDTO>(
                        $"Get partner pending configured callback urls failed."
                    );
        }
    }
}
