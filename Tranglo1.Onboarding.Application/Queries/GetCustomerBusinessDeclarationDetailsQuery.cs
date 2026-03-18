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
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.BusinessDeclaration;

namespace Tranglo1.Onboarding.Application.Queries
{

    public class GetCustomerBusinessDeclarationDetailsQuery : IRequest<GetCustomerBusinessDeclarationDetailsOutputDTO>
    {
        public int BusinessProfileCode { get; set; }

        public class GetCustomerBusinessDeclarationDetailsQueryHandler : IRequestHandler<GetCustomerBusinessDeclarationDetailsQuery, GetCustomerBusinessDeclarationDetailsOutputDTO>
        {
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IConfiguration _config;

            public GetCustomerBusinessDeclarationDetailsQueryHandler(
                IBusinessProfileRepository businessProfileRepository, 
                IPartnerRepository partnerRepository, 
                IConfiguration config)
            {
                _businessProfileRepository = businessProfileRepository;
                _partnerRepository = partnerRepository;
                _config = config;
            }

            public async Task<GetCustomerBusinessDeclarationDetailsOutputDTO> Handle(GetCustomerBusinessDeclarationDetailsQuery request, CancellationToken cancellationToken)
            {
                var outputDTO = new GetCustomerBusinessDeclarationDetailsOutputDTO();

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryFirstOrDefaultAsync(
                        "GetCustomerBusinessDeclarationDetails",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode
                        },
                        null, null, CommandType.StoredProcedure);

                    outputDTO = await reader.ReadFirstAsync<GetCustomerBusinessDeclarationOutputDTO>();
                }

                return outputDTO;
            }
        }
    }
}