using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;
using UserType = Tranglo1.Onboarding.Infrastructure.Services.UserType;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCSummary, UACAction.View)]
    [Permission(Permission.KYCManagementReviewSummary.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetKYCSummaryFeedbackByBusinessProfileQuery : BaseQuery<Result<IEnumerable<KYCSummaryFeedbackOutputDTO>>>
    {
        public int BusinessProfileCode { get; set; }
        public long? KYCCategoryCode { get; set; }
        public int? KYCSummaryFeedbackCode { get; set; }
        public UserType UserType { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<KYCSummaryFeedbackOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Summary Feedback for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }


        public class GetKYCSummaryFeedbackByBusinessProfileQueryHandler : IRequestHandler<GetKYCSummaryFeedbackByBusinessProfileQuery, Result<IEnumerable<KYCSummaryFeedbackOutputDTO>>>
        {
            private readonly IConfiguration _config;

            public GetKYCSummaryFeedbackByBusinessProfileQueryHandler(IConfiguration config, BusinessProfileService businessProfileService)
            {
                _config = config;
            }

            public async Task<Result<IEnumerable<KYCSummaryFeedbackOutputDTO>>> Handle(GetKYCSummaryFeedbackByBusinessProfileQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                long? solutionCodeInput = null;

                IEnumerable<KYCSummaryFeedbackOutputDTO> kycSummaryFeedback;

                if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
                {
                    solutionCodeInput = Solution.Business.Id;

                }
                else if (request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id)
                {
                    solutionCodeInput = Solution.Connect.Id;
                }
                else
                {
                    return Result.Failure<IEnumerable<KYCSummaryFeedbackOutputDTO>>("Invalid solution code.");
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    kycSummaryFeedback = await connection.QueryAsync<KYCSummaryFeedbackOutputDTO>(
                        "dbo.GetKYCSummaryFeedback",
                        new
                        {
                            SolutionCode = solutionCodeInput,
                            BusinessProfileCode = request.BusinessProfileCode,
                            KYCCategoryCode = request.KYCCategoryCode,
                            KYCSummaryFeedbackCode = request.KYCSummaryFeedbackCode
                        },
                        null, null, CommandType.StoredProcedure);
                }

                if (request.UserType == UserType.External)
                {
                    foreach (var x in kycSummaryFeedback)
                    {
                        x.InternalRemarks = "";
                    }
                }

                return Result.Success(kycSummaryFeedback);
            }
        }
    }
}
