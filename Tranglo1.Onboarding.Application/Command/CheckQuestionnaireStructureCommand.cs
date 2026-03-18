using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using CSharpFunctionalExtensions;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Domain.Repositories;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Microsoft.Extensions.Configuration;
using Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Application.Common.Constant;
using System;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCAMLCFT, UACAction.Edit)]
    class CheckQuestionnaireStructureCommand : BaseCommand<Result>
    {
        public IEnumerable<AMLCFTQuestionnaireInputDTO> QuestionnaireDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class CheckQuestionnaireStructureCommandHandler : IRequestHandler<CheckQuestionnaireStructureCommand, Result>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _repository;
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;

            public CheckQuestionnaireStructureCommandHandler(
                    BusinessProfileService businessProfileService,
                    IBusinessProfileRepository repository,
                    IConfiguration config,
                    TrangloUserManager userManager,
                    PartnerService partnerService
                    )
            {
                _businessProfileService = businessProfileService;
                _repository = repository;
                _config = config;
                _userManager = userManager;
                _partnerService = partnerService;
            }

            public async Task<Result> Handle(CheckQuestionnaireStructureCommand request, CancellationToken cancellationToken)
            {
                var checkQuestionnaires = await CheckQuestionnaires(request.QuestionnaireDTO);

                if (checkQuestionnaires.IsFailure)
                {
                    return Result.Failure("Error on questionnaire(s) structure. Please reload the page.");
                }

                return Result.Success();
            }

            // Compare questionnaire(s) in request model vs database. 
            private async Task<Result> CheckQuestionnaires(IEnumerable<AMLCFTQuestionnaireInputDTO> QuestionnaireDTO)
            {
                foreach (var qn in QuestionnaireDTO)
                {
                    var questionnaire = await _repository.GetQuestionnaireByCodeAsync(qn.QuestionnaireCode);
                    if (questionnaire is null)
                    {
                        return Result.Failure($"QuestionnaireCode: {qn.QuestionnaireCode} does not exist or has been disabled. Please reload the page.");
                    }

                    foreach (var qs in qn.QuestionSections)
                    {
                        var questionSection = await _repository.GetQuestionSectionByQuestionSectionCodeAsync(qs.QuestionSectionCode);
                        if (questionSection is null)
                        {
                            return Result.Failure($"QuestionSectionCode: {qs.QuestionSectionCode} does not exist or has been deleted. Please reload the page.");
                        }

                        foreach (var q in qs.Questions)
                        {
                            var question = await _repository.GetQuestionByQuestionCodeAsync(q.QuestionCode);
                            if (question is null)
                            {
                                return Result.Failure($"QuestionCode: {q.QuestionCode} does not exist or has been deleted. Please reload the page.");
                            }

                            foreach (var a in q.Answers)
                            {
                                long answerChoiceCode = Convert.ToInt64(a.AnswerChoiceCode);
                                var answer = await _repository.GetAnswerChoiceByAnswerChoiceCodeAsync(answerChoiceCode);
                                if (answer is null)
                                {
                                    return Result.Failure($"QuestionCode: {a.AnswerChoiceCode} does not exist or has been deleted. Please reload the page.");
                                }
                            }
                        }
                    }
                }

                return Result.Success();
            }
        }
    }
}

