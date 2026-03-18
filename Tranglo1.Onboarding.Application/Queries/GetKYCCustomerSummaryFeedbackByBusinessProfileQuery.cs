using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
	//[Permission(PermissionGroupCode.KYCSummary, UACAction.View)]
	[Permission(Permission.KYCManagementReviewSummary.Action_View_Code,
		new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
		new string[] { })]
	internal class GetKYCCustomerSummaryFeedbackByBusinessProfileQuery : BaseQuery<Result<IEnumerable<KYCCustomerSummaryFeedbackOutputDTO>>>
	{
		public int BusinessProfileCode { get; set; }
		public long? KYCCategoryCode { get; set; } = null;
		public int? KYCSummaryFeedbackCode { get; set; } = null;
		public string CustomerSolution { get; set; }
		public long? AdminSolution { get; set; }

		public override Task<string> GetAuditLogAsync(Result<IEnumerable<KYCCustomerSummaryFeedbackOutputDTO>>result)
		{
			if(result.IsSuccess)
            {				            
				string _description = $"Get KYC Customer Summary Feedback for Business Profile Code: [{this.BusinessProfileCode}]";
				return Task.FromResult(_description);
			}

            return Task.FromResult<string>(null);
		}


		public class GetKYCCustomerSummaryFeedbackByBusinessProfileQueryHandler : IRequestHandler<GetKYCCustomerSummaryFeedbackByBusinessProfileQuery, Result<IEnumerable<KYCCustomerSummaryFeedbackOutputDTO>>>
		{
			private readonly IConfiguration _config;

			public GetKYCCustomerSummaryFeedbackByBusinessProfileQueryHandler(IConfiguration config)
			{
				_config = config;
			}

			public async Task<Result<IEnumerable<KYCCustomerSummaryFeedbackOutputDTO>>> Handle(GetKYCCustomerSummaryFeedbackByBusinessProfileQuery request, CancellationToken cancellationToken)
			{
				var _connectionString = _config.GetConnectionString("DefaultConnection");
				long? solutionCodeInput = null;

				IEnumerable<KYCCustomerSummaryFeedbackOutputDTO> kycCustomerSummaryFeedback;

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
					return Result.Failure<IEnumerable<KYCCustomerSummaryFeedbackOutputDTO>>("Invalid solution code.");
				}


				using (var connection = new SqlConnection(_connectionString))
				{
					await connection.OpenAsync();

					kycCustomerSummaryFeedback = await connection.QueryAsync<KYCCustomerSummaryFeedbackOutputDTO>(
						"dbo.GetKYCCustomerSummaryFeedback",
						new
						
						{
							SolutionCode = solutionCodeInput,
							BusinessProfileCode = request.BusinessProfileCode,
							KYCCategoryCode = request.KYCCategoryCode,
							KYCCustomerSummaryFeedbackCode = request.KYCSummaryFeedbackCode
						},
						null, null, CommandType.StoredProcedure);
				}

				
				return Result.Success(kycCustomerSummaryFeedback);
			}
		}
	}
}
