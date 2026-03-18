using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.BusinessDeclaration;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCBusinessDeclaration, UACAction.Edit)]
    internal class SaveCustomerBusinessDeclarationAnswersCommand : BaseCommand<Result<SaveCustomerBusinessDeclarationAnswersInputDTO>>
    {
        public SaveCustomerBusinessDeclarationAnswersInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<SaveCustomerBusinessDeclarationAnswersInputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Save Customer Business Declaration Answers for BusinessProfileCode: [{this.InputDTO.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }
            return Task.FromResult<string>(null);
        }
    }

    internal class SaveCustomerBusinessDeclarationAnswersCommandHandler : IRequestHandler<SaveCustomerBusinessDeclarationAnswersCommand, Result<SaveCustomerBusinessDeclarationAnswersInputDTO>>
    {
        private readonly ILogger<SaveCustomerBusinessDeclarationAnswersCommand> _logger;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public SaveCustomerBusinessDeclarationAnswersCommandHandler(
            ILogger<SaveCustomerBusinessDeclarationAnswersCommand> logger, IBusinessProfileRepository businessProfileRepository, IPartnerRepository partnerRepository)
        {
            _logger = logger;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<SaveCustomerBusinessDeclarationAnswersInputDTO>> Handle(SaveCustomerBusinessDeclarationAnswersCommand request, CancellationToken cancellationToken)
        {
            var customerBusinessDeclaration = await _businessProfileRepository.GetCustomerBusinessDeclarationByCodeAsync(request.InputDTO.CustomerBusinessDeclarationCode);
            var customerBusinessDeclarationAnswers = await _businessProfileRepository.GetCustomerBusinessDeclarationAnswersByCodeAsync(request.InputDTO.CustomerBusinessDeclarationCode);
            var customerType = await _partnerRepository.GetCustomerTypeByBusinessProfileAsync(request.InputDTO.BusinessProfileCode);
            var declarationQuestions = await _businessProfileRepository.GetDeclarationQuestionsByCustomerTypeAsync(customerType.Id);

            if (customerBusinessDeclaration.RedoCount == 0)
            {
                return Result.Failure<SaveCustomerBusinessDeclarationAnswersInputDTO>($"Customer User is currently blocked from submiting. RedoCount: {customerBusinessDeclaration.RedoCount}");
            }

            // 1) Update answers
            foreach (var c in customerBusinessDeclarationAnswers)
            {
                var input = request.InputDTO.SaveCustomerBusinessDeclarationAnswers
                    .Where(x => x.CustomerBusinessDeclarationAnswerCode == c.Id)
                    .FirstOrDefault();

                var titleQuestion = declarationQuestions.FirstOrDefault(x => x.DeclarationQuestionType == DeclarationQuestionType.Title);
                if (titleQuestion != null && (c.DeclarationQuestionCode == titleQuestion.Id))
                {
                    //Set answer false if null for question type = title && not passed in by FE
                    c.DeclarationAnswer = false;
                }
                else if (input != null)
                {
                    c.DeclarationAnswer = input.DeclarationAnswer;
                    bool isToggleOrTitleQuestion = declarationQuestions
                        .Any(x => x.Id == c.DeclarationQuestionCode && (
                        x.DeclarationQuestionType == DeclarationQuestionType.Toggle || x.DeclarationQuestionType == DeclarationQuestionType.Title
                        ));

                    //Set answer false if null for question type = toggle / title
                    if (input.DeclarationAnswer is null && isToggleOrTitleQuestion)
                    {
                        c.DeclarationAnswer = false;
                    }
                }
            }

            var update = await _businessProfileRepository.UpdateCustomerBusinessDeclarationAnswersAsync(customerBusinessDeclarationAnswers);


            // 2) Check Rejection Matrix
            var questionA = declarationQuestions.Where(y => y.QuestionLabel.Contains('A')).FirstOrDefault();
            var answerA = customerBusinessDeclarationAnswers.Where(x => x.DeclarationQuestionCode == questionA.Id).FirstOrDefault();

            var questionB = declarationQuestions.Where(y => y.QuestionLabel.Contains('B')).FirstOrDefault();
            var answerB = customerBusinessDeclarationAnswers.Where(x => x.DeclarationQuestionCode == questionB.Id).FirstOrDefault();

            var questionsC = declarationQuestions.Where(y => y.QuestionLabel.Contains('C') && y.HasDocumentUpload is true).ToList(); //Filter C questions with HasDocumentUpload to remove title question
            var declarationQuestionCodesC = questionsC.Select(x => x.Id).ToList();
            var answersC = customerBusinessDeclarationAnswers.Where(x => declarationQuestionCodesC.Contains(x.DeclarationQuestionCode)).ToList();
            bool isOneOptionChosenC = answersC.Any(x => x.DeclarationAnswer == true); // If C has one question set true, then isOneOptionChosen is true
            if (questionsC is null)
            {
                isOneOptionChosenC = false;
            }

            var rejectionMatrix = await _businessProfileRepository.GetRejectionMatrixesByCustomerTypeAsync(customerType.Id);
            var rejectionMatrixMatch = rejectionMatrix
                .Where(x => 
                    x.A == answerA.DeclarationAnswer && 
                    x.B == answerB.DeclarationAnswer &&
                    x.C == isOneOptionChosenC)
                .FirstOrDefault();


            // 3) Update RedoCount (Customer can submit/redo max 3 times)
            // If Rejection Matrix has no matches AND no null answers then Business Declaration = Successful
            if (rejectionMatrixMatch == null && customerBusinessDeclarationAnswers.All(x => x.DeclarationAnswer != null))
            {
                var successful = await _businessProfileRepository.GetBusinessDeclarationStatus(BusinessDeclarationStatus.Successful.Id);
                customerBusinessDeclaration.BusinessDeclarationStatus = successful;
            }
            else
            {
                customerBusinessDeclaration.RedoCount--;
                var failed = await _businessProfileRepository.GetBusinessDeclarationStatus(BusinessDeclarationStatus.Failed.Id);
                customerBusinessDeclaration.BusinessDeclarationStatus = failed;

                if (customerBusinessDeclaration.RedoCount == 0)
                {
                    var blocked = await _businessProfileRepository.GetBusinessDeclarationStatus(BusinessDeclarationStatus.Blocked.Id);
                    customerBusinessDeclaration.BusinessDeclarationStatus = blocked;
                }
            }

            var outputDTO = new SaveCustomerBusinessDeclarationAnswersInputDTO();
            outputDTO = request.InputDTO;
            outputDTO.BusinessDeclarationStatusCode = customerBusinessDeclaration.BusinessDeclarationStatus.Id;
            outputDTO.BusinessDeclarationStatusDescription = customerBusinessDeclaration.BusinessDeclarationStatus.Name;

            // Set false if BusinessDeclaration is Successful or IsRedoBusinessDeclaration is null
            if (customerBusinessDeclaration.BusinessDeclarationStatus == BusinessDeclarationStatus.Successful)
            {
                customerBusinessDeclaration.IsRedoBusinessDeclaration = false;
            }
            else
                customerBusinessDeclaration.IsRedoBusinessDeclaration ??= false;

            customerBusinessDeclaration = await _businessProfileRepository.UpdateCustomerBusinessDeclarationAsync(customerBusinessDeclaration);
            return Result.Success(outputDTO);
        }
    }
}