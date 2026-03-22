using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Documentation.AdminTemplateOutputDTO;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetAdminTemplateList : BaseQuery<IEnumerable<AdminTemplateOutputDTO>>
    {
        public int SolutionCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<AdminTemplateOutputDTO> result)
        {
            string _description = $"Get Admin Template List for SolutionCode: [{this.SolutionCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetAdminTemplateListHandler : IRequestHandler<GetAdminTemplateList, IEnumerable<AdminTemplateOutputDTO>>
    {
        private readonly IConfiguration _config;

        public GetAdminTemplateListHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<AdminTemplateOutputDTO>> Handle(GetAdminTemplateList request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetAdminTemplateList",
                    new { SolutionCode = request.SolutionCode },
                    null, null, CommandType.StoredProcedure);

                var templates = (await reader.ReadAsync<AdminTemplateOutputDTO>()).ToList();
                var entities = (await reader.ReadAsync<TrangloEntity>()).ToList();

                foreach (var template in templates)
                {
                    template.TrangloEntities = entities
                        .Where(e => e.CategoryId == template.CategoryId && e.QuestionnaireCode == template.QuestionnaireCode)
                        .ToList();
                }

                return templates;
            }
        }
    }
}
