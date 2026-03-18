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
using Tranglo1.Onboarding.Domain.Common;
using Serilog;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCAMLCFT, UACAction.Edit)]
    [Permission(Permission.KYCManagementAMLCFT.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementAMLCFT.Action_View_Code })]
    class SaveAMLCFTQuestionnaireAnswersCommand : BaseCommand<Result<AMLCFTQuestionnaireAnswersOutputDTO>>
    {
        public string LoginId { get; set; }
        public int BusinessProfileCode { get; set; }
        public IEnumerable<AMLCFTQuestionnaireInputDTO> QuestionnaireDTO { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public Guid? AMLCFTQuestionnaireConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<AMLCFTQuestionnaireAnswersOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add AMLCFT Questionnaire for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveAMLCFTQuestionnaireAnswersCommandHandler : IRequestHandler<SaveAMLCFTQuestionnaireAnswersCommand, Result<AMLCFTQuestionnaireAnswersOutputDTO>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _repository;
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;

            public SaveAMLCFTQuestionnaireAnswersCommandHandler(
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

            public async Task<Result<AMLCFTQuestionnaireAnswersOutputDTO>> Handle(SaveAMLCFTQuestionnaireAnswersCommand request, CancellationToken cancellationToken)
            {
                //var checkQuestionnaires = await CheckQuestionnaires(request.QuestionnaireDTO);

                //if (checkQuestionnaires.IsFailure)
                //{
                //    return Result.Failure(checkQuestionnaires.Error);
                //}
                Result<AMLCFTQuestionnaireAnswersOutputDTO>  result = new Result<AMLCFTQuestionnaireAnswersOutputDTO>();
                var businessProfileResult = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);

                if (businessProfileResult.IsFailure)
                {
                    return Result.Failure<AMLCFTQuestionnaireAnswersOutputDTO>("Invalid business profile.");
                }

                BusinessProfile businessProfile = businessProfileResult.Value;

                List<AMLCFTQuestionnaire> aMLCFTQuestionnaires = new List<AMLCFTQuestionnaire>();
                List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers = new List<AMLCFTQuestionnaireAnswer>();
                var kycReviewResult = await _repository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_AMLOrCFT.Id);
                var kycBusinessReviewResult = await _repository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_AMLOrCFT.Id);

                if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
                {
                    var concurrencyCheck = ConcurrencyCheck(request.AMLCFTQuestionnaireConcurrencyToken, businessProfile);
                    if (concurrencyCheck.Result.IsFailure)
                    {
                        return Result.Failure<AMLCFTQuestionnaireAnswersOutputDTO>(concurrencyCheck.Result.Error);
                    }
                }


                if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete
                    || kycBusinessReviewResult != null))
                {
                    //add, update, delete
                    result = await AddUpdateDeleteAMLCFT(request, businessProfile, cancellationToken, aMLCFTQuestionnaires, aMLCFTQuestionnaireAnswers);
                                        
                    if (result.IsFailure)
                    {
                        return Result.Failure<AMLCFTQuestionnaireAnswersOutputDTO>(
                                            $"Customer user is unable to update for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }                
                }

                else if (applicationUser is TrangloStaff && 
                    ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) || 
                    businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                    || kycBusinessReviewResult != null)))
                {
                    //add, update, delete
                     result = await AddUpdateDeleteAMLCFT(request, businessProfile, cancellationToken, aMLCFTQuestionnaires, aMLCFTQuestionnaireAnswers);

                    if (result.IsFailure)
                    {
                        return Result.Failure<AMLCFTQuestionnaireAnswersOutputDTO>(
                                            $"Admin user is unable to update for BusinessProfileCode: {request.BusinessProfileCode}.  Check Failure"
                                            );
                    }

                    if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
                    {
                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Business_AMLOrCFT);
                    }
                    else
                    {
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_AMLOrCFT);
                    }

                    /*
                    var _isMandatoryCompleted = await _businessProfileService.isMandatoryFieldCompletedAsync(request.BusinessProfileCode);
                    if (!_isMandatoryCompleted.isAMLCompleted)
                    {
                        //set ReviewResult to 'insufficient/incomplete'
                        await _businessProfileService.UpdateKYCSubModuleReviewResult(businessProfile
                                                                                    , KYCCategory.AMLOrCFT
                                                                                    , ReviewResult.Insufficient_Incomplete);
                    }
                    */
                }

                else
                {
                    
                    return Result.Failure<AMLCFTQuestionnaireAnswersOutputDTO>(
                                          $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                         );                    
                }



                return Result.Success<AMLCFTQuestionnaireAnswersOutputDTO>(result.Value);
            }

            private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
            {
                try
                {
                    if ((concurrencyToken.HasValue && businessProfile.AMLCFTQuestionnaireConcurrencyToken != concurrencyToken) ||
                        concurrencyToken is null && businessProfile.AMLCFTQuestionnaireConcurrencyToken != null)
                    {
                        // Return a 409 Conflict status code when there's a concurrency issue
                        return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                    }

                    // Stamp new token
                    businessProfile.AMLCFTQuestionnaireConcurrencyToken = Guid.NewGuid();
                    await _repository.UpdateBusinessProfileAsync(businessProfile);

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while processing the request.");

                    // Return a 409 Conflict status code
                    return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }
            }

            private async Task<Result<AMLCFTQuestionnaireAnswersOutputDTO>> AddUpdateDeleteAMLCFT(SaveAMLCFTQuestionnaireAnswersCommand request,
                                                    BusinessProfile businessProfile,
                                                    CancellationToken cancellationToken,
                                                    List<AMLCFTQuestionnaire> aMLCFTQuestionnaires,
                                                    List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers)
            {
                foreach (AMLCFTQuestionnaireInputDTO questionnaireDTO in request.QuestionnaireDTO)
                {
                    foreach (AMLCFTQuestionSectionDTO questionSectionDTO in questionnaireDTO.QuestionSections)
                    {
                        foreach (AMLCFTQuestionDTO parentQuestion in questionSectionDTO.Questions)
                        {
                            await QuestionnaireHierarchy(parentQuestion,
                                                         businessProfile,
                                                         aMLCFTQuestionnaires,
                                                         aMLCFTQuestionnaireAnswers,
                                                         cancellationToken);
                        }
                    }
                }

                //Perform deletion on items which have been answered -> unanswered
                IEnumerable<AMLCFTQuestionnaire> existingAMLCFTQuestionnaires;
                IEnumerable<AMLCFTQuestionnaireAnswer> existingAMLCFTQuestionnaireAnswers;

                existingAMLCFTQuestionnaires = await _repository.GetAMLCFTQuestionnairesByBusinessProfileAsync(businessProfile);
                existingAMLCFTQuestionnaireAnswers = await _repository.GetAMLCFTQuestionnaireAnswersByBusinessProfileAsync(businessProfile);

                var deletedAMLCFTQuestionnaires = from existing in existingAMLCFTQuestionnaires
                                                  let fromInput = aMLCFTQuestionnaires
                                                  .FirstOrDefault(input =>
                                                      input.Id == existing.Id
                                                      )
                                                  where fromInput == null
                                                  select existing;

                /*from existing in existingAMLCFTQuestionnaires
                                                join fromInput in aMLCFTQuestionnaires on existing.Id equals fromInput.Id
                                                into joinedAMLCFTQuestionnaires
                                                from ju in joinedAMLCFTQuestionnaires.DefaultIfEmpty()
                                                where ju == null
                                                select ju;
                */

                var deletedAMLCFTQuestionnaireAnswers = from existing in existingAMLCFTQuestionnaireAnswers
                                                        let fromInput = aMLCFTQuestionnaireAnswers
                                                        .FirstOrDefault(input =>
                                                            input.Id == existing.Id
                                                            )
                                                        where fromInput == null
                                                        select existing;

                /*from existing in existingAMLCFTQuestionnaireAnswers
                                                        join fromInput in aMLCFTQuestionnaireAnswers on existing.Id equals fromInput.Id
                                                        into joinedAMLCFTQuestionnaires
                                                        from ju in joinedAMLCFTQuestionnaires.DefaultIfEmpty()
                                                        where ju == null
                                                        select ju;
                */

                if (deletedAMLCFTQuestionnaireAnswers.Any())
                    await _businessProfileService.DeleteAMLCFTQuestionnaireAnswersAsync(deletedAMLCFTQuestionnaireAnswers, businessProfile, cancellationToken);
                    //await _repository.DeleteAMLCFTQuestionnaireAnswersAsync(deletedAMLCFTQuestionnaireAnswers, cancellationToken);

                if (deletedAMLCFTQuestionnaires.Any())
                    await _repository.DeleteAMLCFTQuestionnairesAsync(deletedAMLCFTQuestionnaires, cancellationToken);

                AMLCFTQuestionnaireAnswersOutputDTO output = new AMLCFTQuestionnaireAnswersOutputDTO()
                {
                    AMLCFTQuestionnaireConcurrencyToken = businessProfile.AMLCFTQuestionnaireConcurrencyToken
                };
                return Result.Success<AMLCFTQuestionnaireAnswersOutputDTO>(output);
            }
            private async Task QuestionnaireHierarchy(AMLCFTQuestionDTO questionDTO, 
                                                        BusinessProfile businessProfile,
                                                        List<AMLCFTQuestionnaire> aMLCFTQuestionnaires,
                                                        List<AMLCFTQuestionnaireAnswer> aMLCFTQuestionnaireAnswers,
                                                        CancellationToken cancellationToken)
            {
                var question = await _repository.GetAMLCFTQuestionByQuestionCodeAsync(questionDTO.QuestionCode);

                var amlCFTQuestion = _repository.GetAMLCFTQuestionAsync(businessProfile, question);

                AMLCFTQuestionnaire amlCFTQuestionnaire = amlCFTQuestion.Result;

                if (amlCFTQuestionnaire == null)
                {
                    //Add
                    AMLCFTQuestionnaire newAMLCFTQuestionnaire = new AMLCFTQuestionnaire(question, businessProfile);
                    amlCFTQuestionnaire = await _repository.AddAMLCFTQuestionnaireQuestionsAsync(newAMLCFTQuestionnaire,cancellationToken);
                }

                aMLCFTQuestionnaires.Add(amlCFTQuestionnaire);

                foreach(var answerDTO in questionDTO.Answers)
                {
                    if( answerDTO.IsAnswered)
                    {
                        //Convert int? to int
                        //v2 = v1 ?? default(int);
                        var answerChoiceCode = answerDTO.AnswerChoiceCode ?? default(int);

                        var answerChoice = await _repository.GetAMLCFTAnswerChoiceAsync(answerChoiceCode);
                        var amlCFTAnswer = _repository.GetAMLCFTAnswerAsync(amlCFTQuestionnaire, answerChoice, answerDTO.AnswerRemark);

                        AMLCFTQuestionnaireAnswer amlCFTQuestionnaireAnswer = new AMLCFTQuestionnaireAnswer(amlCFTQuestionnaire, answerChoice, answerDTO.AnswerRemark);
                        AMLCFTQuestionnaireAnswer updatedAMLCFTQuestionnaireAnswer;
                        if (amlCFTAnswer.Result == null)
                        {
                            //add
                            updatedAMLCFTQuestionnaireAnswer = await _businessProfileService.AddAMLCFTQuestionnaireAnswersAsync(amlCFTQuestionnaireAnswer, businessProfile, cancellationToken);
                            //updatedAMLCFTQuestionnaireAnswer = await _repository.AddAMLCFTQuestionnaireAnswersAsync(amlCFTQuestionnaireAnswer, cancellationToken);
                        }
                        else
                        {
                            //update
                            var amlCFTAnswerUpdate = amlCFTAnswer.Result;
                            amlCFTAnswerUpdate.AnswerChoice = answerChoice;
                            amlCFTAnswerUpdate.AnswerRemark = answerDTO.AnswerRemark;

                            updatedAMLCFTQuestionnaireAnswer = await _businessProfileService.UpdateAMLCFTQuestionnaireAnswersAsync(amlCFTAnswerUpdate, businessProfile, cancellationToken);
                            //updatedAMLCFTQuestionnaireAnswer = await _repository.UpdateAMLCFTQuestionnaireAnswersAsync(amlCFTAnswerUpdate, cancellationToken);
                        }

                        aMLCFTQuestionnaireAnswers.Add(updatedAMLCFTQuestionnaireAnswer);

                        foreach (AMLCFTQuestionDTO childAnswerQuestion in answerDTO.ChildQuestions)
                        {
                            await QuestionnaireHierarchy(childAnswerQuestion, businessProfile, aMLCFTQuestionnaires, aMLCFTQuestionnaireAnswers, cancellationToken);
                        }
                    }
                }

                foreach (AMLCFTQuestionDTO childQuestion in questionDTO.ChildQuestions)
                {
                    await QuestionnaireHierarchy(childQuestion, businessProfile, aMLCFTQuestionnaires, aMLCFTQuestionnaireAnswers, cancellationToken);
                }
            }


            // Compare questionnaire(s) in request model vs database. 
            //private async Task<Result> CheckQuestionnaires(IEnumerable<AMLCFTQuestionnaireInputDTO> QuestionnaireDTO)
            //{
            //    foreach (var qn in QuestionnaireDTO)
            //    {
            //        var questionnaire = await _repository.GetQuestionnaireByCodeAsync(qn.QuestionnaireCode);
            //        if (questionnaire is null)
            //        {
            //            return Result.Failure($"QuestionnaireCode: {qn.QuestionnaireCode} does not exist or has been disabled. Please reload the page.");
            //        }

            //        foreach (var qs in qn.QuestionSections)
            //        {
            //            var questionSection = await _repository.GetQuestionSectionByQuestionSectionCodeAsync(qs.QuestionSectionCode);
            //            if (questionSection is null)
            //            {
            //                return Result.Failure($"QuestionSectionCode: {qs.QuestionSectionCode} does not exist or has been deleted. Please reload the page.");
            //            }

            //            foreach (var q in qs.Questions)
            //            {
            //                var question = await _repository.GetQuestionByQuestionCodeAsync(q.QuestionCode);
            //                if (question is null)
            //                {
            //                    return Result.Failure($"QuestionCode: {q.QuestionCode} does not exist or has been deleted. Please reload the page.");
            //                }

            //                foreach (var a in q.Answers)
            //                {
            //                    long answerChoiceCode = Convert.ToInt64(a.AnswerChoiceCode);
            //                    var answer = await _repository.GetAnswerChoiceByAnswerChoiceCodeAsync(answerChoiceCode);
            //                    if (answer is null)
            //                    {
            //                        return Result.Failure($"QuestionCode: {a.AnswerChoiceCode} does not exist or has been deleted. Please reload the page.");
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    return Result.Success();
            //}

        }

     
    }    
}
