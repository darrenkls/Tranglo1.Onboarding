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
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetAllExternalPartnerSubscriptionsQuery : BaseQuery<Result<List<ExternalPartnerSubsciptionListOutputDTO>>>
    {
        public long PartnerCode { get; set; }
        public string ExternalSolution { get; set; }

        public class GetAllExternalPartnerSubscriptionsQueryHandler : IRequestHandler<GetAllExternalPartnerSubscriptionsQuery, Result<List<ExternalPartnerSubsciptionListOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetAllExternalPartnerSubscriptionsQueryHandler> _logger;

            public GetAllExternalPartnerSubscriptionsQueryHandler(IConfiguration config, ILogger<GetAllExternalPartnerSubscriptionsQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<ExternalPartnerSubsciptionListOutputDTO>>> Handle(GetAllExternalPartnerSubscriptionsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    long? solutionCodeInput = null;
                    if (request.ExternalSolution == ClaimCode.Business )
                    {
                        solutionCodeInput = Solution.Business.Id;

                    }
                    else if (request.ExternalSolution == ClaimCode.Connect)
                    {
                        solutionCodeInput = Solution.Connect.Id;
                    }
                    else
                    {
                        return Result.Failure<List<ExternalPartnerSubsciptionListOutputDTO>>("Invalid solution code.");
                    }
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetSubscriptionsByPartnerCode&SolutionCode",
                           new
                           {
                               PartnerCode = request.PartnerCode,
                               SolutionCode = solutionCodeInput ,
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<ExternalPartnerSubsciptionListOutputDTO>)await reader.ReadAsync<ExternalPartnerSubsciptionListOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetAllExternalPartnerSubscriptionsQuery] {ex.Message}");
                }
                return Result.Failure<List<ExternalPartnerSubsciptionListOutputDTO>>(
                                $"Get partner subscriptions failed."
                            );
            }
        }
    }
}
