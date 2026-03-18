using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetCompanyUserStatusQuery : IRequest<IEnumerable<CompanyUserStatusOutputDTO>>
    {
        public class GetCompanyUserStatusQueryHandler : IRequestHandler<GetCompanyUserStatusQuery, IEnumerable<CompanyUserStatusOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IMapper _mapper;
            private readonly IConfiguration _config;
            public GetCompanyUserStatusQueryHandler(ApplicationUserDbContext context, IMapper mapper, IConfiguration config)
            {
                _context = context;
                _mapper = mapper;
                _config = config;
            }
            public async Task<IEnumerable<CompanyUserStatusOutputDTO>> Handle(GetCompanyUserStatusQuery query, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                IEnumerable<CompanyUserStatusOutputDTO> combinedAccountStatuses;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetCombinedCompanyUserStatus",
                        CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    combinedAccountStatuses = await reader.ReadAsync<CompanyUserStatusOutputDTO>();
                }
                return combinedAccountStatuses;
            }
        }
    }
}
