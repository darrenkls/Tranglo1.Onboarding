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
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.View)]
    internal class GetHelloSignDocumentsQuery : BaseQuery<Result<List<HelloSignDocumentOutputDTO>>>
    {
        public long PartnerCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public class GetHelloSignDocumentsQueryHandler : IRequestHandler<GetHelloSignDocumentsQuery, Result<List<HelloSignDocumentOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetHelloSignDocumentsQueryHandler> _logger;

            public GetHelloSignDocumentsQueryHandler(IConfiguration config, ILogger<GetHelloSignDocumentsQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<HelloSignDocumentOutputDTO>>> Handle(GetHelloSignDocumentsQuery request, CancellationToken cancellationToken)
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
                        return Result.Failure<List<HelloSignDocumentOutputDTO>>("Invalid solution code.");
                    }


                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetHelloSignDocument",
                           new
                           {
                               SolutionCode = solutionCodeInput,
                               PartnerCode = request.PartnerCode,
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<HelloSignDocumentOutputDTO>)await reader.ReadAsync<HelloSignDocumentOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetHelloSignDocumentsQuery] {ex.Message}");
                }
                return Result.Failure<List<HelloSignDocumentOutputDTO>>(
                            $"Get hellosign document names failed for {request.PartnerCode}."
                        );
            }
        }
    }
}
