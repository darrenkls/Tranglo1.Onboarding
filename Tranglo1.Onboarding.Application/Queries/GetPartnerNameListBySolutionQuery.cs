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
    internal class GetPartnerNameListBySolutionQuery :BaseQuery<Result<List<PartnerNameListBySolutionOutputDTO>>>
    {

    public long? AdminSolution { get; set; }
    public string EntityCode { get; set; }

    public class GetPartnerNameListBySolutionQueryHandler : IRequestHandler<GetPartnerNameListBySolutionQuery, Result<List<PartnerNameListBySolutionOutputDTO>>>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GetPartnerNameListBySolutionQueryHandler> _logger;

        public GetPartnerNameListBySolutionQueryHandler(IConfiguration config, ILogger<GetPartnerNameListBySolutionQueryHandler> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<Result<List<PartnerNameListBySolutionOutputDTO>>> Handle(GetPartnerNameListBySolutionQuery request, CancellationToken cancellationToken)
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
                    return Result.Failure<List<PartnerNameListBySolutionOutputDTO>>("Invalid solution code.");
                }
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "GetPartnersNameBySolution",
                       new
                       {
                           SolutionCode = solutionCodeInput,
                           EntityCode = request.EntityCode
                       },
                       null, null, CommandType.StoredProcedure); ;
                    var result = (List<PartnerNameListBySolutionOutputDTO>)await reader.ReadAsync<PartnerNameListBySolutionOutputDTO>();
                    return Result.Success(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetPartnerNameListBySolutionQuery] {ex.Message}");
            }
            return Result.Failure<List<PartnerNameListBySolutionOutputDTO>>(
                            $"Get partner name failed."
                        );
        }
    }
}
}