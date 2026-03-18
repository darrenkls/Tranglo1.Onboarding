using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.KYCCustomerSummaryFeedbackNotification;
using Tranglo1.Onboarding.Application.Helper;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Security;

namespace Tranglo1.Onboarding.Application.Controllers.KYC
{
    [ApiController]
    [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/kyc")]
    [LogInputDTO]
    [LogOutputDTO]
    public class KYCSummaryController : ControllerBase
    {
        private readonly ILogger<KYCSummaryController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public KYCSummaryController(ILogger<KYCSummaryController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Insert New Customer Feedback for KYC Summary
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="feedbackInputDTO"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/kyc-customer-summary-feedback")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(KYCCustomerSummaryFeedbackOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PostKYCCustomerSummaryFeedback), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> PostKYCCustomerSummaryFeedback([BusinessProfileId] int businessProfileCode, [FromBody] KYCCustomerSummaryFeedbackInputDTO feedbackInputDTO)
        {
            SaveKYCCustomerSummaryFeedbackCommand command = new SaveKYCCustomerSummaryFeedbackCommand
            {
                BusinessProfileCode = businessProfileCode,
                KYCCustomerSummaryFeedback = feedbackInputDTO,
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            command.CustomerSolution = solution.HasValue ? solution.Value : null;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PostKYCCustomerSummaryFeedback] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Update Customer Feedback for KYC Summary
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="feedbackInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/kyc-customer-summary-feedback")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(KYCCustomerSummaryFeedbackOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PutKYCCustomerSummaryFeedback), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> PutKYCCustomerSummaryFeedback([BusinessProfileId] int businessProfileCode, [FromBody] KYCCustomerSummaryFeedbackInputDTO feedbackInputDTO)
        {
            UpdateKYCCustomerSummaryFeedbackCommand command = new UpdateKYCCustomerSummaryFeedbackCommand
            {
                BusinessProfileCode = businessProfileCode,
                KYCCustomerSummaryFeedback = feedbackInputDTO
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            command.CustomerSolution = solution.HasValue ? solution.Value : null;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PostKYCCustomerSummaryFeedback] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Get Customer Feedback for KYC Summary
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/kyc-customer-summary-feedback")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(KYCCustomerSummaryFeedbackOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCCustomerSummaryFeedback), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> GetKYCCustomerSummaryFeedback([BusinessProfileId] int businessProfileCode, long? adminSolution)
        {
            GetKYCCustomerSummaryFeedbackByBusinessProfileQuery query = new GetKYCCustomerSummaryFeedbackByBusinessProfileQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;
            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetKYCCustomerSummaryFeedback] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Insert New KYC Summary Feedback
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="feedbackInputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/kyc-summary-feedback")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(KYCSummaryFeedbackOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PostKYCSummaryFeedback), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]

        public async Task<IActionResult> PostKYCSummaryFeedback([BusinessProfileId] int businessProfileCode, [FromBody] KYCSummaryFeedbackInputDTO feedbackInputDTO, long? adminSolution)
        {
            feedbackInputDTO.KYCSummaryFeedbackCode = 0;

            SaveKYCSummaryFeedbackCommand command = new SaveKYCSummaryFeedbackCommand
            {
                BusinessProfileCode = businessProfileCode,
                KYCSummaryFeedback = feedbackInputDTO
            };

            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PostKYCSummaryFeedback] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Update KYC Summary Feedback
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="feedbackInputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/kyc-summary-feedback")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(KYCSummaryFeedbackOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PutKYCSummaryFeedback), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]

        public async Task<IActionResult> PutKYCSummaryFeedback([BusinessProfileId] int businessProfileCode, [FromBody] KYCSummaryFeedbackInputDTO feedbackInputDTO, long? adminSolution)
        {
            UpdateKYCSummaryFeedbackCommand command = new UpdateKYCSummaryFeedbackCommand
            {
                BusinessProfileCode = businessProfileCode,
                KYCSummaryFeedback = feedbackInputDTO
            };

            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PutKYCSummaryFeedback] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Get KYC Summary Feedback
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param> 
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/kyc-summary-feedback")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<KYCSummaryFeedbackOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCSummaryFeedback), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> GetKYCSummaryFeedback([BusinessProfileId] int businessProfileCode, long? adminSolution)
        {
            GetKYCSummaryFeedbackByBusinessProfileQuery query = new GetKYCSummaryFeedbackByBusinessProfileQuery
            {
                BusinessProfileCode = businessProfileCode,
                UserType = User.GetUserType()
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;
            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetKYCSummaryFeedback] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieve list of KYC Business Categories
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("{businessProfileCode}/kyc-business-categories")]
        [Authorize(Policy = AuthenticationPolicies.ExternalBusinessOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<KYCBusinessCategoriesOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCBusinessCategories), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IEnumerable<KYCBusinessCategoriesOutputDTO>> GetKYCBusinessCategories([BusinessProfileId] int businessProfileCode)
        {
            GetKYCBusinessCategoriesQuery query = new GetKYCBusinessCategoriesQuery()
            {
                BusinessProfileCode = businessProfileCode,
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve list of KYC Connect Categories
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("kyc-connect-categories")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<KYCConnectCategoriesOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCConnectCategories), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> GetKYCConnectCategories()
        {
            GetKYCConnectCategoriesQuery query = new GetKYCConnectCategoriesQuery();

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                _logger.LogError($"[{ExtensionHelper.GetMethodName()}]: {result.Error}");
                ModelState.AddModelError("Error", result.Error);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get Has Unread Customer Feedback Notification(s) for KYC Customer Summary (For Admin only)
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/has-unread-kyc-customer-summary-feedback-notification")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(GetHasUnreadKYCCustomerSummaryFeedbackNotificationOutputDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(HasUnreadKYCCustomerSummaryFeedbackNotifications), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> HasUnreadKYCCustomerSummaryFeedbackNotifications([BusinessProfileId] int businessProfileCode, int? adminSolution)
        {
            GetHasUnreadKYCCustomerSummaryFeedbackNotificationQuery query = new GetHasUnreadKYCCustomerSummaryFeedbackNotificationQuery
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError("{0} {1}", nameof(HasUnreadKYCCustomerSummaryFeedbackNotifications), result.Error);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Mark notification as read for KYC Customer Summary (For Admin only)
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/mark-kyc-customer-summary-feedback-notifications-as-read")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(MarkKYCCustomerSummaryFeedbackNotificationsAsRead), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> MarkKYCCustomerSummaryFeedbackNotificationsAsRead([BusinessProfileId] int businessProfileCode, int? adminSolution)
        {
            MarkKYCSummaryNotificationsAsReadCommand command = new MarkKYCSummaryNotificationsAsReadCommand
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError("{0} {1}", nameof(HasUnreadKYCCustomerSummaryFeedbackNotifications), result.Error);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get Has Unread Feedback Notification(s) for KYC Summary (For Business only)
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/has-unread-kyc-summary-feedback-notification")]
        [Authorize(Policy = AuthenticationPolicies.ExternalBusinessOnlyPolicy)]
        [ProducesResponseType(typeof(GetHasUnreadKYCSummaryFeedbackNotificationOutputDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(HasUnreadKYCSummaryFeedbackNotifications), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> HasUnreadKYCSummaryFeedbackNotifications([BusinessProfileId] int businessProfileCode)
        {
            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);

            GetHasUnreadKYCSummaryFeedbackNotificationQuery query = new GetHasUnreadKYCSummaryFeedbackNotificationQuery
            {
                BusinessProfileCode = businessProfileCode,
                CustomerSolution = solution.HasValue ? solution.Value : null
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError("{0} {1}", nameof(HasUnreadKYCSummaryFeedbackNotifications), result.Error);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get Boolean for determining any pending document for Admin proceed the KYC approval.
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpGet("{businessProfileCode}/has-pending-review-document")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<KYCBusinessCategoriesOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(HasPendingReviewDocument), Tags = new[] { "KYC (Know Your Customer - KYC Summary)" })]
        public async Task<IActionResult> HasPendingReviewDocument([BusinessProfileId] int businessProfileCode)
        {
            GetHasPendingReviewKYCDocumentByBusinessProfileQuery query = new GetHasPendingReviewKYCDocumentByBusinessProfileQuery()
            {
                BusinessProfileCode = businessProfileCode,
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError("{Action} {Error}", nameof(HasPendingReviewDocument), result.Error);

                return ValidationProblem();
            }

            return Ok(result.Value);
        }

    }
}
