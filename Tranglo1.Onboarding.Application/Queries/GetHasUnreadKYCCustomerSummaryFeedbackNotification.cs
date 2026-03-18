using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.KYCCustomerSummaryFeedbackNotification;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementReviewSummary.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class GetHasUnreadKYCCustomerSummaryFeedbackNotificationQuery 
        : BaseQuery<Result<GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto>>
    {
        public int BusinessProfileCode { get; set; }
        public int? AdminSolution { get; set; }

        internal class GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler 
            : IRequestHandler<GetHasUnreadKYCCustomerSummaryFeedbackNotificationQuery, Result<GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto>>
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

            public async Task<Result<GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto>> Handle(GetHasUnreadKYCCustomerSummaryFeedbackNotificationQuery request, CancellationToken cancellationToken)
            {
                GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto response = new GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto();

                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        response.HasUnread = await connection.ExecuteScalarAsync<bool>("HasUnreadKYCCustomerSummaryFeedbackNotificationByBusinessProfile",
                            new
                            {
                                SolutionCode = request.AdminSolution,
                                BusinessProfileCode = request.BusinessProfileCode,
                            },
                            null, null, CommandType.StoredProcedure);

                        return response;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{0}]", nameof(GetHasUnreadKYCCustomerSummaryFeedbackNotificationQueryHandler));

                    return Result.Failure<GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto>("Unable to retrieve KYC Customer Feedback Summary notification(s).");
                }
            }
        }
    }
}
