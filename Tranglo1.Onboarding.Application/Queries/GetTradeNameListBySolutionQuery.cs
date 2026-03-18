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
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetTradeNameListBySolutionQuery : BaseQuery<Result<List<TradeNameListBySolutionOutputDTO>>>
    {

        public long? AdminSolution { get; set; }

        public class GetTradeNameListBySolutionQueryHandler : IRequestHandler<GetTradeNameListBySolutionQuery, Result<List<TradeNameListBySolutionOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetTradeNameListBySolutionQueryHandler> _logger;

            public GetTradeNameListBySolutionQueryHandler(IConfiguration config, ILogger<GetTradeNameListBySolutionQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<TradeNameListBySolutionOutputDTO>>> Handle(GetTradeNameListBySolutionQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    long? solutionCodeInput = null;
                    if (request.AdminSolution == Solution.Business.Id)
                    {
                        solutionCodeInput = Solution.Business.Id;

                    }
                    else if (request.AdminSolution == Solution.Connect.Id)
                    {
                        solutionCodeInput = Solution.Connect.Id;
                    }
                    else
                    {
                        return Result.Failure<List<TradeNameListBySolutionOutputDTO>>("Invalid solution code.");
                    }
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetTradesNameBySolution",
                           new
                           {
                               SolutionCode = solutionCodeInput,
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<TradeNameListBySolutionOutputDTO>)await reader.ReadAsync<TradeNameListBySolutionOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetTradeNameListBySolutionQuery] {ex.Message}");
                }
                return Result.Failure<List<TradeNameListBySolutionOutputDTO>>(
                                $"Get trade name failed."
                            );
            }
        }
    }
}