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
using Tranglo1.Onboarding.Application.DTO.Watchlist;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetRBACountersQuery : IRequest<RBACounterOutputDTO>
    {
        public int BusinessProfileCode { get; set; }
        public int OwnershipStructureType { get; set; }
        public int TableId { get; set; }

        public class GetRBACountersQueryHandler : IRequestHandler<GetRBACountersQuery, RBACounterOutputDTO>
        {
            private readonly IConfiguration _config;

            public GetRBACountersQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<RBACounterOutputDTO> Handle(GetRBACountersQuery request, CancellationToken cancellationToken)
            {
                RBACounterOutputDTO result = new RBACounterOutputDTO();
                {
                    result.RBAResult = " ";
                    result.DateAndTime = DateTime.Now;
                    result.RiskLevel = " ";
                    result.RiskScore = " ";
                }
               /* var _connectionString = _config.GetConnectionString("DefaultConnection");


                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       " ",
                       new
                       {
                           BusinessProfileCode = request.BusinessProfileCode,
                           OwnershipStructureTypeId = request.OwnershipStructureType,
                           tableId = request.TableId
                       },
                       null, null, CommandType.StoredProcedure); ;


                    result = reader.ReadFirstOrDefault<RBACounterOutputDTO>();*/

                
                    return result;
                
            }
        }
    }
}

