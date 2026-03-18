using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOnlineAMLCFTQuestionnaires, UACAction.Edit)]
    internal class UpdateQuestionnaireStatusCommand : BaseCommand<Result<QuestionnaireListOutputDTO>>
    {
        public long QuestionnaireCode { get; set; }
        public UpdateQuestionnaireStatusInputDTO UpdateQuestionnaireStatus { get; set; }


        public override Task<string> GetAuditLogAsync(Result<QuestionnaireListOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                if (UpdateQuestionnaireStatus.IsActive)
                {
                    string _description = string.Format("Enabled AMLCFT Questionnaire");
                    return Task.FromResult(_description);
                }
                else
                {
                    string _description = string.Format("Disabled AMLCFT Questionnaire");
                    return Task.FromResult(_description);
                }
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdateQuestionnaireStatusCommandHandler : IRequestHandler<UpdateQuestionnaireStatusCommand, Result<QuestionnaireListOutputDTO>>
    {
        private readonly ILogger<UpdateQuestionnaireStatusCommandHandler> _logger;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public IUnitOfWork _unitOfWork { get; }

        public UpdateQuestionnaireStatusCommandHandler(ILogger<UpdateQuestionnaireStatusCommandHandler> logger, 
            IBusinessProfileRepository businessProfileRepository, 
            IConfiguration config, 
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _businessProfileRepository = businessProfileRepository;
            _config = config;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<QuestionnaireListOutputDTO>> Handle(UpdateQuestionnaireStatusCommand request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");

            var questionnaire = await _businessProfileRepository.GetQuestionnaireByQuestionnaireCodeAsync(request.QuestionnaireCode);

            if (questionnaire == null)
            {
                _logger.LogError($"[UpdateQuestionnaireStatus] QuestionnaireCode: {request.QuestionnaireCode} not found.");
                return Result.Failure<QuestionnaireListOutputDTO>($"No questionnaire found for QuestionnaireCode: {request.QuestionnaireCode}");
            }

            questionnaire.IsActive = request.UpdateQuestionnaireStatus.IsActive;

            var update = await _businessProfileRepository.UpdateQuestionnaireStatusAsync(questionnaire);

            if (update.IsFailure)
            {
                _logger.LogError($"[UpdateQuestionnaireStatus] Failed to update status for QuestionnaireCode: {request.QuestionnaireCode}");
                return Result.Failure<QuestionnaireListOutputDTO>($"Failed to update status for QuestionnaireCode: {request.QuestionnaireCode}");
            }

            QuestionnaireListOutputDTO output = await _unitOfWork.Connection.QueryFirstAsync<QuestionnaireListOutputDTO>(
                                        "dbo.GetQuestionnaireDetail",
                                        new
                                        {
                                            QuestionnaireCode = request.QuestionnaireCode
                                        },
                                        _unitOfWork.Transaction, null, CommandType.StoredProcedure);

            

            return Result.Success(output);
        }
    }
}