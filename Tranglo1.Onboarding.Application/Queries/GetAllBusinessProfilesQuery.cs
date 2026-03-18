using AutoMapper;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetAllBusinessProfilesQuery : BaseQuery<List<GetAllBusinessProfilesOutputDTO>>
    {
        public string Email { get; set; }

        public override Task<string> GetAuditLogAsync(List<GetAllBusinessProfilesOutputDTO> result)
        {
            string _description = $"Get list of all Business Profiles";
            return Task.FromResult(_description);
        }
    }

    internal class GetAllBusinessProfilesQueryHandler : IRequestHandler<GetAllBusinessProfilesQuery, List<GetAllBusinessProfilesOutputDTO>>
    {
        private readonly BusinessProfileDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public GetAllBusinessProfilesQueryHandler(BusinessProfileDbContext context, IMapper mapper, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
        }

        public async Task<List<GetAllBusinessProfilesOutputDTO>> Handle(GetAllBusinessProfilesQuery request, CancellationToken cancellationToken)
        {
            List<GetAllBusinessProfilesOutputDTO> result = new List<GetAllBusinessProfilesOutputDTO>();
            var _connectionString = _config.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetAllBusinessProfiles",
                    new
                    {

                    },
                    null, null, CommandType.StoredProcedure);

                result = (List<GetAllBusinessProfilesOutputDTO>)await reader.ReadAsync<GetAllBusinessProfilesOutputDTO>();
            }

            return result;
        }
    }
}
