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
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerNameListByTrangloEntityQuery : BaseQuery<Result<List<PartnerNameListbyTrangloEntityOutputDTO>>>
    {
        public string TrangloEntity { get; set; }


        public class GetPartnerNameListByTrangloEntityQueryHandler : IRequestHandler<GetPartnerNameListByTrangloEntityQuery, Result<List<PartnerNameListbyTrangloEntityOutputDTO>>>
        {
            private readonly IConfiguration _config;

            public GetPartnerNameListByTrangloEntityQueryHandler(IConfiguration config)
            {
                _config = config;
            }


            public async Task<Result<List<PartnerNameListbyTrangloEntityOutputDTO>>> Handle(GetPartnerNameListByTrangloEntityQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var reader = await connection.QueryMultipleAsync(
                   "GetPartnersNameByTrangloEntity",
                   new
                   {
                       TrangloEntity = request.TrangloEntity
                   },
                   null, null, CommandType.StoredProcedure); ;
                var result = (List<PartnerNameListbyTrangloEntityOutputDTO>)await reader.ReadAsync<PartnerNameListbyTrangloEntityOutputDTO>();
                return Result.Success(result);
            }

        }
    }
}
