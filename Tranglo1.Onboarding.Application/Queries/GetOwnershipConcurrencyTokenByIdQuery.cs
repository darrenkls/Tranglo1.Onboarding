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
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetOwnershipConcurrencyTokenByIdQuery  : BaseQuery<IEnumerable<OwnershipConcurrencyTokenOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<OwnershipConcurrencyTokenOutputDTO> result)
        {

            string _description = $"Get Ownership Concurrency Token for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetOwnershipConcurrencyTokenByIdQueryHandler : IRequestHandler<GetOwnershipConcurrencyTokenByIdQuery, IEnumerable<OwnershipConcurrencyTokenOutputDTO>>
        {

            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly IConfiguration _config;

            public GetOwnershipConcurrencyTokenByIdQueryHandler(IBusinessProfileRepository repository, IConfiguration config, BusinessProfileService businessProfileService)
            {
                this._businessProfileRepository = repository;
                this._config = config;
                this._businessProfileService = businessProfileService;
            }
            public async Task<IEnumerable<OwnershipConcurrencyTokenOutputDTO>> Handle(GetOwnershipConcurrencyTokenByIdQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<OwnershipConcurrencyTokenOutputDTO> outputDTO;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "dbo.GetOwnershipConcurrencyTokenbyBusinessProfileCode",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode
                        },
                        null, null, CommandType.StoredProcedure);

                    outputDTO = await reader.ReadAsync<OwnershipConcurrencyTokenOutputDTO>();

                }

                return outputDTO;
            }
        }
    }
}
