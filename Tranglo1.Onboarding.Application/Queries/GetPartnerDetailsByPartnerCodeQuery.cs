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
    [Permission(Permission.ManagePartnerPartnerDetails.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]
    internal class GetPartnerDetailsByPartnerCodeQuery : BaseQuery<Result<PartnerDetailsOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public string TrangloEntity { get; set; }

        public class GetPartnerDetailsByPartnerCodeQueryHandler : IRequestHandler<GetPartnerDetailsByPartnerCodeQuery, Result<PartnerDetailsOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetPartnerDetailsByPartnerCodeQueryHandler> _logger;

            public GetPartnerDetailsByPartnerCodeQueryHandler(IConfiguration config, ILogger<GetPartnerDetailsByPartnerCodeQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<PartnerDetailsOutputDTO>> Handle(GetPartnerDetailsByPartnerCodeQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    PartnerDetailsOutputDTO outputDTO = new PartnerDetailsOutputDTO();
                    IEnumerable<PartnerDetail> partnerDetails;

                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetPartnerDetails",
                           new
                           {
                               PartnerCode = request.PartnerCode,
                               TrangloEntity = request.TrangloEntity
                           },
                           null, null, CommandType.StoredProcedure); ;

                        partnerDetails = await reader.ReadAsync<PartnerDetail>();
                    }
                    outputDTO.PartnerCode = request.PartnerCode;
                    outputDTO.PartnerDetails = partnerDetails.ToList();

                    return Result.Success(outputDTO);
                }

                catch (Exception ex)
                {
                    _logger.LogError($"[GetPartnerDetailsByPartnerCodeQuery] {ex.Message}");
                }

                return Result.Failure<PartnerDetailsOutputDTO>($"Get partner details failed for {request.PartnerCode}");
            }
        }
    }
}
