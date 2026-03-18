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
using Tranglo1.Onboarding.Application.DTO.TransactionEvaluation;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation;
using Serilog;

namespace Tranglo1.Onboarding.Application.Command
{
    class SaveTransactionEvaluationCommand : BaseCommand<Result<SaveTransactionEvaluationOutputDTO>>
    {
        public string LoginId { get; set; }
        public int BusinessProfileCode { get; set; }
        public TransactionEvaluationInputDTO TransactionEvaluationInputDTO { get; set; }

        // Concurrency Token
        public Guid? TransactionEvalConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<SaveTransactionEvaluationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Transaction Evaluation for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveTransactionEvaluationCommandHandler : IRequestHandler<SaveTransactionEvaluationCommand, Result<SaveTransactionEvaluationOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly IPartnerRepository _partnerRepository;
            private readonly BusinessProfileService _businessProfileService;

            public SaveTransactionEvaluationCommandHandler(           
                        IBusinessProfileRepository repository,
                        IConfiguration config,
                        TrangloUserManager userManager,
                        IPartnerRepository partnerRepository,
                        BusinessProfileService businessProfileService
                    )
            {
                _repository = repository;
                _config = config;
                _userManager = userManager;
                _partnerRepository = partnerRepository;
                _businessProfileService = businessProfileService;
            }

            public async Task<Result<SaveTransactionEvaluationOutputDTO>> Handle(SaveTransactionEvaluationCommand request, CancellationToken cancellationToken)
            {
                var outputDTO = new SaveTransactionEvaluationOutputDTO();

                //Handle concurrency
                var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
                if (businessProfile is null)
                {
                    return Result.Failure<SaveTransactionEvaluationOutputDTO>($"BusinessProfileCode: {request.BusinessProfileCode} does not exist");
                }
                else
                {
                    var concurrencyCheck = ConcurrencyCheck(request.TransactionEvalConcurrencyToken, businessProfile);
                    if (concurrencyCheck.IsFailure)
                    {
                        return Result.Failure<SaveTransactionEvaluationOutputDTO>(concurrencyCheck.Error);
                    }

                    businessProfile = await _repository.UpdateBusinessProfileAsync(businessProfile);
                    outputDTO.TransactionEvalConcurrencyToken = businessProfile.TransactionEvalConcurrencyToken;
                }

                //Get Transaction Evaluation per Business Profile

                List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswersToAdd = new List<CustomerBusinessTransactionEvaluationAnswer>();
                List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswersToUpdate = new List<CustomerBusinessTransactionEvaluationAnswer>();
                List<CustomerBusinessTransactionEvaluationAnswer> customerBusinessTransactionEvaluationAnswersToDelete = new List<CustomerBusinessTransactionEvaluationAnswer>();

                if (request.TransactionEvaluationInputDTO.Questions != null )
                {
                    foreach (var transactionEvaluationQuestion in request.TransactionEvaluationInputDTO.Questions)
                    {
                        foreach(var transactionEvaluationAnswer in transactionEvaluationQuestion.Answers)
                        {
                            //Get AnswerChoice
                            var answerChoice = transactionEvaluationAnswer.AnswerChoiceCode.HasValue ?
                                                    await _repository.GetTransactionEvaluationAnswerChoiceAsync(transactionEvaluationAnswer.AnswerChoiceCode.Value) :
                                                    null;
                            
                            //Get Question Code
                            var question = await _repository.GetTransactionEvaluationQuestionAsync(transactionEvaluationQuestion.QuestionCode);

                            if (transactionEvaluationAnswer.AnswerCode.HasValue)
                            {
                                var customerBusinessTransactionEvaluationAnswer = await _repository.GetCustomerBusinessTransactionEvaluationAnswersByIdAsync(transactionEvaluationAnswer.AnswerCode.Value);

                                if (question.TransactionEvaluationQuestionInputType != TransactionEvaluationQuestionInputType.RadioButton
                                    && transactionEvaluationAnswer.IsRemoved 
                                    && customerBusinessTransactionEvaluationAnswer != null)
                                {
                                    //To Delete
                                    customerBusinessTransactionEvaluationAnswersToDelete.Add(customerBusinessTransactionEvaluationAnswer);
                                }
                                else if(question.TransactionEvaluationQuestionInputType == TransactionEvaluationQuestionInputType.RadioButton
                                        && customerBusinessTransactionEvaluationAnswer != null 
                                        && ( transactionEvaluationAnswer.IsRemoved || !transactionEvaluationAnswer.IsAnswered) )
                                {
                                    //To Delete
                                    customerBusinessTransactionEvaluationAnswersToDelete.Add(customerBusinessTransactionEvaluationAnswer);
                                }
                            }
                            else if(transactionEvaluationAnswer.IsAnswered)
                            {
                                CustomerBusinessTransactionEvaluationAnswer customerBusinessTransactionEvaluationAnswer 
                                    = new CustomerBusinessTransactionEvaluationAnswer(
                                                request.BusinessProfileCode,
                                                answerChoice,
                                                question,
                                                transactionEvaluationAnswer.AnswerMetaCode,
                                                transactionEvaluationAnswer.AnswerRemark
                                        );

                                customerBusinessTransactionEvaluationAnswersToAdd.Add(customerBusinessTransactionEvaluationAnswer);
                            }
                        }
                    }
                }

                //Transaction Evaluation add/update/delete
                if(customerBusinessTransactionEvaluationAnswersToAdd.Count > 0)
                {
                    //ADD
                    var customerBusinessTransactionEvaluationAnswer = await _repository.AddCustomerBusinessTransactionEvaluationAnswer(customerBusinessTransactionEvaluationAnswersToAdd);
                }
                if (customerBusinessTransactionEvaluationAnswersToDelete.Count > 0)
                {
                    //DELETE
                    _repository.DeleteCustomerBusinessTransactionEvaluationAnswer(customerBusinessTransactionEvaluationAnswersToDelete);
                }


                //Update Collection tier if question is answered
                var transactionEvaluationAnswers = await _repository.GetCustomerBusinessTransactionEvaluationAnswersAsync(request.BusinessProfileCode);
                var isQuestionAnswered = transactionEvaluationAnswers.FindAll(x => x.TransactionEvaluationAnswerChoice != null);
                if (businessProfile.CollectionTier != CollectionTier.Tier_3 && businessProfile.CollectionTier != CollectionTier.Tier_2) 
                {
                    if (isQuestionAnswered.Count > 0)
                    {
                        var updateCollectionTierOnTransactionEvaluation = await _businessProfileService.EnsureCollectionTierOnTransactionEvaluation(businessProfile, true);
                    }
                }
                
                return Result.Success(outputDTO);
            }

            
            private Result ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
            {
                try
                {
                    if ((concurrencyToken.HasValue && businessProfile.TransactionEvalConcurrencyToken != concurrencyToken) ||
                        concurrencyToken is null && businessProfile.TransactionEvalConcurrencyToken != null)
                    {
                        // Return a 409 Conflict status code when there's a concurrency issue
                        return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                    }

                    // Stamp new token
                    businessProfile.TransactionEvalConcurrencyToken = Guid.NewGuid();
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while processing the request.");

                    // Return a 409 Conflict status code
                    return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }
            }
        }        
    }    
}