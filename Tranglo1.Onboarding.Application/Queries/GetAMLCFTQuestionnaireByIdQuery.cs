using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCAMLCFT, UACAction.View)]
    [Permission(Permission.KYCManagementAMLCFT.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect , (int)PortalCode.Business },
        new string[] {})]
    internal class GetAMLCFTQuestionnaireByIdQuery : BaseQuery<IEnumerable<AMLCFTQuestionnaireOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; internal set; }
        public int TrangloEntityCode { get; set; }


        public override Task<string> GetAuditLogAsync(IEnumerable<AMLCFTQuestionnaireOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get AMLCFT Questionnaire for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get AMLCFT Questionnaire for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetAMLCFTQuestionnaireByIdQueryHandler : IRequestHandler<GetAMLCFTQuestionnaireByIdQuery, IEnumerable<AMLCFTQuestionnaireOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly IBusinessProfileRepository _businessProfileRepository;

            public GetAMLCFTQuestionnaireByIdQueryHandler(IConfiguration config,
                                    TrangloUserManager userManager,
                                    IBusinessProfileRepository businessProfileRepository)
            {
                _config = config;
                _userManager = userManager;
                _businessProfileRepository = businessProfileRepository;
            }


            private static readonly Dictionary<string, string> SolutionNameMap = new Dictionary<string, string>
            {
                { "business", Solution.Business.Name },
                { "connect", Solution.Connect.Name },
            };


            async Task<IEnumerable<AMLCFTQuestionnaireOutputDTO>> IRequestHandler<GetAMLCFTQuestionnaireByIdQuery, IEnumerable<AMLCFTQuestionnaireOutputDTO>>.Handle(GetAMLCFTQuestionnaireByIdQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<AMLCFTQuestionnaireOutputDTO> questionnaireDTOs;
                IEnumerable<AMLCFTQuestionnaireSolutionOutputDTO> questionnaireSolutionOutputDTOs;
                IEnumerable<AMLCFTQuestionSectionDTO> questionSectionDTOs;
                IEnumerable<AMLCFTQuestionDTO> questionDTOs;
                IEnumerable<AMLCFTAnswerDTO> answerDTOs;
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
                long? solutionCode = null;
                var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);

                if (applicationUser is CustomerUser)
                {
                    var claimSolutionName = request.CustomerSolution.ToLower(); // Assuming the claim value is lowercase
                    var solutionName = SolutionNameMap.ContainsKey(claimSolutionName)
                        ? SolutionNameMap[claimSolutionName]
                        : throw new ArgumentException("Invalid solution name from claims.");


                    solutionCode = await _businessProfileRepository.GetSolutionByNameAsync(solutionName);
                }
                else if(applicationUser is TrangloStaff)
                {
                    solutionCode = request.AdminSolution;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetAMLCFTQuestionnaire",
                        new { 
                        businessProfileCode = request.BusinessProfileCode,
                        solutionCode = solutionCode,
                        trangloEntityCode = request.TrangloEntityCode
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    questionnaireDTOs = await reader.ReadAsync<AMLCFTQuestionnaireOutputDTO>();
                    questionnaireSolutionOutputDTOs = await reader.ReadAsync<AMLCFTQuestionnaireSolutionOutputDTO>();
                    questionSectionDTOs = await reader.ReadAsync<AMLCFTQuestionSectionDTO>();
                    questionDTOs = await reader.ReadAsync<AMLCFTQuestionDTO>();
                    answerDTOs = await reader.ReadAsync<AMLCFTAnswerDTO>();
                }

                foreach(AMLCFTQuestionnaireOutputDTO questionnaireDTO in questionnaireDTOs)
                {
                    questionnaireDTO.QuestionnaireSolutions = questionnaireSolutionOutputDTOs
                    .Where(x => x.QuestionnaireCode == questionnaireDTO.QuestionnaireCode)
                    .ToList();
                    questionnaireDTO.AMLCFTQuestionnaireConcurrencyToken = businessProfile.AMLCFTQuestionnaireConcurrencyToken;
                    questionnaireDTO.QuestionSections = questionSectionDTOs.Where(x => x.QuestionnaireCode == questionnaireDTO.QuestionnaireCode).OrderBy(x=>x.QuestionnaireCode).ToList();

                    foreach(AMLCFTQuestionSectionDTO questionSectionDTO in questionnaireDTO.QuestionSections)
                    {
                        questionSectionDTO.Questions = questionDTOs.Where(x => x.QuestionSectionCode == questionSectionDTO.QuestionSectionCode && x.IsRoot).OrderBy(x => x.SequenceNo).ThenBy(x=>x.QuestionSectionCode).ToList();
                        var childQuestionsUnderSection = questionDTOs.Where(x => x.QuestionSectionCode == questionSectionDTO.QuestionSectionCode && !x.IsRoot).ToList();

                        foreach(AMLCFTQuestionDTO parentQuestion in questionSectionDTO.Questions)
                        {
                            BuildTree(parentQuestion, childQuestionsUnderSection, answerDTOs);
                        }
                    }
                }

                return questionnaireDTOs;

            }
        }
        private static void BuildTree(AMLCFTQuestionDTO parentQuestion, List<AMLCFTQuestionDTO> childQuestions, IEnumerable<AMLCFTAnswerDTO> answerItems)
        {
            parentQuestion.ChildQuestions = childQuestions.Where(x => x.ParentQuestionCode == parentQuestion.QuestionCode).OrderBy(x=>x.SequenceNo).ThenBy(x=>x.QuestionCode).ToList();
            parentQuestion.ChildQuestions.OrderBy(x => x.SequenceNo).ThenBy(x => x.QuestionCode);
            parentQuestion.Answers = answerItems.Where(x => x.QuestionCode == parentQuestion.QuestionCode).OrderBy(x=>x.SequenceNumber).ToList();
            parentQuestion.Answers.OrderBy(x => x.AnswerChoiceCode);

            foreach (AMLCFTAnswerDTO answerDTO in parentQuestion.Answers)
            {
                answerDTO.ChildQuestions = childQuestions.Where(x => x.ParentAnswerCode == answerDTO.AnswerChoiceCode && x.ParentAnswerCode != null).OrderBy(x=>x.SequenceNo).ToList();
                answerDTO.ChildQuestions.OrderBy(x => x.SequenceNo);
                foreach(AMLCFTQuestionDTO childQuestion in answerDTO.ChildQuestions )
                {
                    BuildTree(childQuestion, childQuestions, answerItems);
                }
            }

            foreach (AMLCFTQuestionDTO childQuestion in parentQuestion.ChildQuestions )
            {
                BuildTree(childQuestion, childQuestions, answerItems);
            }
        }

    }
}
