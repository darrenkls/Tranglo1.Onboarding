using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetAuthorisationLevelQuery : IRequest<IEnumerable<AuthorisationLevelOutputDTO>>
    {
        public class GetAuthorisationLevelQueryHandler : IRequestHandler<GetAuthorisationLevelQuery, IEnumerable<AuthorisationLevelOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IConfiguration _config;

            public GetAuthorisationLevelQueryHandler(ApplicationUserDbContext context,IConfiguration config)
            {
                _config = config;
                _context = context;
            }
             public async Task<IEnumerable<AuthorisationLevelOutputDTO>> Handle(GetAuthorisationLevelQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<AuthorisationLevelOutputDTO> outputDTO;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "dbo.GetAuthorisationLevel",
                        CommandType.StoredProcedure);

                    outputDTO = await reader.ReadAsync<AuthorisationLevelOutputDTO>();
                }

                return outputDTO;
            }
        }
    }
}
