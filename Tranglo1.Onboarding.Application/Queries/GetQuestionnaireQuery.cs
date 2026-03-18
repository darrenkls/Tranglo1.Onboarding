using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOnlineAMLCFTQuestionnaires, UACAction.View)]
    [Permission(Permission.KYCAdministrationOnlineAMLCFTQuestionnaires.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]
    internal class GetQuestionnaireQuery : BaseQuery<IEnumerable<AdminAMLCFTQuestionnaireOutputDTO>>
    {
        public long QuestionnaireCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<AdminAMLCFTQuestionnaireOutputDTO> result)
        {
            string _description = $"Get AMLCFT Questionnaire for QuestionnaireCode: [{this.QuestionnaireCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetQuestionnaireQueryHandler : IRequestHandler<GetQuestionnaireQuery, IEnumerable<AdminAMLCFTQuestionnaireOutputDTO>>
    {
        private readonly IConfiguration _config;
        public GetQuestionnaireQueryHandler(IConfiguration config)
        {
            _config = config;
        }

        async Task<IEnumerable<AdminAMLCFTQuestionnaireOutputDTO>> IRequestHandler<GetQuestionnaireQuery, IEnumerable<AdminAMLCFTQuestionnaireOutputDTO>>.Handle(GetQuestionnaireQuery request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");

            IEnumerable<AdminAMLCFTQuestionnaireOutputDTO> questionnaireDTOs;
            IEnumerable<QuestionnaireSolutionsOutputDTO> questionnaireSolutions;
            IEnumerable<AdminAMLCFTQuestionSectionDTO> questionSectionDTOs;
            IEnumerable<AdminAMLCFTQuestionDTO> questionDTOs;
            IEnumerable<AdminAMLCFTAnswerDTO> answerDTOs;


            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetAMLCFTQuestionnaireByCode",
                    new { questionnaireCode = request.QuestionnaireCode },
                    null, null, CommandType.StoredProcedure);

                // read as IEnumerable<dynamic>
                questionnaireDTOs = await reader.ReadAsync<AdminAMLCFTQuestionnaireOutputDTO>();
                questionnaireSolutions = await reader.ReadAsync<QuestionnaireSolutionsOutputDTO>();
                questionSectionDTOs = await reader.ReadAsync<AdminAMLCFTQuestionSectionDTO>();
                questionDTOs = await reader.ReadAsync<AdminAMLCFTQuestionDTO>();
                answerDTOs = await reader.ReadAsync<AdminAMLCFTAnswerDTO>();


            }

            foreach (AdminAMLCFTQuestionnaireOutputDTO questionnaireDTO in questionnaireDTOs)
            {
                questionnaireDTO.QuestionnaireSolutions = questionnaireSolutions
                .Where(x => x.QuestionnaireCode == questionnaireDTO.QuestionnaireCode)
                .ToList();

                questionnaireDTO.QuestionSections = questionSectionDTOs.Where(x => x.QuestionnaireCode == questionnaireDTO.QuestionnaireCode).OrderBy(x => x.QuestionnaireCode).ToList();

                foreach (AdminAMLCFTQuestionSectionDTO questionSectionDTO in questionnaireDTO.QuestionSections)
                {
                    questionSectionDTO.Questions = questionDTOs.Where(x => x.QuestionSectionCode == questionSectionDTO.QuestionSectionCode && x.IsRoot).OrderBy(x => x.SequenceNo).ThenBy(x => x.QuestionSectionCode).ToList();
                    var childQuestionsUnderSection = questionDTOs.Where(x => x.QuestionSectionCode == questionSectionDTO.QuestionSectionCode && !x.IsRoot).ToList();

                    foreach (AdminAMLCFTQuestionDTO parentQuestion in questionSectionDTO.Questions)
                    {
                        BuildTree(parentQuestion, childQuestionsUnderSection, answerDTOs);
                    }
                }

              
            }


            return questionnaireDTOs;
        }

        private static void BuildTree(AdminAMLCFTQuestionDTO parentQuestion, List<AdminAMLCFTQuestionDTO> childQuestions, IEnumerable<AdminAMLCFTAnswerDTO> answerItems)
        {
            parentQuestion.ChildQuestions = childQuestions.Where(x => x.ParentQuestionCode == parentQuestion.QuestionCode).OrderBy(x => x.SequenceNo).ThenBy(x => x.QuestionCode).ToList();
            parentQuestion.ChildQuestions.OrderBy(x => x.SequenceNo).ThenBy(x => x.QuestionCode);
            parentQuestion.Answers = answerItems.Where(x => x.QuestionCode == parentQuestion.QuestionCode).OrderBy(x => x.SequenceNumber).ToList();
            parentQuestion.Answers.OrderBy(x => x.AnswerChoiceCode);

            foreach (AdminAMLCFTAnswerDTO answerDTO in parentQuestion.Answers)
            {
                answerDTO.ChildQuestions = childQuestions.Where(x => x.ParentAnswerCode == answerDTO.AnswerChoiceCode && x.ParentAnswerCode != null).OrderBy(x => x.SequenceNo).ToList();
                answerDTO.ChildQuestions.OrderBy(x => x.SequenceNo);
                foreach (AdminAMLCFTQuestionDTO childQuestion in answerDTO.ChildQuestions)
                {
                    BuildTree(childQuestion, childQuestions, answerItems);
                }
            }

            foreach (AdminAMLCFTQuestionDTO childQuestion in parentQuestion.ChildQuestions)
            {
                BuildTree(childQuestion, childQuestions, answerItems);
            }
        }
    }    
}
