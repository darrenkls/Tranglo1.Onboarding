using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Application.MediatR;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetDisplayRuleByQuestionnaireCodeQuery : BaseQuery<AMLCFTDisplayRuleOutputDTO>
    {
        public long QuestionnaireCode { get; set; }
        public class GetDisplayRuleByQuestionnaireCodeQueryHandler : IRequestHandler<GetDisplayRuleByQuestionnaireCodeQuery, AMLCFTDisplayRuleOutputDTO>
        {
            private readonly IConfiguration _config;
            public GetDisplayRuleByQuestionnaireCodeQueryHandler(IConfiguration config)
            {
                _config = config;
            }
            async Task<AMLCFTDisplayRuleOutputDTO> IRequestHandler<GetDisplayRuleByQuestionnaireCodeQuery, AMLCFTDisplayRuleOutputDTO>.Handle(GetDisplayRuleByQuestionnaireCodeQuery request, CancellationToken cancellationToken)
            {
                
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                AMLCFTDisplayRuleOutputDTO displayRule;
                List<DisplayRules> displayRuleList = new List<DisplayRules>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "dbo.GetAMLCFTDisplayRule",
                        new
                        {
                           QuestionnaireCode = request.QuestionnaireCode
                        },
                        null, null,
                         CommandType.StoredProcedure);
                    displayRule = await reader.ReadFirstOrDefaultAsync<AMLCFTDisplayRuleOutputDTO>();
                    displayRuleList = (List<DisplayRules>)await reader.ReadAsync<DisplayRules>();
                    displayRule.DisplayRules = displayRuleList;
                }
                return displayRule;
            }
        }
    }
}
