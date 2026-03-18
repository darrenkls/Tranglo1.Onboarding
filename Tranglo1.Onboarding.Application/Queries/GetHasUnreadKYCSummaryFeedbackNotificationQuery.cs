using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementReviewSummary.Action_View_Code,
        new int[] { (int)PortalCode.Business },
        new string[] { })]
    internal class GetHasUnreadKYCSummaryFeedbackNotificationQuery
        : BaseQuery<Result<GetHasUnreadKYCSummaryFeedbackNotificationOutputDto>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }

        public class GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler
            : IRequestHandler<GetHasUnreadKYCSummaryFeedbackNotificationQuery, Result<GetHasUnreadKYCSummaryFeedbackNotificationOutputDto>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler> _logger;
            private readonly string _connectionString;

            public GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler(IConfiguration configuration,
                ILogger<GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler> logger)
            {
                _config = configuration;
                _logger = logger;
                _connectionString = _config.GetConnectionString("DefaultConnection");
            }

            public async Task<Result<GetHasUnreadKYCSummaryFeedbackNotificationOutputDto>> Handle(GetHasUnreadKYCSummaryFeedbackNotificationQuery request, CancellationToken cancellationToken)
            {
                long solutionCode;

                if (request.CustomerSolution == ClaimCode.Business)
                {
                    solutionCode = Solution.Business.Id;
                }
                else if (request.CustomerSolution == ClaimCode.Connect)
                {
                    solutionCode = Solution.Connect.Id;
                }
                else
                {
                    return Result.Failure<GetHasUnreadKYCSummaryFeedbackNotificationOutputDto>("Invalid solution code.");
                }

                GetHasUnreadKYCSummaryFeedbackNotificationOutputDto response = new GetHasUnreadKYCSummaryFeedbackNotificationOutputDto();

                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        var parameters = new DynamicParameters();
                        parameters.Add("@SolutionCode", solutionCode);
                        parameters.Add("@BusinessProfileCode", request.BusinessProfileCode);
                        parameters.Add("@UnreadNotificationCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        response.HasUnread = await connection.ExecuteScalarAsync<bool>("HasUnreadKYCSummaryFeedbackNotificationByBusinessProfile",
                            param: parameters,
                            null, null, CommandType.StoredProcedure);

                        response.UnreadNotificationCount = parameters.Get<int>("@UnreadNotificationCount");

                        return response;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{0}]", nameof(GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler));

                    return Result.Failure<GetHasUnreadKYCSummaryFeedbackNotificationOutputDto>("Unable to retrieve KYC Customer Feedback Summary notification(s).");
                }
            }
        }
    }
}
