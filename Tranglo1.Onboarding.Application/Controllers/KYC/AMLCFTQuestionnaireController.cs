using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Security;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Tranglo1.Onboarding.Application.Controllers.KYC
{
    [ApiController]
    [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/kyc")]
    [LogInputDTO]
    [LogOutputDTO]
    public class AMLCFTQuestionnaireController : ControllerBase
    {
        private readonly ILogger<AMLCFTQuestionnaireController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public AMLCFTQuestionnaireController(ILogger<AMLCFTQuestionnaireController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve AMLCFT Flag per BusinessProfileCode (Check HasUploadedAMLDocumentation and CheckHasAnsweredAMLQuestionnaire)
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/aml-cft-flag")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(AMLCFTFlagOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetAMLCFTFlag), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<AMLCFTFlagOutputDTO> GetAMLCFTFlag([BusinessProfileId] int businessProfileCode, [FromQuery] long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);

            GetAMLCFTFlagByIdQuery query = new GetAMLCFTFlagByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                LoginId = User.GetSubjectId().Value
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve AMLCFT Questionnaire(s) per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/aml-cft-questionnaires")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AMLCFTQuestionnaireOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAMLCFTQuestionnaires), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<IEnumerable<AMLCFTQuestionnaireOutputDTO>> GetAMLCFTQuestionnaires([BusinessProfileId] int businessProfileCode, long? adminSolution, int trangloEntityCode)
        {

            GetAMLCFTQuestionnaireByIdQuery query = new GetAMLCFTQuestionnaireByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution,
                TrangloEntityCode = trangloEntityCode
            };
            query.LoginId = User.GetSubjectId().Value;
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            query.CustomerSolution = solution.HasValue ? solution.Value : null;


            return await Mediator.Send(query);
        }

        /// <summary>
        /// Add or Update AMLCFT Questionnaire(s) per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="questionnaireDTO"></param>
        /// <param name="adminSolution"></param>
        /// <param name="aMLCFTQuestionnaireConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/aml-cft-questionnaires")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(AMLCFTQuestionnaireAnswersOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveAMLCFTQuestionnaireAnswers), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<IActionResult> SaveAMLCFTQuestionnaireAnswers([BusinessProfileId] int businessProfileCode, [FromBody] 
        IEnumerable<AMLCFTQuestionnaireInputDTO> questionnaireDTO, long? adminSolution, Guid? aMLCFTQuestionnaireConcurrencyToken)
        {
            //SaveAMLCFTQuestionnaireAnswersCommand command = _mapper.Map<SaveAMLCFTQuestionnaireAnswersCommand>(questionnaireAnswersDTO);
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SaveAMLCFTQuestionnaireAnswersCommand command = new SaveAMLCFTQuestionnaireAnswersCommand
            {
                BusinessProfileCode = businessProfileCode,
                QuestionnaireDTO = questionnaireDTO,
                LoginId = User.GetSubjectId().Value,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                AMLCFTQuestionnaireConcurrencyToken = aMLCFTQuestionnaireConcurrencyToken
            };

            var result = await Mediator.Send(command);

             if (result.IsFailure)
            {
                // Check if the error message contains "ConcurrencyError"
                if (result.Error.Contains("ConcurrencyError"))
                {
                    // Handle the concurrency error by returning a custom response with 409 Conflict
                    return Conflict(new { ErrorCode = "ConcurrencyError", ErrorMessage = "Data has been modified by another user. Please refresh and try again." });
                }

                // Handle other types of errors as needed
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieve AMLCFT Questionnaire Display Rule
        /// </summary>
        /// <param name="questionnaireCode"></param>
        /// <returns></returns>
        [HttpGet("aml-cft-questionnaires/{questionnaireCode}/display-rules")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(AMLCFTDisplayRuleOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAMLCFTDisplayRule), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<AMLCFTDisplayRuleOutputDTO> GetAMLCFTDisplayRule(long questionnaireCode)
        {
            GetDisplayRuleByQuestionnaireCodeQuery query = new GetDisplayRuleByQuestionnaireCodeQuery
            {
                QuestionnaireCode = questionnaireCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve AMLCFT Questionnaire Display Rule
        /// </summary>
        //// <param name="questionnaireCode"></param>
        /// <param name="displayRuleCode"></param>
        //// <param name="displayRuleDeleteInputDTO"></param>
        /// <returns></returns>
        [HttpDelete("aml-cft-questionnaires/display-rules/{displayRuleCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(AMLCFTDisplayRuleDeleteInputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RemoveAMLCFTDisplayRule), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<Result<AMLCFTDisplayRules>> RemoveAMLCFTDisplayRule(long displayRuleCode)
        {
            DeleteAMLCFTDisplayRuleCommand query = new DeleteAMLCFTDisplayRuleCommand
            {
                //QuestionnaireCode = questionnaireCode,
                DisplayRuleCode = displayRuleCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve AMLCFT Questionnaire Display Rule
        /// </summary>
        //// <param name="questionnaireCode"></param>
        /// <param name="displayRuleCode"></param>
        //// <param name="displayRuleDeleteInputDTO"></param>
        /// <returns></returns>
        [HttpPost("aml-cft-questionnaires/display-rules")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(AMLCFTDisplayRuleUpdateInputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveAMLCFTDisplayRule), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<Result<AMLCFTDisplayRuleUpdateOutputDTO>> SaveAMLCFTDisplayRule(AMLCFTDisplayRuleUpdateInputDTO displayRuleInput)
        {
            SaveAMLCFTDisplayRuleCommand query = new SaveAMLCFTDisplayRuleCommand
            {

                displayRuleUpdateInputDTO = displayRuleInput
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Update Questionnaire Status
        /// </summary>
        /// <param name="questionnaireCode"></param>
        /// <param name="updateQuestionnaireStatusInputDTO"></param>
        /// <returns></returns>
        [HttpPut("aml-cft-questionnaires/{questionnaireCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(QuestionnaireListOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PutQuestionnaireStatus), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<IActionResult> PutQuestionnaireStatus([FromRoute] long questionnaireCode, [FromBody] UpdateQuestionnaireStatusInputDTO updateQuestionnaireStatusInputDTO)
        {
            UpdateQuestionnaireStatusCommand command = new UpdateQuestionnaireStatusCommand
            {
                QuestionnaireCode = questionnaireCode,
                UpdateQuestionnaireStatus = updateQuestionnaireStatusInputDTO
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdateQuestionnaireStatus] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Retrieve AMLCFT Questionnaire list
        /// </summary>
        /// <returns></returns>
        [HttpGet("aml-cft-questionnaires")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<QuestionnaireListOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAMLCFTQuestionnaireList), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<Result<IEnumerable<QuestionnaireListOutputDTO>>> GetAMLCFTQuestionnaireList()
        {
            GetAMLCFTQuestionnaireListQuery query = new GetAMLCFTQuestionnaireListQuery { };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve AMLCFT Questionnaire per QuestionnaireCode
        /// </summary>
        /// <param name="questionnaireCode"></param>
        /// <returns></returns>
        [HttpGet("aml-cft-questionnaires/{questionnaireCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AdminAMLCFTQuestionnaireOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAMLCFTQuestionnaire), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<IEnumerable<AdminAMLCFTQuestionnaireOutputDTO>> GetAMLCFTQuestionnaire([FromRoute] long questionnaireCode)
        {
            GetQuestionnaireQuery query = new GetQuestionnaireQuery
            {
                QuestionnaireCode = questionnaireCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Add or Update AMLCFT Questionnaire
        /// </summary>
        /// <param name="questionnaireCode"></param>
        ///// <param name="questionnaireDTO"></param>
        /// <returns></returns>
        [HttpPost("aml-cft-questionnaires")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveAMLCFTQuestionnaire), Tags = new[] { "KYC (Know Your Customer) - AML/CFT Questionnaire" })]
        public async Task<IActionResult> SaveAMLCFTQuestionnaire([FromBody] AdminAMLCFTQuestionnaireOutputDTO questionnaireDTO)
        {
            SaveAMLCFTQuestionnaireCommand command = new SaveAMLCFTQuestionnaireCommand
            {
                QuestionnaireDTO = questionnaireDTO
            };

            Result result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SaveAMLCFTQuestionnaire] {result.Error}");
                return BadRequest(result);
            }

            return Ok();
        }

    }
}
