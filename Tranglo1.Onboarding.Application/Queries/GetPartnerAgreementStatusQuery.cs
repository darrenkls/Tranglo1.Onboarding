using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerAgreementStatusQuery : BaseQuery<Result<PartnerAgreementStatusOutputDTO>>
    {
        public long PartnerCode { get; set; }

        public class GetPartnerAgreementStatusQueryHandler : IRequestHandler<GetPartnerAgreementStatusQuery, Result<PartnerAgreementStatusOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetPartnerAgreementStatusQueryHandler> _logger;

            public GetPartnerAgreementStatusQueryHandler(IConfiguration config, ILogger<GetPartnerAgreementStatusQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<PartnerAgreementStatusOutputDTO>> Handle(GetPartnerAgreementStatusQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    PartnerAgreementStatusOutputDTO partnerAgreementDetails;

                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        partnerAgreementDetails = await connection.QueryFirstOrDefaultAsync<PartnerAgreementStatusOutputDTO>(
                           "GetPartnerAgreementDetails",
                           new
                           {
                               PartnerCode = request.PartnerCode
                           },
                           null, null, CommandType.StoredProcedure); ;
                    }

                    if(partnerAgreementDetails.AgreementEndDate!= null && partnerAgreementDetails.AgreementEndDate < DateTime.UtcNow)
                    {
                        partnerAgreementDetails.PartnerAgreementStatus = "Expired";
                        partnerAgreementDetails.PartnerAgreementStatusCode = 2;
                    }
                    return Result.Success(partnerAgreementDetails);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetPartnerAgreementStatusQuery] {ex.Message}");
                }
                return Result.Failure<PartnerAgreementStatusOutputDTO>(
                            $"Get partner agreement status failed for {request.PartnerCode}."
                        );
            }
        }
    }
}
