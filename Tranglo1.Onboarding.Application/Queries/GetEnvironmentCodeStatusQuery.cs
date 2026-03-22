using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetEnvironmentCodeStatusQuery : BaseQuery<Result<PartnerEnvironmentOutputDTO>>
    {
        public long PartnerCode { get; set; }


        public override Task<string> GetAuditLogAsync(Result<PartnerEnvironmentOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get Environment Code Status for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetEnvironmentCodeStatusQueryHandler : IRequestHandler<GetEnvironmentCodeStatusQuery, Result<PartnerEnvironmentOutputDTO>>
    {
        private readonly IConfiguration _config;

        public GetEnvironmentCodeStatusQueryHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task <Result<PartnerEnvironmentOutputDTO>> Handle (GetEnvironmentCodeStatusQuery request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");
            PartnerEnvironmentOutputDTO partnerEnvironmentOutputDTO;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                partnerEnvironmentOutputDTO = await connection.QueryFirstOrDefaultAsync<PartnerEnvironmentOutputDTO>(
                           "GetEnvironmentCodeStatusQuery",
                           new
                           {
                               PartnerCode = request.PartnerCode
                           },
                           null, null, CommandType.StoredProcedure); ;
            }
            return Result.Success(partnerEnvironmentOutputDTO);


         
        }

        
    }
}
