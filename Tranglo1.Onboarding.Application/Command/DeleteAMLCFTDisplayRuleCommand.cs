using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class DeleteAMLCFTDisplayRuleCommand : BaseCommand<Result<AMLCFTDisplayRules>>
    {
        //public long QuestionnaireCode { get; set; }
        public long DisplayRuleCode { get; set; }
        public override Task<string> GetAuditLogAsync(Result<AMLCFTDisplayRules> result)
        {
            if (result.IsSuccess) 
            {
                return Task.FromResult("Removed Display Rule");
            }
            return base.GetAuditLogAsync(result);
        }
    }
    internal class DeleteAMLCFTDisplayRuleCommandHandler : IRequestHandler<DeleteAMLCFTDisplayRuleCommand, Result<AMLCFTDisplayRules>>
    {
        private readonly IConfiguration _config;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;

        public DeleteAMLCFTDisplayRuleCommandHandler(IConfiguration config, BusinessProfileService businessProfileService, IBusinessProfileRepository repository)
        {
            _config = config;
            _businessProfileService = businessProfileService;
            _repository = repository;
        }
        public async Task<Result<AMLCFTDisplayRules>> Handle(DeleteAMLCFTDisplayRuleCommand request, CancellationToken cancellationToken)
        {
          
            var getDisplayRules = await _repository.GetAMLCFTDisplayRulesByCodeAsync(request.DisplayRuleCode);
            var removeDisplayRules = await _repository.DeleteAMLCFTDisplayRulesAsync(getDisplayRules);
            if (removeDisplayRules.IsFailure)
            {
                return Result.Failure<AMLCFTDisplayRules>(
                                $"Unable to remove Display Rule"
                                );
            }
            return Result.Success(removeDisplayRules.Value);
        }
    }
}
