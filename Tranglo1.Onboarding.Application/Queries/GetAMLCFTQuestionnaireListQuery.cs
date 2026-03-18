using CSharpFunctionalExtensions;
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
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOnlineAMLCFTQuestionnaires, UACAction.View)]
    [Permission(Permission.KYCAdministration.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {  })]
    internal class GetAMLCFTQuestionnaireListQuery : BaseQuery<Result<IEnumerable<QuestionnaireListOutputDTO>>>
    {
        public IEnumerable<QuestionnaireListOutputDTO> QuestionnaireListOutputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<QuestionnaireListOutputDTO>> result)
        {
            
            if (result.IsSuccess)
            {
                string _description = $"Get Questionnaire List]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);           
        }
    }

    internal class GetAMLCFTQuestionnaireListQueryHandler : IRequestHandler<GetAMLCFTQuestionnaireListQuery, Result<IEnumerable<QuestionnaireListOutputDTO>>>
    {
        private readonly IConfiguration _config;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public GetAMLCFTQuestionnaireListQueryHandler(IConfiguration config, IBusinessProfileRepository businessProfileRepository)
        {
            _config = config;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<IEnumerable<QuestionnaireListOutputDTO>>> Handle(GetAMLCFTQuestionnaireListQuery request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");

            IEnumerable<QuestionnaireListOutputDTO> questionnaireListOutputDTOs;
            IEnumerable<QuestionnaireSolutionList> questionnaireSolutionLists;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetQuestionnaireList",
                    new
                    {
                        
                    },
                    null, null, CommandType.StoredProcedure);

                // read as IEnumerable<dynamic>
                questionnaireListOutputDTOs = await reader.ReadAsync<QuestionnaireListOutputDTO>();
                questionnaireSolutionLists = await reader.ReadAsync<QuestionnaireSolutionList>();
            }

            var removeSolutions = new List<QuestionnaireSolutionList>();
            foreach (var q in questionnaireSolutionLists)
            {
                if (q.SolutionCode is null && q.SolutionDescription is null)
                {
                    removeSolutions.Add(q);
                }
            }

            var solutionList = questionnaireSolutionLists.ToList();
            solutionList.RemoveAll(x => removeSolutions.Contains(x));

            foreach (var q in questionnaireListOutputDTOs)
            {
                q.QuestionnaireSolutionList = solutionList
                        .Where(x => x.QuestionnaireCode == q.QuestionnaireCode)
                        .ToList();               
            }

            /*
            // API skeleton with mock data
            List<QuestionnaireSolutionList> solutionList = new List<QuestionnaireSolutionList>();
            var businessSolution = await _businessProfileRepository.GetSolutionByCodeAsync(Solution.Business.Id);
            var connectSolution = await _businessProfileRepository.GetSolutionByCodeAsync(Solution.Connect.Id);

            var solutionListBusiness = new QuestionnaireSolutionList();
            solutionListBusiness.SolutionCode = businessSolution.Id;
            solutionListBusiness.SolutionDescription = businessSolution.Name;

            var solutionListConnect = new QuestionnaireSolutionList();
            solutionListConnect.SolutionCode = connectSolution.Id;
            solutionListConnect.SolutionDescription = connectSolution.Name;

            solutionList.Add(solutionListConnect);
            solutionList.Add(solutionListBusiness);

            foreach (var s in QuestionnaireListOutputDTOs)
            {
                s.QuestionnaireSolutionList = solutionList;
            }
            */

            return Result.Success<IEnumerable<QuestionnaireListOutputDTO>>(questionnaireListOutputDTOs);
        }
    }
}
