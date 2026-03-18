using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class SaveAMLCFTDisplayRuleCommand : BaseCommand<Result<AMLCFTDisplayRuleUpdateOutputDTO>>
    {
        public AMLCFTDisplayRuleUpdateInputDTO displayRuleUpdateInputDTO { get; set; }
        public override Task<string> GetAuditLogAsync(Result<AMLCFTDisplayRuleUpdateOutputDTO> result)
        {
            var action = GetCurrentAction();
            if (result.IsSuccess && action.HasValue)
            {
                if (action.Value.Equals(Action.Create))
                {
                    return Task.FromResult("Added Display Rule");
                }

                if (action.Value.Equals(Action.Update))
                {
                    return Task.FromResult("Edited Display Rule Display Rule");
                }
            }

            return base.GetAuditLogAsync(result);
        }

        private Action? GetCurrentAction()
        {
            if (displayRuleUpdateInputDTO.ActionCode.Equals("1"))
            {
                return Action.Create;
            }
            else if (displayRuleUpdateInputDTO.ActionCode.Equals("2"))
            {
                return Action.Update;
            }
            else 
            {
                return null;
            }
        }

        private enum Action 
        {
            Create,
            Update
        }

        public class SaveAMLCFTDisplayRuleCommandHandler : IRequestHandler<SaveAMLCFTDisplayRuleCommand, Result<AMLCFTDisplayRuleUpdateOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _repository;
            private readonly IApplicationUserRepository _applicationUserRepository;
            private BusinessProfileDbContext _context;
            public SaveAMLCFTDisplayRuleCommandHandler(IConfiguration config, BusinessProfileService businessProfileService, 
                IBusinessProfileRepository repository, BusinessProfileDbContext context, IApplicationUserRepository applicationUserRepository)
            {
                _config = config;
                _businessProfileService = businessProfileService;
                _repository = repository;
                _context = context;
                _applicationUserRepository = applicationUserRepository;
            }

            public async Task<Result<AMLCFTDisplayRuleUpdateOutputDTO>> Handle(SaveAMLCFTDisplayRuleCommand request, CancellationToken cancellationToken)
            {
                if(request.displayRuleUpdateInputDTO.ActionCode != 1 && request.displayRuleUpdateInputDTO.ActionCode != 2)
                {
                    return Result.Failure<AMLCFTDisplayRuleUpdateOutputDTO>("Invalid Action Code");
                }
                var entityCode = _context.EntityTypes.FirstOrDefault(x => x.Id == request.displayRuleUpdateInputDTO.EntityTypeCode);
                var tieUp = _context.RelationshipTieUps.FirstOrDefault(x => x.Id == request.displayRuleUpdateInputDTO.RelationshipTieUpCode);
                var servicesOffered = _context.ServicesOffered.FirstOrDefault(x => x.Id == request.displayRuleUpdateInputDTO.ServicesOfferedCode);
                var questionnaire = await _businessProfileService.GetQuestionnaireByCode(request.displayRuleUpdateInputDTO.QuestionnaireCode);
                
                if (request.displayRuleUpdateInputDTO.DisplayRuleCode == null && request.displayRuleUpdateInputDTO.ActionCode == 1)
                {
                    var getDisplayRule = await _repository.GetAMLCFTDisplayRuleQuestionnaireAsync(entityCode, tieUp, servicesOffered, questionnaire);
                    
                    if (getDisplayRule != null)
                    {
                        return Result.Failure<AMLCFTDisplayRuleUpdateOutputDTO>("Display Rule already exists");
                    }

                    if (entityCode == null && tieUp == null && servicesOffered == null)
                    {
                        return Result.Failure<AMLCFTDisplayRuleUpdateOutputDTO>("At least One Dropdown value must be selected");
                    }
                                        
                    var displayRule = new AMLCFTDisplayRules(entityCode, tieUp, servicesOffered, questionnaire);
                    var addDisplayRule = await _repository.AddAMLCFTDisplayRules(displayRule);
                    var getName = await _applicationUserRepository.GetTrangloUserByUserId((int)addDisplayRule.CreatedBy);
                    var result = new AMLCFTDisplayRuleUpdateOutputDTO
                    {
                        DisplayRuleCode = addDisplayRule.Id,
                        QuestionnaireCode = addDisplayRule.Questionnaire.Id,
                        EntityTypeCode = addDisplayRule.EntityType?.Id,
                        EntityTypeDescription = addDisplayRule.EntityType?.Name,
                        RelationshipTieUpCode = addDisplayRule.RelationshipTieUp?.Id,
                        RelationshipTieUpDescription = addDisplayRule.RelationshipTieUp?.Name,
                        ServicesOfferedCode = addDisplayRule.ServicesOffered?.Id,
                        ServicesOfferedDescription = addDisplayRule.ServicesOffered?.Name,
                        FullName = getName.FullName.Value,
                        UpdatedDate = addDisplayRule.CreatedDate
                    };
                    return Result.Success(result);
                    


                }
                else if(request.displayRuleUpdateInputDTO.DisplayRuleCode != null && request.displayRuleUpdateInputDTO.ActionCode == 2)
                {
                    var getDisplayRule = await _repository.GetAMLCFTDisplayRulesByCodeAsync((long)request.displayRuleUpdateInputDTO.DisplayRuleCode);
                    var getDisplayRuleByQuestionnaire = await _repository.GetAMLCFTDisplayRuleByQuestionnaireAsync(entityCode, tieUp, servicesOffered, questionnaire);
                    var entityCodefromDb = getDisplayRule.EntityType;
                    var tieUpfromDb = getDisplayRule.RelationshipTieUp;
                    var serviceOfferedfromDb = getDisplayRule.ServicesOffered;
                    var questionnairefromdb = getDisplayRule.Questionnaire;
                    



                    if (entityCode == null && tieUp == null && servicesOffered == null)
                    {
                        return Result.Failure<AMLCFTDisplayRuleUpdateOutputDTO>("At Least One Dropdown value must be selected");
                    }


                    if (entityCodefromDb == entityCode && tieUpfromDb == tieUp && serviceOfferedfromDb == servicesOffered)
                    {

                        getDisplayRule.ServicesOffered = servicesOffered;
                        getDisplayRule.RelationshipTieUp = tieUp;
                        getDisplayRule.EntityType = entityCode;
                        var updateDisplayRule = await _repository.UpdateAMLCFTDisplayRules(getDisplayRule);
                        var getName = await _applicationUserRepository.GetTrangloUserByUserId((int)updateDisplayRule.LastModifiedBy);
                        var result = new AMLCFTDisplayRuleUpdateOutputDTO
                        {
                            DisplayRuleCode = updateDisplayRule.Id,
                            QuestionnaireCode = updateDisplayRule.Questionnaire.Id,
                            EntityTypeCode = updateDisplayRule.EntityType?.Id,
                            EntityTypeDescription = updateDisplayRule.EntityType?.Name,
                            RelationshipTieUpCode = updateDisplayRule.RelationshipTieUp?.Id,
                            RelationshipTieUpDescription = updateDisplayRule.RelationshipTieUp?.Name,
                            ServicesOfferedCode = updateDisplayRule.ServicesOffered?.Id,
                            ServicesOfferedDescription = updateDisplayRule.ServicesOffered?.Name,
                            FullName = getName.FullName.Value,
                            UpdatedDate = updateDisplayRule.LastModifiedDate
                        };
                        return Result.Success(result);
                    }
                    else
                          if (getDisplayRuleByQuestionnaire != null)
                    {
                        //if (getDisplayRule == null)
                        //{
                        //    return Result.Failure<AMLCFTDisplayRuleUpdateOutputDTO>("Display Rule does not exist");
                        //}
                        return Result.Failure<AMLCFTDisplayRuleUpdateOutputDTO>
                        ($"Display Rule already exists.");


                    }
                    else
                    {
                        getDisplayRule.ServicesOffered = servicesOffered;
                        getDisplayRule.RelationshipTieUp = tieUp;
                        getDisplayRule.EntityType = entityCode;
                        var updateDisplayRule = await _repository.UpdateAMLCFTDisplayRules(getDisplayRule);
                        var getName = await _applicationUserRepository.GetTrangloUserByUserId((int)updateDisplayRule.LastModifiedBy);
                        var result = new AMLCFTDisplayRuleUpdateOutputDTO
                        {
                            DisplayRuleCode = updateDisplayRule.Id,
                            QuestionnaireCode = updateDisplayRule.Questionnaire.Id,
                            EntityTypeCode = updateDisplayRule.EntityType?.Id,
                            EntityTypeDescription = updateDisplayRule.EntityType?.Name,
                            RelationshipTieUpCode = updateDisplayRule.RelationshipTieUp?.Id,
                            RelationshipTieUpDescription = updateDisplayRule.RelationshipTieUp?.Name,
                            ServicesOfferedCode = updateDisplayRule.ServicesOffered?.Id,
                            ServicesOfferedDescription = updateDisplayRule.ServicesOffered?.Name,
                            FullName = getName.FullName.Value,
                            UpdatedDate = updateDisplayRule.LastModifiedDate
                        };
                        return Result.Success(result);
                    }
                    

                }
                return null;
            }
        }
    }
}
