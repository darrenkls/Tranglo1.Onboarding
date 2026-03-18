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
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;
using UserType = Tranglo1.Onboarding.Infrastructure.Services.UserType;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.View)]
    [Permission(Permission.ManagePartnerPartnerDocuments.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] {})]
    internal class GetSignedPartnerAgreementsQuery: BaseQuery<Result<List<SignedPartnerAgreementOutputDTO>>>
    {
        public long PartnerCode { get; set; }
        public UserType UserType { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public class GetSignedPartnerAgreementsQueryHandler : IRequestHandler<GetSignedPartnerAgreementsQuery, Result<List<SignedPartnerAgreementOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetSignedPartnerAgreementsQueryHandler> _logger;

            public GetSignedPartnerAgreementsQueryHandler(IConfiguration config, ILogger<GetSignedPartnerAgreementsQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<SignedPartnerAgreementOutputDTO>>> Handle(GetSignedPartnerAgreementsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    bool? isDisplay = request.UserType == UserType.External ? true : (bool?)null;

                    long? solutionCodeInput = null;
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
                        return Result.Failure<List<SignedPartnerAgreementOutputDTO>>("Invalid solution code.");
                    }

                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetSignedPartnerAgreement",
                           new
                           {
                               SolutionCode = solutionCodeInput,
                               PartnerCode = request.PartnerCode,
                               isDisplay = isDisplay
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<SignedPartnerAgreementOutputDTO>)await reader.ReadAsync<SignedPartnerAgreementOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetSignedPartnerAgreementsQuery] {ex.Message}");
                }
                return Result.Failure<List<SignedPartnerAgreementOutputDTO>>(
                            $"Get signed partner agreements failed for {request.PartnerCode}."
                        );
            }
        }

    }
}
