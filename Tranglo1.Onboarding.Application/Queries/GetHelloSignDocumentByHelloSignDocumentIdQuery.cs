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
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.View)]
    internal class GetHelloSignDocumentByHelloSignDocumentIdQuery : BaseQuery<Result<HelloSignDocumentOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long HelloSignDocumentId { get; set; }

        public class GetHelloSignDocumentByHelloSignDocumentIdQueryHandler : IRequestHandler<GetHelloSignDocumentByHelloSignDocumentIdQuery, Result<HelloSignDocumentOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetHelloSignDocumentByHelloSignDocumentIdQueryHandler> _logger;

            public GetHelloSignDocumentByHelloSignDocumentIdQueryHandler(IConfiguration config, ILogger<GetHelloSignDocumentByHelloSignDocumentIdQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<HelloSignDocumentOutputDTO>> Handle(GetHelloSignDocumentByHelloSignDocumentIdQuery request, CancellationToken cancellationToken)
            {
                HelloSignDocumentOutputDTO result;
                var _connectionString = _config.GetConnectionString("DefaultConnection");

                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetHelloSignDocument",
                           new
                           {
                               PartnerCode = request.PartnerCode,
                               HelloSignDocumentId = request.HelloSignDocumentId
                           },
                           null, null, CommandType.StoredProcedure); ;

                        result = reader.ReadFirstOrDefault<HelloSignDocumentOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetHelloSignDocumentByHelloSignDocumentIdQuery] {ex.Message}");
                }
                return Result.Failure<HelloSignDocumentOutputDTO>(
                            $"Get hellosign document name failed for {request.PartnerCode}."
                        );
            }
        }
    }
}
