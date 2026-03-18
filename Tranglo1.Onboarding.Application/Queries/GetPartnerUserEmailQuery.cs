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
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerUserEmailQuery : BaseQuery<IEnumerable<PartnerUserEmailOutputDTO>>
    {
         
    }

    internal class GetPartnerUserEmailQueryHandler : IRequestHandler<GetPartnerUserEmailQuery, IEnumerable<PartnerUserEmailOutputDTO>>
    {
        private readonly ILogger<PartnerUserEmailOutputDTO> _logger;
        private readonly IConfiguration _config;
        private readonly IPartnerRepository _repository;
        
        public GetPartnerUserEmailQueryHandler(ILogger<PartnerUserEmailOutputDTO> logger, IConfiguration config,IPartnerRepository repository)
        {
            _logger = logger;
            _config = config;
            _repository = repository;

        }

        public async Task<IEnumerable<PartnerUserEmailOutputDTO>> Handle(GetPartnerUserEmailQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<PartnerUserEmailOutputDTO> outputDTO;
            var _connectionString = _config.GetConnectionString("DefaultConnection");
            using(var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "dbo.GetPartnerUserEmail",
                CommandType.StoredProcedure);

                outputDTO = await reader.ReadAsync<PartnerUserEmailOutputDTO>(); 
            }

            return outputDTO;
        }
    }
}
