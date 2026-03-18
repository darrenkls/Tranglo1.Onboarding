using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.DTO.Documentation.AdminTemplateManagementInputDTO;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.Queries;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class SaveTemplateManagementCommand : BaseCommand<Result<bool>>
    {
        public List<AdminTemplateManagementInputDTO> InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<bool> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Edit Templates Management";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveTemplateManagementCommandHandler : IRequestHandler<SaveTemplateManagementCommand, Result<bool>>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ILogger<SaveTemplateManagementCommandHandler> _logger;


        public SaveTemplateManagementCommandHandler(IBusinessProfileRepository businessProfileRepository, ILogger<SaveTemplateManagementCommandHandler> logger)
        {
            _businessProfileRepository = businessProfileRepository;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(SaveTemplateManagementCommand request, CancellationToken cancellationToken)
        {
            foreach (var tm in request.InputDTO)
            {
                if (tm.QuestionnaireCode.HasValue)
                {
                    var questionnaireInfo = await _businessProfileRepository.GetQuestionnaireByQuestionnaireCodeAsync((long)tm.QuestionnaireCode);
                    if (questionnaireInfo is null)
                    {
                        return Result.Failure<bool>(
                                        $"Questionnaire is not exist");
                    }
                }


                    foreach (var en in tm.TrangloEntities)
                    {

                        QuestionManagement output = new QuestionManagement()
                        {
                            TrangloEntityCode = en.TrangloEntityCode,
                            IsChecked = en.IsChecked,
                            QuestionnaireCode = tm.QuestionnaireCode,
                            SolutionCode = tm.SolutionCode

                        };


                        var existingQM = await _businessProfileRepository.GetAdminTemplateManagement(output);

                        if (en.IsChecked == false)
                        {
                            if (existingQM != null)
                            {
                                existingQM.IsChecked = false;
                                //Update template management
                                await _businessProfileRepository.UpdateAdminTemplateManagement(existingQM);
                            }
                            else
                            {
                                return Result.Failure<bool>(
                                        $"Admin Template Management is not exist");
                            }
                        }
                        else if (en.IsChecked == true)
                        {
                            if (existingQM == null)
                            {
                                //Save template management
                                await _businessProfileRepository.SaveAdminTemplateManagement(output);
                            }
                            else if (existingQM != null)
                            {
                                if (existingQM.IsChecked != en.IsChecked)
                                {
                                    existingQM.IsChecked = true;
                                    //Update template management
                                    await _businessProfileRepository.UpdateAdminTemplateManagement(existingQM);
                                }
                                else
                                {
                                    return Result.Failure<bool>(
                                            $"Admin Template Management is existed");
                                }
                            }

                        }
                        else
                        {
                            return Result.Failure<bool>(
                                    $"Unable to add/update Admin Template Management");
                        }

                    }
                
            }
            return Result.Success(true);
        }
    }
}
