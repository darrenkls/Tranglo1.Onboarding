using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.DTO.Partner;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetCreatedByQuery : BaseQuery<IEnumerable<CreatedByOutputDTO>>
    {

    }
    internal class GetCreatedByQueryHandler : IRequestHandler<GetCreatedByQuery, IEnumerable<CreatedByOutputDTO>>
    {
        private readonly IConfiguration _config;

        public GetCreatedByQueryHandler(IConfiguration config)
        {
            _config = config;
        }


        public async Task<IEnumerable<CreatedByOutputDTO>> Handle(GetCreatedByQuery request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");

            IEnumerable<CreatedByOutputDTO> DocumentDataDtos;


            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetPartnerKYCStatusRequisitionsCreatedBy",
                   CommandType.StoredProcedure);

                DocumentDataDtos = await reader.ReadAsync<CreatedByOutputDTO>();

            }
            return DocumentDataDtos;
        }
    }
}
