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
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{

    [Permission(Permission.ManagePartnerPartnerDocuments.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {  })]
    internal class GetPartnerAgreementTemplatesQuery: BaseQuery<Result<List<PartnerAgreementTemplateOutputDTO>>>
    {

        public long PartnerCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public class GetPartnerAgreementTemplatesQueryHandler : IRequestHandler<GetPartnerAgreementTemplatesQuery, Result<List<PartnerAgreementTemplateOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetPartnerAgreementTemplatesQueryHandler> _logger;

            public GetPartnerAgreementTemplatesQueryHandler(IConfiguration config, ILogger<GetPartnerAgreementTemplatesQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<PartnerAgreementTemplateOutputDTO>>> Handle(GetPartnerAgreementTemplatesQuery request, CancellationToken cancellationToken)
            {
                try
                {
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
                        return Result.Failure<List<PartnerAgreementTemplateOutputDTO>>("Invalid solution code.");
                    }
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetPartnerAgreementTemplate",
                           new
                           {
                               SolutionCode = solutionCodeInput,
                               PartnerCode = request.PartnerCode
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<PartnerAgreementTemplateOutputDTO>)await reader.ReadAsync<PartnerAgreementTemplateOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError($"[GetPartnerAgreementTemplatesQuery] {ex.Message}");
                }
                return Result.Failure<List<PartnerAgreementTemplateOutputDTO>>(
                                $"Get partner agreement templates failed for {request.PartnerCode}."
                            );
            }
        }
    }
}