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
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerAPISettingsByPartnerCodeQuery : BaseQuery<Result<IEnumerable<APIEnvironmentSettingsOutputDTO>>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<APIEnvironmentSettingsOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get API Settings for PartnerSubscriptionCode: [{this.PartnerSubscriptionCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class GetPartnerAPISettingsByPartnerCodeQueryHandler : IRequestHandler<GetPartnerAPISettingsByPartnerCodeQuery, Result<IEnumerable<APIEnvironmentSettingsOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetPartnerAPISettingsByPartnerCodeQueryHandler> _logger;

            public GetPartnerAPISettingsByPartnerCodeQueryHandler(IConfiguration config, ILogger<GetPartnerAPISettingsByPartnerCodeQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<IEnumerable<APIEnvironmentSettingsOutputDTO>>> Handle(GetPartnerAPISettingsByPartnerCodeQuery query, CancellationToken cancellationToken)
            {
                try
                {
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    IEnumerable<APIEnvironmentSettingsOutputDTO> environmentSettingsDTOs;
                    IEnumerable<WhitelistIPAddressOutputDTO> IPDTOs;

                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                                   "GetPartnerAPISettingsByPartnerCode",
                                   new
                                   {
                                       PartnerCode = query.PartnerCode,
                                       PartnerSubscriptionCode = query.PartnerSubscriptionCode
                                   },
                                   null, null, CommandType.StoredProcedure);

                        environmentSettingsDTOs = await reader.ReadAsync<APIEnvironmentSettingsOutputDTO>();
                        IPDTOs = await reader.ReadAsync<WhitelistIPAddressOutputDTO>();
                    }
                    foreach (APIEnvironmentSettingsOutputDTO environmentSettingsDTO in environmentSettingsDTOs)
                    {
                        environmentSettingsDTO.APIAccessWhitelistIps = IPDTOs.Where(
                            x => x.PartnerCode == environmentSettingsDTO.PartnerCode && 
                            x.PartnerSubscriptionCode == environmentSettingsDTO.PartnerSubscriptionCode && 
                            x.EnvironmentCode == environmentSettingsDTO.EnvironmentCode).OrderBy(x => x.PartnerSubscriptionCode).ToList();
                    }
                    return Result.Success<IEnumerable<APIEnvironmentSettingsOutputDTO>>(environmentSettingsDTOs);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetPartnerAPISettingsByPartnerCodeQuery] {ex.Message}");
                }
                return Result.Failure<IEnumerable<APIEnvironmentSettingsOutputDTO>>(
                            $"Get partner API settings failed for PartnerCode: {query.PartnerCode} and PartnerSubscriptionCode: {query.PartnerSubscriptionCode}."
                        );

            }
        }
    }
}

