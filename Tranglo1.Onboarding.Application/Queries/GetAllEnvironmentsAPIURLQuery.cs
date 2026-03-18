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
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetAllEnvironmentsAPIURLQuery : BaseQuery<Result<List<APIURLOutputDTO>>>
    {
        public override Task<string> GetAuditLogAsync(Result<List<APIURLOutputDTO>> result)
        {
            string _description = $"Get all environment API URLs";
            return Task.FromResult(_description);
        }

        public class GetAllEnvironmentsAPIURLQueryHandler : IRequestHandler<GetAllEnvironmentsAPIURLQuery, Result<List<APIURLOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetAllEnvironmentsAPIURLQueryHandler> _logger;

            public GetAllEnvironmentsAPIURLQueryHandler(IConfiguration config, ILogger<GetAllEnvironmentsAPIURLQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }
            public async Task<Result<List<APIURLOutputDTO>>> Handle(GetAllEnvironmentsAPIURLQuery query, CancellationToken cancellationToken)
            {
                try
                {
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetAPIURLs",
                           new
                           {

                           },
                           null, null, CommandType.StoredProcedure); ;

                        var result = (List<APIURLOutputDTO>)await reader.ReadAsync<APIURLOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetAllEnvironmentsAPIURLQuery] {ex.Message}");
                }

                return Result.Failure<List<APIURLOutputDTO>>(
                            $"Get all environment API URLs failed."
                        );
            }
        }
    }
}
