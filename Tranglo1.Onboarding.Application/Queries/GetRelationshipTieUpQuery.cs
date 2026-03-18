using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class GetRelationshipTieUpQuery : IRequest<IEnumerable<RelationshipTieUpOutputDTO>>
    {
        public class GetRelationshipTieUpQueryHandler : IRequestHandler<GetRelationshipTieUpQuery, IEnumerable<RelationshipTieUpOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            private readonly IConfiguration _config;

            public GetRelationshipTieUpQueryHandler(ApplicationUserDbContext context, IMapper mapper,IConfiguration config)
            {
                _context = context;
                _mapper = mapper;
                _config = config;
            }

            public async Task<IEnumerable<RelationshipTieUpOutputDTO>> Handle(GetRelationshipTieUpQuery query, CancellationToken cancellationToken)
            {
                IEnumerable<RelationshipTieUpOutputDTO> outputDTO;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using(var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader =await connection.QueryMultipleAsync(
                        "dbo.GetRelationshipTieUp",
                        CommandType.StoredProcedure);

                    outputDTO = await reader.ReadAsync<RelationshipTieUpOutputDTO>();
                }
                return outputDTO;
            }
        }
    }
}
