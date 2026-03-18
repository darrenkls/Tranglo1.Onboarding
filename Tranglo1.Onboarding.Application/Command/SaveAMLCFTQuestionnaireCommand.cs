using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOnlineAMLCFTQuestionnaires, UACAction.Edit)]
    [Permission(Permission.KYCAdministrationOnlineAMLCFTQuestionnaires.Action_Add_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCAdministrationOnlineAMLCFTQuestionnaires.Action_View_Code })]
    class SaveAMLCFTQuestionnaireCommand : BaseCommand<Result>
    {
        public AdminAMLCFTQuestionnaireOutputDTO QuestionnaireDTO { get; set; }

        private enum Action 
        {
            Create,
            Update
        }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                var action = 
                    QuestionnaireDTO.QuestionnaireCode.HasValue ?
                    Action.Update : Action.Create;

                if (action.Equals(Action.Create))
                {
                    string _description = $"Added a new AMLCFT Questionnaire";
                    return Task.FromResult(_description);
                }
                else
                {
                    string _description = $"Edited AMLCFT Questionnaire";
                    return Task.FromResult(_description);
                }
            }

            return Task.FromResult<string>(null);
        }

        public class SaveAMLCFTQuestionnaireCommandHandler : IRequestHandler<SaveAMLCFTQuestionnaireCommand, Result>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _repository;
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;
            private readonly ILogger<SaveAMLCFTQuestionnaireCommand> _logger;

            public SaveAMLCFTQuestionnaireCommandHandler(
                    BusinessProfileService businessProfileService,
                    IBusinessProfileRepository repository,
                    IConfiguration config,
                    TrangloUserManager userManager,
                    PartnerService partnerService,
                    ILogger<SaveAMLCFTQuestionnaireCommand> logger
                    )
            {
                _businessProfileService = businessProfileService;
                _repository = repository;
                _config = config;
                _userManager = userManager;
                _partnerService = partnerService;
                _logger = logger; 
            }

            public async Task<Result> Handle(SaveAMLCFTQuestionnaireCommand request, CancellationToken cancellationToken)
            {
                int add = 1;
                int update = 2;

                List<QuestionnaireSolution> addQuestionnaireSolution = new List<QuestionnaireSolution>();
                List<QuestionnaireSolution> updateQuestionnaireSolution = new List<QuestionnaireSolution>();
                List<QuestionnaireSolution> deleteQuestionnaireSolution = new List<QuestionnaireSolution>();

                List<QuestionSection> addedQuestionSections = new List<QuestionSection>();
                List<QuestionSection> updatedQuestionSections = new List<QuestionSection>();

                List<Question> addedQuestions = new List<Question>();
                List<Question> updatedQuestions = new List<Question>();

                List<AnswerChoice> addedAnswerChoices = new List<AnswerChoice>();
                List<AnswerChoice> updatedAnswerChoices = new List<AnswerChoice>();

                // editing existing questionnaire
                if (request.QuestionnaireDTO.QuestionnaireCode != null)
                {
                    bool isNewQuestionnaire = false;

                    var questionnaireCode = request.QuestionnaireDTO.QuestionnaireCode ?? default(long);
                    var existingQuestionnaire = await _repository.GetQuestionnaireByQuestionnaireCodeAsync(questionnaireCode);
                    var existingQuestionnaireSolution = await _repository.GetQuestionnaireSolutionsByQuestionnaireCodeAsync(questionnaireCode);

                    List<QuestionSection> existingQuestionSections = await _repository.GetQuestionSectionsByQuestionnaireCodeAsync(existingQuestionnaire.Id);
                    List<Question> existingQuestions = await _repository.GetQuestionsByQuestionSectionAsync(existingQuestionSections);
                    List<AnswerChoice> existingAnswerChoices = await _repository.GetAnswerChoicesByQuestionAsync(existingQuestions);

                    bool isActive = request.QuestionnaireDTO.QuestionnaireCode is null ? true : existingQuestionnaire.IsActive;

                    // Update Questionnaire description
                    if (existingQuestionnaire.Description != request.QuestionnaireDTO.QuestionnaireDescription)
                    {
                        existingQuestionnaire.Description = request.QuestionnaireDTO.QuestionnaireDescription;
                        var updateQuestionnaireResult = _repository.AddOrUpdateQuestionnaireAsync(existingQuestionnaire, update);

                        if (updateQuestionnaireResult.Result.IsFailure)
                        {
                            //return Result.Failure($"Unable to update Questionnaire description for : {request.QuestionnaireDTO.QuestionnaireDescription}.");
                            return Result.Failure($"This questionnaire name already existed");
                        }
                    }

                    //Update Questionnaire Solution
                    foreach (var i in request.QuestionnaireDTO.QuestionnaireSolutions)
                    {

                        if (!i.isDeleted)
                        {
                            var solution = await _repository.GetSolutionByCodeAsync(i.SolutionCode.Value);
                            var existingQuestionnaireSolutions = await _repository.GetQuestionnaireSolutionByQuestionnaireAndSolutionAsync(existingQuestionnaire, solution);

                            if (existingQuestionnaireSolutions == null)
                            {
                                QuestionnaireSolution questionnaireSolution = new QuestionnaireSolution(existingQuestionnaire, solution);
                                updateQuestionnaireSolution.Add(questionnaireSolution);
                            }

                        }
                        else if (i.isDeleted)
                        {
                            var deleteableExistingQuestionnaireSolution = await _repository.GetQuestionnaireSolutionsByQuestionnaireCodeAsync(existingQuestionnaire.Id);
                            deleteQuestionnaireSolution.Add(deleteableExistingQuestionnaireSolution);
                        }


                    }

                    foreach (AdminAMLCFTQuestionSectionDTO qs in request.QuestionnaireDTO.QuestionSections)
                    {
                        var sequenceNo = 1; //update dto to return sequence no. Need to update GetQuestionnaire to return sequenceNo also

                        var questionSectionCode = qs.QuestionSectionCode ?? default(long);

                        QuestionSection questionSection = new QuestionSection();

                        if (qs.QuestionSectionCode == null)
                        {
                            // add
                            QuestionSection section = new QuestionSection(existingQuestionnaire, qs.QuestionSectionDescription, sequenceNo);
                            addedQuestionSections.Add(section);
                            questionSection = section;
                        }
                        else if (qs.QuestionSectionCode != null)
                        {
                            // update
                            var section = await _repository.GetQuestionSectionByQuestionSectionCodeAsync(questionSectionCode);

                            section.Description = qs.QuestionSectionDescription;
                            updatedQuestionSections.Add(section);
                            questionSection = section;
                        }

                        foreach (AdminAMLCFTQuestionDTO parentQuestion in qs.Questions)
                        {

                            var questionSequenceNo = parentQuestion.SequenceNo ?? default(int);

                            var questionCode = parentQuestion.QuestionCode ?? default(long);    // convert bool? to bool
                            var isOptional = parentQuestion.IsOptional ?? default(bool);    // convert bool? to bool                           

                            QuestionInputType questionInputType = QuestionInputType.FindById<QuestionInputType>(parentQuestion.QuestionInputTypeCode);

                            if (parentQuestion.QuestionCode == null)
                            {
                                // add
                                Question question = new Question(questionInputType, parentQuestion.QuestionDescription,
                                questionSection, null, null, isOptional, questionSequenceNo);

                                //AnswerChoice answerChoice = new AnswerChoice();
                                addedQuestions.Add(question);

                                await QuestionHierarchy(questionSection, parentQuestion, question, null, isNewQuestionnaire,
                                    addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);

                            }
                            else if (parentQuestion.QuestionCode != null)
                            {
                                var checkParentQuestionExists = await _repository.GetQuestionByQuestionCodeAsync(questionCode);

                                // update
                                if (checkParentQuestionExists != null)
                                {

                                    var updatedParentQuestion = checkParentQuestionExists;
                                    updatedParentQuestion.Description = parentQuestion.QuestionDescription;
                                    updatedParentQuestion.QuestionInputType = questionInputType;
                                    updatedParentQuestion.QuestionInputTypeCode = questionInputType.Id;
                                    updatedParentQuestion.SequenceNo = questionSequenceNo;
                                    updatedParentQuestion.IsActive = true;
                                    updatedParentQuestion.IsOptional = isOptional;

                                    updatedQuestions.Add(updatedParentQuestion);
                                    //AnswerChoice answerChoice = new AnswerChoice();

                                    await QuestionHierarchy(questionSection, parentQuestion, updatedParentQuestion, null, isNewQuestionnaire,
                                        addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                                }
                                else
                                {
                                    return Result.Failure($"Failed to updated question. QuestionCode: {parentQuestion.QuestionCode} does not exist.");
                                }
                            }
                        }
                    }

                    // delete
                    var inputAnswerChoices = addedAnswerChoices.Concat(updatedAnswerChoices).ToList();
                    var inputQuestions = addedQuestions.Concat(updatedQuestions).ToList();
                    var inputQuestionSections = addedQuestionSections.Concat(updatedQuestionSections).ToList();

                    var checkExistingQuestionSections = await _repository.GetQuestionSectionsByQuestionnaireCodeAsync(questionnaireCode);
                    var checkExistingQuestions = await _repository.GetQuestionsByQuestionSectionAsync(checkExistingQuestionSections);
                    var checkExistingAnswerChoices = await _repository.GetAnswerChoicesByQuestionAsync(checkExistingQuestions);

                    var deletedAnswerChoices = (from existing in checkExistingAnswerChoices
                                                let fromInput = inputAnswerChoices
                                                .FirstOrDefault(input =>
                                                    input.Id == existing.Id
                                                    )
                                                where fromInput == null
                                                select existing).ToList();

                    var deletedQuestions = (from existing in checkExistingQuestions
                                            let fromInput = inputQuestions
                                            .FirstOrDefault(input =>
                                                input.Id == existing.Id
                                                )
                                            where fromInput == null
                                            select existing).ToList();

                    var deletedQuestionSections = (from existing in checkExistingQuestionSections
                                                   let fromInput = inputQuestionSections
                                                   .FirstOrDefault(input =>
                                                       input.Id == existing.Id
                                                       )
                                                   where fromInput == null
                                                   select existing).ToList();

                    try
                    {
                        var updateQuestionsResult = await _repository.AddOrUpdateQuestionsAsync(updatedQuestions, update);
                        var addQuestionsResult = await _repository.AddOrUpdateQuestionsAsync(addedQuestions, add);

                        var deleteAnswerChoicesResult = await _repository.DeleteAnswerChoicesAsync(deletedAnswerChoices);
                        var deleteQuestionsResult = await _repository.DeleteQuestionsAsync(deletedQuestions);
                        var deleteQuestionSectionsResult = await _repository.DeleteQuestionSectionsAsync(deletedQuestionSections);

                        

                        var updateQuestionSectionsResult = await _repository.AddOrUpdateQuestionSectionAsync(updatedQuestionSections, update);
                        var addQuestionSectionsResult = await _repository.AddOrUpdateQuestionSectionAsync(addedQuestionSections, add);
                        
                        var addAnswerChoicesResult = await _repository.AddOrUpdateAnswerChoicesAsync(addedAnswerChoices, add);
                        var updateAnswerChoicesResult = await _repository.AddOrUpdateAnswerChoicesAsync(updatedAnswerChoices, update);
                        var updateQuestionnaireSolutionResult = await _repository.UpdateQuestionnaireSolutionAsync(updateQuestionnaireSolution);
                        
                        var deletedQuestionnaireSolutionResult = await _repository.DeleteQuestionnaireSolutionAsync(deleteQuestionnaireSolution);


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception: {ex.Message}");

                        return Result.Failure($"Unable to update Questionnaire {request.QuestionnaireDTO.QuestionnaireDescription}.");
                    }                               
                }

                // Adding a whole new questionnaire
                else if (request.QuestionnaireDTO.QuestionnaireCode is null)
                {                    
                    bool isNewQuestionnaire = true;
                    bool isActive = true;
                    Questionnaire inputQuestionnaire = new Questionnaire(request.QuestionnaireDTO.QuestionnaireDescription, isActive);

                    if (inputQuestionnaire.Description == "" || inputQuestionnaire.Description == null)
                    {
                        return Result.Failure($"Unable to add Questionnaire description for : {request.QuestionnaireDTO.QuestionnaireDescription} as Question name is mandatory.");
                    }
                   

                    foreach (AdminAMLCFTQuestionSectionDTO qs in request.QuestionnaireDTO.QuestionSections)
                    {
                        var questionSectionSequenceNo = 1; //update get to return value

                        // Add question sections
                        QuestionSection questionSection = new QuestionSection(inputQuestionnaire, qs.QuestionSectionDescription, questionSectionSequenceNo);
                        addedQuestionSections.Add(questionSection);

                        if (questionSection.Description == "" || questionSection.Description == null)
                        {
                            return Result.Failure($"Unable to add Questionnaire description for : {request.QuestionnaireDTO.QuestionnaireDescription} as Section name is mandatory.");
                        }

                        //add Solutions
                        foreach(var i in request.QuestionnaireDTO.QuestionnaireSolutions)
                        {
                            if (i.SolutionCode.HasValue)
                            {
                                var solution = await _repository.GetSolutionByCodeAsync(i.SolutionCode.Value);
                                QuestionnaireSolution questionnaireSolutionList = new QuestionnaireSolution(inputQuestionnaire, solution);
                                addQuestionnaireSolution.Add(questionnaireSolutionList);
                            }


                        }

                        foreach (AdminAMLCFTQuestionDTO parentQuestion in qs.Questions)
                        {
                            var questionSequenceNo = parentQuestion.SequenceNo ?? default(int);

                            QuestionInputType questionInputType = QuestionInputType.FindById<QuestionInputType>(parentQuestion.QuestionInputTypeCode);
                            var isOptional = parentQuestion.IsOptional ?? default(bool);    // convert bool? to bool

                            Question question = new Question(questionInputType, parentQuestion.QuestionDescription,
                                questionSection, null, null, isOptional, questionSequenceNo);

                            addedQuestions.Add(question);
                            //AnswerChoice answerChoice = new AnswerChoice();

                            if (question.Description == "" || question.Description == null)
                            {
                                return Result.Failure($"Unable to add Questionnaire description for : {request.QuestionnaireDTO.QuestionnaireDescription} as parent question are not filled in.");
                            }


                            await QuestionHierarchy(questionSection, parentQuestion, question, null, isNewQuestionnaire, 
                                addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                        }
                    }

                    // save list of added questionnaire, question sections, questions, answer choices
                    var addQuestionnaireResult = await _repository.AddOrUpdateQuestionnaireAsync(inputQuestionnaire, add);

                    if (addQuestionnaireResult.IsFailure)
                    {
                        _logger.LogError($"Questionnaire ID : {inputQuestionnaire.Id}, Error : {addQuestionnaireResult.Error}");
                        return Result.Failure(addQuestionnaireResult.Error);
                    }

                    var addQuestionSectionResult = await _repository.AddOrUpdateQuestionSectionAsync(addedQuestionSections, add);

                    if (addQuestionSectionResult.IsFailure)
                    {
                        _logger.LogError($"Question Section is unable to add or update. Error : {addQuestionSectionResult.Error}");
                        return Result.Failure(addQuestionSectionResult.Error);
                    }

                    var addQuestionsResult = await _repository.AddOrUpdateQuestionsAsync(addedQuestions, add);

                    if (addQuestionsResult.IsFailure)
                    {
                        _logger.LogError($"Question is unable to add or update. Error : {addQuestionsResult.Error}");
                        return Result.Failure(addQuestionsResult.Error);
                    }

                    var addAnswerChoicesResult = await _repository.AddOrUpdateAnswerChoicesAsync(addedAnswerChoices, add);

                    if (addAnswerChoicesResult.IsFailure)
                    {
                        _logger.LogError($"Answer choices is unable to add or update.  Error : {addAnswerChoicesResult.Error}");
                        return Result.Failure(addAnswerChoicesResult.Error);
                    }

                    var addQuestionnaireSolutionResult = await _repository.AddQuestionnaireSolutionAsync(addQuestionnaireSolution);
                    if (addQuestionnaireSolutionResult.IsFailure)
                    {
                        _logger.LogError($"Tranglo Solution is unable to be added or updated.  Error : {addQuestionnaireSolutionResult.Error}");
                        return Result.Failure(addQuestionnaireSolutionResult.Error);
                    }

                }

                return Result.Success();
            }            

            private async Task QuestionHierarchy(QuestionSection questionSection, AdminAMLCFTQuestionDTO parentQuestion, 
                Question question, AnswerChoice answerChoice, bool isNewQuestionnaire,
                List<Question> addedQuestions, List<Question> updatedQuestions, 
                List<AnswerChoice> addedAnswerChoices, List<AnswerChoice> updatedAnswerChoices)
            {
                long? newAnswerSequenceNumber = 0;
                foreach (AdminAMLCFTAnswerDTO answer in parentQuestion.Answers)
                {                   
                    var parentAnswerChoice = new AnswerChoice();
                    // add or update answer choice
                    var answerChoiceCode = answer.AnswerChoiceCode ?? default(long);    // convert bool? to bool
                    var checkAnswerChoiceExists = await _repository.GetAnswerChoiceByAnswerChoiceCodeAsync(answerChoiceCode);
                    

                    if (checkAnswerChoiceExists != null && answer.SequenceNumber != null)
                    {
                        if (answer.SequenceNumber != checkAnswerChoiceExists.SequenceNumber)
                        {
                            checkAnswerChoiceExists.SequenceNumber = answer.SequenceNumber;
                        }
                    }

                    if (!isNewQuestionnaire && answer.AnswerChoiceCode != null)
                    {
                        
                        if (checkAnswerChoiceExists == null)
                        {
                            // add
                            long? newSeqNo = checkAnswerChoiceExists.SequenceNumber + 1;
                            AnswerChoice answerChoiceWithParentQuestion = new AnswerChoice(answer.AnswerChoiceDescription, question, newSeqNo);
                            addedAnswerChoices.Add(answerChoiceWithParentQuestion);

                            parentAnswerChoice = answerChoiceWithParentQuestion;
                        }

                        else if (checkAnswerChoiceExists != null)
                        {
                            // update
                            checkAnswerChoiceExists.Description = answer.AnswerChoiceDescription;
                            var updatedAnswerChoice = checkAnswerChoiceExists;
                            updatedAnswerChoices.Add(updatedAnswerChoice);

                            parentAnswerChoice = updatedAnswerChoice;
                        }
                    }
                    else if (answer.AnswerChoiceCode == null)
                    {
                        
                        // add answer choice
                        if (checkAnswerChoiceExists != null)
                        {
                            newAnswerSequenceNumber = checkAnswerChoiceExists.SequenceNumber + 1;
                        }
                        else
                        {
                            newAnswerSequenceNumber++;
                        }
                        
                        AnswerChoice answerChoiceWithParentQuestion = new AnswerChoice(answer.AnswerChoiceDescription, question, newAnswerSequenceNumber);
                        addedAnswerChoices.Add(answerChoiceWithParentQuestion);

                        parentAnswerChoice = answerChoiceWithParentQuestion;
                    }

                    foreach (AdminAMLCFTQuestionDTO questionWithParentAnswer in answer.ChildQuestions)
                    {
                        var questionSequenceNo = questionWithParentAnswer.SequenceNo ?? default(int);                   

                        QuestionInputType questionInputType = QuestionInputType.FindById<QuestionInputType>(questionWithParentAnswer.QuestionInputTypeCode);
                        var isOptional = questionWithParentAnswer.IsOptional ?? default(bool);    // convert bool? to bool

                        if (!isNewQuestionnaire)
                        {
                            // add or update child question
                            var questionCode = questionWithParentAnswer.QuestionCode ?? default(long);
                            var checkChildQuestionExists = await _repository.GetQuestionByQuestionCodeAsync(questionCode);

                            if (checkChildQuestionExists == null)
                            {
                                // add
                                Question addedChildQuestion = new Question(questionInputType, questionWithParentAnswer.QuestionDescription,
                                questionSection, parentAnswerChoice, null, isOptional, questionSequenceNo);

                                //AnswerChoice parentAnswerChoice = new AnswerChoice();

                                addedQuestions.Add(addedChildQuestion);

                                await QuestionHierarchy(questionSection, questionWithParentAnswer, addedChildQuestion, parentAnswerChoice, isNewQuestionnaire,
                                    addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                            }
                            else if (checkChildQuestionExists != null)
                            {
                                // update
                                var updatedChildQuestion = checkChildQuestionExists;

                                updatedChildQuestion.Description = questionWithParentAnswer.QuestionDescription;
                                updatedChildQuestion.QuestionInputType = questionInputType;
                                updatedChildQuestion.QuestionInputTypeCode = questionInputType.Id;
                                updatedChildQuestion.SequenceNo = questionSequenceNo;
                                updatedChildQuestion.IsActive = true;
                                updatedChildQuestion.IsOptional = isOptional;
                                updatedQuestions.Add(updatedChildQuestion);

                                await QuestionHierarchy(questionSection, questionWithParentAnswer, updatedChildQuestion, parentAnswerChoice, isNewQuestionnaire,
                                    addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                            }
                        }
                        else if (isNewQuestionnaire)
                        {
                            // add child question
                            Question addedChildQuestion = new Question(questionInputType, questionWithParentAnswer.QuestionDescription,
                                questionSection, parentAnswerChoice, null, isOptional, questionSequenceNo);
                            addedQuestions.Add(addedChildQuestion);

                            await QuestionHierarchy(questionSection, questionWithParentAnswer, addedChildQuestion, parentAnswerChoice, isNewQuestionnaire, 
                                addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                        }
                    }
                }

                foreach (AdminAMLCFTQuestionDTO questionWithParentQuestion in parentQuestion.ChildQuestions)
                {
                    var questionSequenceNo = questionWithParentQuestion.SequenceNo ?? default(int);

                    QuestionInputType questionInputType = QuestionInputType.FindById<QuestionInputType>(questionWithParentQuestion.QuestionInputTypeCode);
                    var isOptional = questionWithParentQuestion.IsOptional ?? default(bool);    // convert bool? to bool

                    if (!isNewQuestionnaire)
                    {
                        // add or update child question
                        var questionCode = questionWithParentQuestion.QuestionCode ?? default(long);
                        var checkChildQuestionExists = await _repository.GetQuestionByQuestionCodeAsync(questionCode);

                        if (checkChildQuestionExists == null)
                        {
                            // add
                            Question addedChildQuestion = new Question(questionInputType, questionWithParentQuestion.QuestionDescription,
                            questionSection, null , question, isOptional, questionSequenceNo);
                            addedQuestions.Add(addedChildQuestion);

                            await QuestionHierarchy(questionSection, questionWithParentQuestion, addedChildQuestion, answerChoice, isNewQuestionnaire,
                                addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                        }
                        else if (checkChildQuestionExists != null)
                        {
                            //update
                            var updatedChildQuestion = checkChildQuestionExists;

                            updatedChildQuestion.Description = questionWithParentQuestion.QuestionDescription;
                            updatedChildQuestion.QuestionInputType = questionInputType;
                            updatedChildQuestion.QuestionInputTypeCode = questionInputType.Id;
                            updatedChildQuestion.SequenceNo = questionSequenceNo;
                            updatedChildQuestion.IsActive = true;
                            updatedChildQuestion.IsOptional = isOptional;
                            updatedQuestions.Add(updatedChildQuestion);

                            await QuestionHierarchy(questionSection, questionWithParentQuestion, updatedChildQuestion, answerChoice, isNewQuestionnaire,
                                addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                        }
                    }
                    else if (isNewQuestionnaire)
                    {
                        // add child question
                        Question addedChildQuestion = new Question(questionInputType, questionWithParentQuestion.QuestionDescription,
                            questionSection, null, question, isOptional, questionSequenceNo);
                        addedQuestions.Add(addedChildQuestion);

                        await QuestionHierarchy(questionSection, questionWithParentQuestion, addedChildQuestion, answerChoice, isNewQuestionnaire, 
                            addedQuestions, updatedQuestions, addedAnswerChoices, updatedAnswerChoices);
                    }                    
                }
            }
        }
    }
}
