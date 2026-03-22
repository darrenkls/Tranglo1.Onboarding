using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO.TransactionEvaluation;
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
    public class TransactionEvaluationController : ControllerBase
    {
        private readonly ILogger<TransactionEvaluationController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public TransactionEvaluationController(ILogger<TransactionEvaluationController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve Transaction Evaluation(s) per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/transaction-evaluations")]
        [ProducesResponseType(typeof(GetTransactionEvaluationOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetTransactionEvaluation), Tags = new[] { "KYC (Know Your Customer) - Transaction Evaluation" })]
        public async Task<ActionResult<GetTransactionEvaluationOutputDTO>> GetTransactionEvaluation([BusinessProfileId] int businessProfileCode)
        {
            GetTransactionEvaluationByIdQuery query = new GetTransactionEvaluationByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Add or Update AMLCFT Questionnaire(s) per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="transactionEvaluationInputDTO"></param>
        /// <param name="transactionEvalConcurrencyToken"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.ExternalBusinessOnlyPolicy)]
        [HttpPost("{businessProfileCode}/transaction-evaluations")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveTransactionEvaluation), Tags = new[] { "KYC (Know Your Customer) - Transaction Evaluation" })]
        public async Task<IActionResult> SaveTransactionEvaluation([BusinessProfileId] int businessProfileCode, [FromBody] TransactionEvaluationInputDTO transactionEvaluationInputDTO, Guid? transactionEvalConcurrencyToken)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            SaveTransactionEvaluationCommand command = new SaveTransactionEvaluationCommand
            {
                BusinessProfileCode = businessProfileCode,
                TransactionEvaluationInputDTO = transactionEvaluationInputDTO,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                TransactionEvalConcurrencyToken = transactionEvalConcurrencyToken
            };

            Result result = await Mediator.Send(command);
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

            return Ok();
        }
    }
}
