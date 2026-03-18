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
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerAgreementTemplateByTemplateIdQuery :  BaseQuery<Result<PartnerAgreementTemplateOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public Guid TemplateId { get; set; }

        public class GetPartnerAgreementTemplateByTemplateIdQueryHandler : IRequestHandler<GetPartnerAgreementTemplateByTemplateIdQuery, Result<PartnerAgreementTemplateOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetPartnerAgreementTemplateByTemplateIdQueryHandler> _logger;

            public GetPartnerAgreementTemplateByTemplateIdQueryHandler(IConfiguration config, ILogger<GetPartnerAgreementTemplateByTemplateIdQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<PartnerAgreementTemplateOutputDTO>> Handle(GetPartnerAgreementTemplateByTemplateIdQuery request, CancellationToken cancellationToken)
            {
                PartnerAgreementTemplateOutputDTO result;
                var _connectionString = _config.GetConnectionString("DefaultConnection");

                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetPartnerAgreementTemplate",
                           new
                           {
                               PartnerCode = request.PartnerCode,
                               TemplateId = request.TemplateId
                           },
                           null, null, CommandType.StoredProcedure); ;

                        // read as IEnumerable<dynamic>

                        result = reader.ReadFirstOrDefault<PartnerAgreementTemplateOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetPartnerAgreementTemplateByTemplateIdQuery] {ex.Message}");
                }
                return Result.Failure<PartnerAgreementTemplateOutputDTO>(
                            $"Get partner agreement template failed for {request.PartnerCode}."
                        );
            }
        }
    }
}
