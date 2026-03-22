using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.Onboarding.Application.DTO.CommentAndReviewRemarks;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.DTO.Documentation.AdminTemplateManagementInputDTO;
using Tranglo1.Onboarding.Application.DTO.Documentation.AdminTemplateOutputDTO;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.Helper;
using Tranglo1.Onboarding.Application.Infrastructure.Swagger;
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
    public class KYCController : ControllerBase
    {
        private readonly ILogger<KYCController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public KYCController(ILogger<KYCController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }



        /// <summary>
        /// Update KYC Result Reviews per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="kysStatusReviewInputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <param name="reviewAndFeedbackConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/kyc-result-reviews")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<KYCStatusReviewsOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PostKYCResultReviews), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> PostKYCResultReviews([BusinessProfileId] int businessProfileCode, 
            [FromBody] KYCStatusReviewsInputDTO kysStatusReviewInputDTO, int adminSolution,Guid? reviewAndFeedbackConcurrencyToken, string trangloEntity)
        {
            UpdateKYCStatusReviewsCommand command = new UpdateKYCStatusReviewsCommand
            {
                BusinessProfileCode = businessProfileCode,
                InputDTO = kysStatusReviewInputDTO,
                AdminSolution = adminSolution,
                ReviewAndFeedbackConcurrencyToken = reviewAndFeedbackConcurrencyToken,
                TrangloEntity = trangloEntity
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
                _logger.LogError($"[PostKYCResultReviews] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }
        /// <summary>
        /// Add the assigned Compliance Officer per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="complianceOfficerInputDTO"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/compliance-officer-assignment")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(KYCComplianceOfficerOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(PostComplianceOfficerAssignmentByBusinessProfile), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> PostComplianceOfficerAssignmentByBusinessProfile([BusinessProfileId] int businessProfileCode, [FromBody] GetComplianceOfficerInputDTOByBusinessProfile complianceOfficerInputDTO, int adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            PostComplianceOfficerAssignmentCommand command = new PostComplianceOfficerAssignmentCommand
            {
                COOfficerAssignedLoginID = complianceOfficerInputDTO.COOfficerAssignedLoginID,
                BusinessProfileCode = businessProfileCode,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                AdminSolution = adminSolution
            };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PostComplianceOfficerAssignmentByBusinessProfile] {result.Error}");
                return this.ValidationProblem();
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Search business profiles based on the parameter criteria for approval history
        /// </summary>
        /// <param name="partnerName"></param>
        /// <param name="countryISO2"></param>
        /// <param name="complianceOfficerLogin"></param>
        /// <param name="fromRegistrationDate"></param>
        /// <param name="toRegistrationDate"></param>
        /// <param name="fromApproveDate"></param>
        /// <param name="toApproveDate"></param>
        /// <param name="fromRejectDate"></param>
        /// <param name="toRejectDate"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// /// <param name="entityCode"></param>
        /// <returns></returns>
        [HttpGet("approval-histories")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        //[Authorize(Roles = "Master Admin")] //TODO: Enable 
        [ProducesResponseType(typeof(PagedResult<BusinessProfileApprovalHistoryOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAdminBusinessProfile), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<PagedResult<BusinessProfileApprovalHistoryOutputDTO>> GetAdminBusinessProfile(string partnerName, string countryISO2, string complianceOfficerLogin,
            string fromRegistrationDate, string toRegistrationDate, string fromApproveDate, string toApproveDate,
            string fromRejectDate, string toRejectDate, int pageSize, int page, string sortExpression, SortDirection sortDirection, [TrangloEntityId] string entityCode)
        {
            GetBusinessProfilesApproveHistoryQuery query = new GetBusinessProfilesApproveHistoryQuery
            {
                PartnerName = partnerName,
                CountryISO2 = countryISO2,
                ComplianceOfficerLoginId = complianceOfficerLogin,
                FromRegistrationDate = fromRegistrationDate,
                ToRegistrationDate = toRegistrationDate,
                FromApproveDate = fromApproveDate,
                ToApproveDate = toApproveDate,
                FromRejectDate = fromRejectDate,
                ToRejectDate = toRejectDate,
                EntityCode = entityCode,
                PagingOptions = new PagingOptions 
                {
                    PageSize = pageSize,
                    PageIndex = page,
                    SortExpression = sortExpression,
                    Direction = sortDirection
                }
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Business Profile per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/business-profiles")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        //[Authorize(Roles = "Master Admin")] //TODO: Enable 
        [ProducesResponseType(typeof(BusinessProfileOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetBusinessProfile), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetBusinessProfile([BusinessProfileId] int businessProfileCode)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            GetBusinessProfileQuery query = new GetBusinessProfileQuery
            {
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                BusinessProfileCode = businessProfileCode
            };

            Result<BusinessProfileOutputDTO> result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetBusinessProfile] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }
        /// <summary>
        /// Retrieve Verified Business Profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("approved-business-profiles")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [SwaggerOperation(OperationId = nameof(GetBusinessProfileList), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<List<BusinessProfileListOutputDTO>> GetBusinessProfileList()
        {
            GetBusinessProfileList query = new GetBusinessProfileList();
            return await Mediator.Send(query);
        }
        /// <summary>
        /// Add or update Business Profile per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="businessProfile"></param>
        /// <param name="adminSolution"></param>
        /// <param name="businessProfileConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/business-profiles")]
        //[Authorize(Roles = "Master Admin")] //TODO: Enable
        [ProducesResponseType(typeof(BusinessProfileOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = nameof(UpdateBusinessProfile), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> UpdateBusinessProfile([BusinessProfileId] int businessProfileCode, [FromBody] BusinessProfileInputDTO businessProfile, long? adminSolution, Guid? businessProfileConcurrencyToken, bool fromComment)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            UpdateBusinessProfileCommand command = _mapper.Map<UpdateBusinessProfileCommand>(businessProfile);
            command.LoginId = subjectId.HasValue ? subjectId.Value : null;
            command.BusinessProfileCode = businessProfileCode;
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;
            command.BusinessProfileConcurrencyToken = businessProfileConcurrencyToken;
            command.FromComment = fromComment;

            Result<BusinessProfileOutputDTO> result = await Mediator.Send(command);

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
                _logger.LogError($"[UpdateBusinessProfile] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Submit the business profile and let Tranglo Compliance to do verification for Admin or Tranglo Connect User
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="reviewConcurrencyToken"></param>

        /// <returns></returns>
        [HttpPost("{businessProfileCode}/verification")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        //[Authorize(Roles = "Master Admin")] //TODO: Enable 
        [ProducesResponseType(typeof(SubmitKYCOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SubmitForVerification), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> SubmitForVerification([BusinessProfileId] int businessProfileCode,
            Guid? reviewConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            SubmitKYCCommand command = new SubmitKYCCommand()
            {
                businessProfileCode = businessProfileCode,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                ReviewConcurrencyToken = reviewConcurrencyToken


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
                _logger.LogError($"[SubmitKYC] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Submit the business profile and let Tranglo Compliance to do verification for Admin or Tranglo Business User
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <param name="entityCode"></param>
        /// <param name="reviewConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/business-user-verification")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        //[Authorize(Roles = "Master Admin")] //TODO: Enable 
        [ProducesResponseType(typeof(SubmitBusinessKYCOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        [SwaggerOperation(OperationId = nameof(SubmitBusinessUserKYC), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IActionResult> SubmitBusinessUserKYC([BusinessProfileId] int businessProfileCode, long? adminSolution, string entityCode,
            Guid? reviewConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            SubmitBusinessUserKYCCommand command = new SubmitBusinessUserKYCCommand()
            {
                BusinessProfileCode = businessProfileCode,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                EntityCode = entityCode,
                ReviewConcurrencyToken = reviewConcurrencyToken
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
                _logger.LogError($"[SubmitKYC] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }
        /// <summary>
        /// Retrieve Comments and Review Remarks per BusinessProfileCode and CategoryID
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/document-categories/{categoryId}/review-remarks")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<CommentAndReviewRemarksOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetReviewRemarks), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IEnumerable<CommentAndReviewRemarksOutputDTO>> GetReviewRemarks([BusinessProfileId] int businessProfileCode, int categoryId)
        {
            GetReviewRemarksCommentsByIdQuery query = new GetReviewRemarksCommentsByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                DocumentCategoryCode = categoryId,
                IsExternal = 0,
            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Retrieve Comments and Review Remarks per BusinessProfileCode and CategoryID
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/document-categories/{categoryId}/comments")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<CommentAndReviewRemarksOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetComments), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IEnumerable<CommentAndReviewRemarksOutputDTO>> GetComments([BusinessProfileId] int businessProfileCode, int categoryId)
        {
            GetReviewRemarksCommentsByIdQuery query = new GetReviewRemarksCommentsByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                DocumentCategoryCode = categoryId,
                IsExternal = 1

            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Add Comments per BusinessProfileCode and CategoryID
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="comments"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/document-categories/{categoryId}/comments")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(AddComments), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> AddComments([BusinessProfileId] int businessProfileCode, int categoryId, [FromBody] CommentAndReviewRemarksInputDTO comments, long? adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            SaveCommentsCommand command = _mapper.Map<SaveCommentsCommand>(comments);
            command.BusinessProfileCode = businessProfileCode;
            command.DocumentCategoryCode = categoryId;
            command.LoginId = subjectId.HasValue ? subjectId.Value : null;
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new { DocumentCommentBPCode = result.Value });
            }
            else if (result.IsFailure && result.Error.Trim().ToLower() == "null")
            {
                _logger.LogError($"[AddComments] Add comments failed. Comments not found");
                return this.NotFound();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[AddComments] {result.Error}");
                return this.ValidationProblem();
            }
        }

        /// <summary>
        /// Upload file(s) to Documentation Comments Section
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentcommentBPcode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [RequestSizeLimit(52428800)]
        [HttpPost]
        [Route("{businessProfileCode}/comments/{documentcommentBPcode}/documents")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload Document Files into Documentation Comments", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadCommentDocumentsCommand), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<ActionResult> UploadCommentDocumentsCommand([BusinessProfileId] int businessProfileCode, long documentcommentBPcode, IFormFile uploadedFile, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            UploadCommentDocumentsCommand command = new UploadCommentDocumentsCommand()
            {

                BusinessProfileCode = businessProfileCode,
                documentcommentBPCode = documentcommentBPcode,
                uploadedFile = uploadedFile,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadDocument] {result.Error}");
                return ValidationProblem();
            }

            return Ok();
        }

        /// <summary>
        /// Add Review Remarks per BusinessProfileCode and CategoryID
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="comments"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/document-categories/{categoryId}/review-remarks")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(AddReviewRemarks), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> AddReviewRemarks([BusinessProfileId] int businessProfileCode, int categoryId, [FromBody] CommentAndReviewRemarksInputDTO comments, long? adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            SaveReviewRemarksCommand command = _mapper.Map<SaveReviewRemarksCommand>(comments);
            command.BusinessProfileCode = businessProfileCode;
            command.DocumentCategoryCode = categoryId;
            command.LoginId = subjectId.HasValue ? subjectId.Value : null;
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[AddReviewRemarks] {result.Error}");
                return ValidationProblem();
            }

            return Ok(new { DocumentCommentBPCode = result.Value });

        }


        #region declaration


        /// <summary>
        /// Retrieve Tranglo Connect KYC summary by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{businessProfileCode}/summary")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(List<GetKYCProgressOutputDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCSummary), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCSummary([BusinessProfileId] int businessProfileCode)
        {
            GetKYCSummaryByCodeQuery query = new GetKYCSummaryByCodeQuery()
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetKYCSummary] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieve Tranglo Connect KYC summary for portal by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{businessProfileCode}/connect-user-summary")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(List<GetKYCConnectUserSummaryOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCConnectUserSummary), Tags = new[] { "KYC (Know Your Customer)" })]

        public async Task<IActionResult> GetKYCConnectUserSummary([BusinessProfileId] int businessProfileCode)
        {
            var query = new GetKYCConnectUserSummaryQuery()
            {
                BusinessProfileCode = businessProfileCode
            };

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
        /// Retrieve Tranglo Business KYC summary by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <param name="entityCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{businessProfileCode}/business-user-summary")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(List<GetKYCProgressOutputDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCBusinessUserSummary), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCBusinessUserSummary([BusinessProfileId] int businessProfileCode, [TrangloEntityId] string entityCode, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            GetKYCBusinessUserSummaryByCodeQuery query = new GetKYCBusinessUserSummaryByCodeQuery()
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                EntityCode = entityCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                _logger.LogError($"[{ExtensionHelper.GetMethodName()}]: {result.Error}");
                ModelState.AddModelError("Error", result.Error);
                return ValidationProblem();
            }

            return Ok(result);
        }

        #endregion

        /// <summary>
        /// Upload Document
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="uploadedFile"></param>
        /// <param name="adminSolution"></param>
        /// <param name="fromComment"></param>
        /// <returns></returns>
        [RequestSizeLimit(52428800)]
        [HttpPost("{businessProfileCode}/document-categories/{categoryId}/documents/")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Add authorized document upload", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadDocument), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> UploadDocument([BusinessProfileId] int businessProfileCode, int categoryId, IFormFile uploadedFile, long? adminSolution, bool fromComment)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            SaveDocumentCommand command = new SaveDocumentCommand()
            {
                BusinessProfileCode = businessProfileCode,
                DocumentCategoryCode = categoryId,
                uploadedFile = uploadedFile,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution,
                FromComment = fromComment
        };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadDocument] {result.Error}");
                return ValidationProblem();
            }

            return Ok();

        }

        /// <summary>
        /// Get Internal Document Upload by BusinessProfileCode 
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/internal-documents")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetInternalDocumentUpload), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<ActionResult<List<InternalDocumentUploadOutputDTO>>> GetInternalDocumentUpload([BusinessProfileId] int businessProfileCode)
        {
            GetInternalDocumentUploadQuery query = new GetInternalDocumentUploadQuery()
            {
                BusinessProfileCode = businessProfileCode,
                UserType = User.GetUserType()
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetInternalDocumentUpload] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Upload Internal Document
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/internal-documents")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        //[FileParameter(Description = "Add authorized document upload", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadInternalDocumentCommand), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> UploadInternalDocumentCommand([BusinessProfileId] int businessProfileCode, List<IFormFile> uploadedFile)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            UploadInternalDocumentCommand command = new UploadInternalDocumentCommand()
            {
                BusinessProfileCode = businessProfileCode,
                uploadedFile = uploadedFile,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                UserType = User.GetUserType()


            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadInternalDocument] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Remove Internal Document Upload
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/internal-documents/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(InternalDocumentUploadOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RemoveInternalDocumentUpload), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> RemoveInternalDocumentUpload([BusinessProfileId] int businessProfileCode, Guid documentId)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            RemoveInternalDocumentUploadCommand command = new RemoveInternalDocumentUploadCommand()
            {
                BusinessProfileCode = businessProfileCode,
                DocumentId = documentId,
                LoginId = subjectId.HasValue ? subjectId.Value : null
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[RemoveInternalDocumentUpload] {result.Error}");
                return BadRequest(result);
            }
            return Ok();
        }

        /// <summary>
        /// Verifying Document
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="documentId"></param>
        /// <param name="DocumentInfoInputDTOs"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/document-categories/{categoryId}/documents/{documentId}")]
        [SwaggerOperation(OperationId = nameof(UpdateDocumentInfo), Tags = new[] { "KYC (Know Your Customer)" })]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> UpdateDocumentInfo([BusinessProfileId] int businessProfileCode, int categoryId, Guid documentId, [FromBody] DocumentInfoInputDTO DocumentInfoInputDTOs, long? adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            UpdateDocumentInfoCommand command = _mapper.Map<UpdateDocumentInfoCommand>(DocumentInfoInputDTOs);

            command.BusinessProfileCode = businessProfileCode;
            command.DocumentCategoryCode = categoryId;
            command.documentId = documentId;
            command.LoginId = subjectId.HasValue ? subjectId.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdateDocumentInfo] {result.Error}");
                return BadRequest(result.Error);
            }
        }

        /// <summary>
        /// Select Review Result
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="documentCategoryInfoInputDTOs"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/document-categories/{categoryId}/documents/")]
        [SwaggerOperation(OperationId = nameof(UpdateDocumentCategoryBPStatus), Tags = new[] { "KYC (Know Your Customer)" })]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> UpdateDocumentCategoryBPStatus([BusinessProfileId] int businessProfileCode, int categoryId, [FromBody] DocumentCategoryInfoInputDTO documentCategoryInfoInputDTOs, long? adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            UpdateDocumentCategoriesInfoCommand command = _mapper.Map<UpdateDocumentCategoriesInfoCommand>(documentCategoryInfoInputDTOs);

            command.BusinessProfileCode = businessProfileCode;
            command.DocumentCategoryCode = categoryId;
            command.LoginId = subjectId.HasValue ? subjectId.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdateDocumentCategoryBPStatus] {result.Error}");
                return BadRequest(result.Error);
            }
        }

        /// <summary>
        /// Get Release Document Uploads by BusinessProfileCode 
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/documents-release-history")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetReleaseDocumentHistory), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<ActionResult<List<InternalDocumentUploadOutputDTO>>> GetReleaseDocumentHistory([BusinessProfileId] int businessProfileCode)
        {
            GetReleaseDocumentsUploadHistoryQuery query = new GetReleaseDocumentsUploadHistoryQuery()
            {
                BusinessProfileCode = businessProfileCode,
                UserType = User.GetUserType()
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetReleaseDocumentHistory] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Upload Document Released
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/documents-release")]
        [SwaggerOperation(OperationId = nameof(UploadDocumentReleased), Tags = new[] { "KYC (Know Your Customer)" })]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> UploadDocumentReleased([BusinessProfileId] int businessProfileCode, List<IFormFile> uploadedFile, long? adminSolution)
        {
            SaveDocumentReleasedCommand command = new SaveDocumentReleasedCommand()
            {
                BusinessProfileCode = businessProfileCode,
                uploadedFile = uploadedFile,
                AdminSolution = adminSolution
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadDocumentReleased] {result.Error}");
                return BadRequest(result.Error);
            }
        }

        /// <summary>
        /// Delete Document Upload
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpDelete("{businessProfileCode}/document-categories/{categoryId}/documents/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(DeleteDocument), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> DeleteDocument([BusinessProfileId] int businessProfileCode, int categoryId, Guid documentId, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            DeleteDocumentsUploadByDocIdCommand command = new DeleteDocumentsUploadByDocIdCommand()
            {
                BusinessProfileCode = businessProfileCode,
                DocumentCategoryCode = categoryId,
                DocumentId = documentId,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution
            };

            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[DeleteDocument] {result.Error}");
                return BadRequest(result.Error);
            }
        }

        /// <summary>
        /// Retrieve Document Upload by document id
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/document-categories/{categoryId}/documents/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DocumentsUploadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileResponse]
        [SwaggerOperation(OperationId = nameof(GetDocumentContent), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetDocumentContent([BusinessProfileId] int businessProfileCode, int categoryId, Guid documentId)
        {
            GetDocumentsUploadByDocIdQuery query = new GetDocumentsUploadByDocIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                DocumentCategoryCode = categoryId,
                DocumentId = documentId
            };

            var result = await Mediator.Send(query);
            if (result != null)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return File(result.File, result.ContentType, result.FileName);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Retrieve Document Category List by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/document-categories")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<DocumentsCategoryListOutputDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetDocumentCategories), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IEnumerable<DocumentsCategoryListOutputDto>> GetDocumentCategories([BusinessProfileId] int businessProfileCode, int solutionCode, int trangloEntityCode)
        {
            GetDocumentsCategoriesListQuery query = new GetDocumentsCategoriesListQuery()
            {
                BusinessProfileCode = businessProfileCode,
                SolutionCode = solutionCode,
                TrangloEntityCode = trangloEntityCode
            };

            return await Mediator.Send(query);
        }
        #region Template Management
        /// <summary>
        /// Admin Template Upload
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="questionnaireCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost("document-categories/{categoryId}/templates/")]
        [SwaggerOperation(OperationId = nameof(UploadTemplate), Tags = new[] { "KYC (Know Your Customer)" })]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> UploadTemplate(int categoryId, long? questionnaireCode, IFormFile uploadedFile, long adminSolution)
        {
            SaveTemplateCommand command = new SaveTemplateCommand()
            {
                DocumentCategoryCode = categoryId,
                uploadedFile = uploadedFile,
                QuestionnaireCode = questionnaireCode,
                AdminSolution = adminSolution
            };


            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadTemplate] {result.Error}");
                return BadRequest(result.Error);
            }
        }

        /// <summary>
        /// Retrieve admin templates management
        /// </summary>
        /// <param name="solutionCode"></param>
        /// <returns></returns>
        [HttpGet("document-categories/{solutionCode}/admin-templates/")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<DocumentMetaDataOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAdminTemplatesManagement), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IEnumerable<AdminTemplateOutputDTO>> GetAdminTemplatesManagement(int solutionCode)
        {
            GetAdminTemplateList query = new GetAdminTemplateList 
            {
                SolutionCode = solutionCode
            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Save Admin Template Management
        /// </summary>
        /// <param name="questionnaireCode"></param>
        /// <param name="solutionCode"></param>
        /// <param name="trangloEntityCode"></param>
        /// <returns></returns>
        [HttpPost("document-categories/templatemanagements/")]
        [SwaggerOperation(OperationId = nameof(SaveTemplateManagement), Tags = new[] { "KYC (Know Your Customer)" })]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> SaveTemplateManagement(List<AdminTemplateManagementInputDTO> adminTemplateManagementInputDTO)
        {
            SaveTemplateManagementCommand command = new SaveTemplateManagementCommand()
            {
                InputDTO = adminTemplateManagementInputDTO
            };


            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadTemplate] {result.Error}");
                return BadRequest(result.Error);
            }
        }

        #endregion

        /// <summary>
        /// Retrieve Admin template by document id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("document-categories/{categoryId}/templates/{documentId}")]
        [FileResponse]
        [SwaggerOperation(OperationId = nameof(GetTemplate), Tags = new[] { "KYC (Know Your Customer)" })]
        [ProducesResponseType(typeof(DocumentsUploadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> GetTemplate(int categoryId, Guid documentId)
        {
            GetDocumentsUploadByDocIdQuery query = new GetDocumentsUploadByDocIdQuery
            {
                DocumentCategoryCode = categoryId,
                DocumentId = documentId
            };

            var result = await Mediator.Send(query);
            if (result != null)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return File(result.File, result.ContentType, result.FileName);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Retrieve Document MetaData by business profile code and category code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/document-categories/{categoryId}/documents-metadata/")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<DocumentMetaDataOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetDocumentsMetaData), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IEnumerable<DocumentMetaDataOutputDTO>> GetDocumentsMetaData([BusinessProfileId] int businessProfileCode, int categoryId)
        {
            GetDocumentMetaData query = new GetDocumentMetaData
            {
                BusinessProfileCode = businessProfileCode,
                DocumentCategoryCode = categoryId,
            };
            var result = await Mediator.Send(query);

            return result;
        }


        /// <summary>
        /// Retrieve KYC Submission Status per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/submission-status")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetKYCSubmissionStatusOutputDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCSubmissionStatus), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCSubmissionStatus([BusinessProfileId] int businessProfileCode, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            GetKYCSubmissionStatusByIdQuery query = new GetKYCSubmissionStatusByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
            };

            var result = await Mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[KYCSubmissionStatus] {result.Error}");
                return NotFound();
            }
        }

        /// <summary>
        /// Search Business Profiles based on certain Criteria
        /// </summary>
        /// <param name="PartnerNameFilter"></param>
        /// <param name="CountryISO2Filter"></param>
        /// <param name="ComplianceOfficerAssignedFilter"></param>
        /// <param name="WorkflowStatusCodeFilter"></param>
        /// <param name="KYCStatusCodeFilter"></param>
        /// <param name="RegistrationFromFilter"></param>
        /// <param name="RegistrationToFilter"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortExpression"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortDirection"></param>
        /// <param name="entityCode"></param>
        /// <param name="solutionCode"></param>
        /// <param name="fullRegisteredCompanyLegalName"></param>
        /// <param name="workflowStatusCode"></param>
        /// <param name="isComplianceTeamReview"></param>
        /// <returns></returns>
        [HttpGet("kyc-management")]
        [ProducesResponseType(typeof(PagedResult<KYCManagementOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCManagement), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<PagedResult<KYCManagementOutputDTO>> GetKYCManagement(
              string PartnerNameFilter, string CountryISO2Filter,
             string ComplianceOfficerAssignedFilter, long KYCStatusCodeFilter,
             string RegistrationFromFilter, string RegistrationToFilter, int pageSize, int pageIndex,
              string sortExpression, SortDirection sortDirection, [TrangloEntityId] string entityCode, long solutionCode, string fullRegisteredCompanyLegalName,
              long? workflowStatusCode,long? customerTypeCode,int isComplianceTeamReview
             )
        {
            GetKYCManagementQuery query = new GetKYCManagementQuery
            {
                PartnerName = PartnerNameFilter,
                CountryISO2 = CountryISO2Filter,
                ComplianceOfficerAssignedLoginId = ComplianceOfficerAssignedFilter,
                KYCStatusCode = KYCStatusCodeFilter,
                fromRegistrationDate = RegistrationFromFilter,
                toRegistrationDate = RegistrationToFilter,
                EntityCode = entityCode,
                SolutionCode = solutionCode,
                FullRegisteredCompanyLegalName = fullRegisteredCompanyLegalName,
                WorkFlowStatusCode = workflowStatusCode,
                CustomerTypeCode = customerTypeCode,
                IsComplianceTeamReview = isComplianceTeamReview,
                PagingOptions = new PagingOptions 
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortExpression = sortExpression,
                    Direction = sortDirection,
                }
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Search KYCApprovalDetails
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="solutionCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/kyc-approval-details")]
        [ProducesResponseType(typeof(KYCApprovalDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCApprovalDetails), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCApprovalDetails([BusinessProfileId] int businessProfileCode, long? solutionCode)
        {
            GetKYCApprovalDetailsQuery query = new GetKYCApprovalDetailsQuery
            {
                BusinessProfileCode = businessProfileCode,
                SolutionCode = solutionCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[KYCApprovalDetails] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Search KYCCategoriesStatuses
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="solutionCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/kyc-categories-statuses")]
        [ProducesResponseType(typeof(IEnumerable<KYCCategoriesStatusesOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCCategoriesStatus), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IEnumerable<KYCCategoriesStatusesOutputDTO>> GetKYCCategoriesStatus([BusinessProfileId] int businessProfileCode, long solutionCode, string entityCode)
        {
            GetKYCCategoriesStatusesQuery query = new GetKYCCategoriesStatusesQuery
            {
                BusinessProfileCode = businessProfileCode,
                SolutionCode = solutionCode,
                EntityCode = entityCode
            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Get KYC Review Result per category
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="KYCCategoryCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/kyc-review-result/{KYCCategoryCode}")]
        [ProducesResponseType(typeof(KYCReviewResultOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCReviewResults), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCReviewResults([BusinessProfileId] int businessProfileCode, int KYCCategoryCode)
        {
            GetKYCReviewResultQuery query = new GetKYCReviewResultQuery
            {
                BusinessProfileCode = businessProfileCode,
                KYCCategoryCode = KYCCategoryCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetKYCReviewResultQuery] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Update User KYC Notification
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/kyc-user-notification")]
        [ProducesResponseType(typeof(BusinessProfile), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(UpdateKYCUserNotification), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> UpdateKYCUserNotification([BusinessProfileId] int businessProfileCode, [FromBody] KYCStatusInputDTO kycStatusInputDTO, int adminSolution)
        {
            NotifyKYCUserCommand command = new NotifyKYCUserCommand
            {
                BusinessProfileCode = businessProfileCode,
                KYCStatusCode = kycStatusInputDTO.KYCStatusCode,
                AdminSolution = adminSolution
            };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[NotifyKYCUser] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result);
        }
        

        #region KYC Approval Requisition
        /// <summary>
        /// Get KYC Approval Requisition By Requisition Code
        /// </summary>
        /// <param name="requisitionCode"></param>
        /// <returns></returns>
        [HttpGet("kyc-approval-requisitions/{requisitionCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(KYCApprovalRequisitionOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCApprovalRequisition), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCApprovalRequisition(string requisitionCode)
        {
            GetKYCApprovalRequisitionQuery query = new GetKYCApprovalRequisitionQuery
            {
                RequisitionCode = requisitionCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetKYCApprovalRequisition] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }
        /// <summary>
        /// Get Pending KYC Requsition by Business Profile Code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/kyc-pending-requisition")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(PagedResult<PartnerKYCStatusRequisitionListingOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetBusinessProfilePendingKYCRequisition), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetBusinessProfilePendingKYCRequisition([BusinessProfileId] int businessProfileCode, int adminSolution)
        {
            GetBusinessProfilePendingKYCRequisitionQuery query = new GetBusinessProfilePendingKYCRequisitionQuery
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution
            };
            var result = await Mediator.Send(query);

            return Ok(result.Value);
        }
        /// <summary>
        /// Get KYC Approval Requisition Listing
        /// </summary>
        /// <returns></returns>
        [HttpGet("kyc-approval-requisitions")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(PagedResult<PartnerKYCStatusRequisitionListingOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetKYCApprovalRequisitionList), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> GetKYCApprovalRequisitionList([FromQuery] PartnerKYCStatusRequisitionListingInputDTO inputDTO, 
        int pageIndex, int pageSize, string sortExpression, SortDirection sortDirection, [TrangloEntityId] string entityCode, int? isComplianceApproval
         ,int? adminSolution, long? collectionTierCode, int? customerTypeCode)
        {
            GetKYCApprovalRequisitionListQuery query = new GetKYCApprovalRequisitionListQuery
            {
                InputDTO = inputDTO,
                PagingOptions = new PagingOptions 
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortExpression = sortExpression,    
                    Direction = sortDirection
                },
                EntityCode = entityCode,
                IsComplianceApproval = isComplianceApproval,
                AdminSolution = adminSolution,
                CollectionTierCode = collectionTierCode,
                CustomerTypeCode = customerTypeCode
            };

            var result = await Mediator.Send(query);

            return Ok(result);

        }

      

        /// <summary>
        /// Approve KYC Requisition Approval by Requisition Code
        /// </summary>
        /// <param name="roleCode"></param>
        /// <param name="entityCode"></param>
        /// <param name="requisitionApproveInputDTO"></param>
        /// <returns></returns>
        [HttpPut("kyc-requisition-approval/entity/{entityCode}/role/{roleCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(ApproveKYCRequisition), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> ApproveKYCRequisition([RoleCode] string roleCode, [TrangloEntityId] string entityCode, KYCRequisitionApproveInputDTO requisitionApproveInputDTO, int adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            ApproveKYCRequisitionCommand command = new ApproveKYCRequisitionCommand
            {
                KYCRequisitionApproveInputDTO = requisitionApproveInputDTO,
                UserId = User.GetUserId().Value,
                EntityCode = entityCode,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                AdminSolution = adminSolution
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[ApproveKYCRequisition] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
            
        }

        /// <summary>
        /// Reject KYC Requisition Approval by Requisition Code
        /// </summary>
        /// <param name="roleCode"></param>
        /// <param name="requisitionRejectInputDTO"></param>
        /// <returns></returns>
        [HttpPut("kyc-requisition-rejection/entity/{entityCode}/role/{roleCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RejectKYCRequisition), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> RejectKYCRequisition([RoleCode] string roleCode, [TrangloEntityId] string entityCode, KYCRequisitionRejectInputDTO requisitionRejectInputDTO)
        {
            RejectKYCRequisitionCommand command = new RejectKYCRequisitionCommand
            {
                KYCRequisitionRejectInputDTO = requisitionRejectInputDTO,
                UserId = User.GetUserId().Value,
                EntityCode = entityCode
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[RejectKYCRequisition] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Request OTP
        /// </summary>
        /// <param name="requisitionCode"></param>
        /// <returns></returns>
        [HttpPost("request-kyc-requisition-otp/{requisitionCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RequestKycRequisitionOTPCode), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> RequestKycRequisitionOTPCode(string requisitionCode)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            RequestKYCApprovalOTPCommand command = new RequestKYCApprovalOTPCommand
            {
                RequisitionCode = requisitionCode,
                LoginId = subjectId.HasValue ? subjectId.Value : null
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[RejectKYCRequisition] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
            
        }
        #endregion


        /// <summary>
        /// Change Customer Type Handling
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="inputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/change-customer-type-handling")]
        [ProducesResponseType(typeof(BusinessProfile), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(ChangeCustomerTypeHandling), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<IActionResult> ChangeCustomerTypeHandling([BusinessProfileId] int businessProfileCode, [FromBody] ChangeCustomerTypeInputDTO inputDTO, long adminSolution)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            ChangeCustomerTypeCommand command = new ChangeCustomerTypeCommand
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution,
                InputDTO = inputDTO,
                LoginId = subjectId.HasValue ? subjectId.Value : null
            };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[ChangeCustomerTypeHandling] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get Business Onboarding Status
        /// </summary>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/business-onboarding-status")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(BusinessOnboardingStatusOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetBusinessOnboardingKYCStatus), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<ActionResult<BusinessOnboardingStatusOutputDTO>> GetBusinessOnboardingKYCStatus([BusinessProfileId] int businessProfileCode)
        {
            GetBusinessOnboardingStatusQuery query = new GetBusinessOnboardingStatusQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[BusinessOnboardingSummary] {result.Error}");
                return ValidationProblem(result.Error);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get Connect Onboarding Status
        /// </summary>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/connect-onboarding-status")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ConnectOnboardingStatusOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetConnectOnboardingKYCStatus), Tags = new[] { "KYC (Know Your Customer)" })]

        public async Task<ActionResult<ConnectOnboardingStatusOutputDTO>> GetConnectOnboardingKYCStatus([BusinessProfileId] int businessProfileCode)
        {
            GetConnectOnboardingStatusQuery query = new GetConnectOnboardingStatusQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[BusinessOnboardingSummary] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }


        /// <summary>
        /// Retrieve RBA Results per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/rba-results")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(List<RBAResultsOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetReviewRemarks), Tags = new[] { "KYC (Know Your Customer)" })]
        
        public async Task<List<RBAResultsOutputDTO>> GetRBAResults([BusinessProfileId] int businessProfileCode)
        {
            GetRBAScreeningResultQuery query = new GetRBAScreeningResultQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Default Template Upload - To upload templates not included in Admin - Template Management
        /// </summary>
        /// <param name="defaultTemplateCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost("default-template/{defaultTemplateCode}")]
        [SwaggerOperation(OperationId = nameof(UploadDefaultTemplate), Tags = new[] { "KYC (Know Your Customer)" })]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> UploadDefaultTemplate(int defaultTemplateCode, IFormFile uploadedFile)
        {
            SaveDefaultTemplateCommand command = new SaveDefaultTemplateCommand()
            {
                DefaultTemplateCode = defaultTemplateCode,
                uploadedFile = uploadedFile,
            };

            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadDefaultTemplate] {result.Error}");
                return BadRequest(result.Error);
            }
        }
        #region 
        /// <summary>
        /// Retrieve RBA Approval Results per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/rba-approval-results")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<RBAApprovalResultsOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetRBAApprovalResults), Tags = new[] { "KYC (Know Your Customer)" })]

        public async Task<IEnumerable<RBAApprovalResultsOutputDTO>> GetRBAApprovalResults([BusinessProfileId] int businessProfileCode, [TrangloEntityId] string entity)
        {
            GetRBAApprovalResultsQuery query = new GetRBAApprovalResultsQuery
            {
                BusinessProfileCode = businessProfileCode,
                TrangloEntityCode = string.IsNullOrEmpty(entity) ? entity : entity.ToUpper()
            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Update Compliance Internal Risk Rating
        /// </summary>
        /// <param name="updateRBAComplianceRatingInputDTO"></param>
        /// <returns></returns>
        [HttpPut("rba-compliance-internal-risk/{rbaCode}")]
        [ProducesResponseType(typeof(UpdateRBAComplianceRatingOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdateComplianceInternalRisk), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IActionResult> UpdateComplianceInternalRisk(int rbaCode,UpdateRBAComplianceRatingInputDTO updateRBAComplianceRatingInputDTO)
        {
            UpdateRBAComplianceInternalRiskCommand command = new UpdateRBAComplianceInternalRiskCommand
            {
                inputDTO = updateRBAComplianceRatingInputDTO,
                RbaCode = rbaCode
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdateComplianceInternalRiskRating] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result);
        }

        /// <summary>
        /// View Update Compliance Internal Risk By Requisition Code
        /// </summary>
        /// <param name="requisitionCode"></param>
        /// <returns></returns>
        [HttpGet("rba-compliance-internal-risk/{requisitionCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(RBAComplianceInternalRIskRequisitionOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetRBAComplianceInternalRiskRequisition), Tags = new[] { "KYC (Know Your Customer)" })]

        public async Task<IActionResult> GetRBAComplianceInternalRiskRequisition(string requisitionCode)
        {
            GetRBAComplianceInternalRiskRequisitionQuery query = new GetRBAComplianceInternalRiskRequisitionQuery
            {
                RequisitionCode = requisitionCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetRBAComplianceInternalRiskRequisition] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Edit Update Compliance Internal Risk Rating
        /// </summary>
        /// <param name="requisitionCode"></param>
        /// <param name="inputDTO"></param>
        /// <returns></returns>
        [HttpPut("rba-compliance-internal-risk/{requisitionCode}/edit-update-compliance-internal-risk")]
        [ProducesResponseType(typeof(RequisitionEditResult<RBARequisition>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(EditUpdateComplianceInternalRiskRequisition), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IActionResult> EditUpdateComplianceInternalRiskRequisition(string requisitionCode, EditUpdateComplianceInternalRiskInputDTO inputDTO)
        {
            EditUpdateComplianceInternalRiskRequisitionCommand query = new EditUpdateComplianceInternalRiskRequisitionCommand()
            {
                RequisitionCode = requisitionCode,
                InputDTO = inputDTO
            };

            var result = await Mediator.Send(query);
            if (!result.IsSuccess)
            {
                _logger.LogError($"[EditUpdateComplianceInternalRiskRequisition] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result);
        }


        /// <summary>
        /// Approve Compliance Internal Risk 
        /// </summary>
        /// <param name="roleCode"></param>
        /// <param name="approveRBARequisitionInputDTO"></param>
        /// <returns></returns>
        [HttpPut("rba-compliance-internal-risk-requisition/role/{roleCode}/approve")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(ApproveRBARequisition), Tags = new[] { "KYC (Know Your Customer)" })]

        public async Task<IActionResult> ApproveRBARequisition([RoleCode] string roleCode,ApproveRBARequisitionInputDTO approveRBARequisitionInputDTO)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            ApproveRBARequisitionCommand command = new ApproveRBARequisitionCommand
            {
                ApproveRBARequisitionInputDTO = approveRBARequisitionInputDTO,
                UserId = User.GetUserId().Value,
                LoginId = subjectId.HasValue ? subjectId.Value : null,

            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[ApproveRBARequisition] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }
        /// 

        /// <summary>
        /// Reject Compliance Internal Risk 
        /// </summary>
        /// <param name="roleCode"></param>
        /// <param name="rejectRBARequisitionInputDTO"></param>
        /// <returns></returns>
        [HttpPut("rba-compliance-internal-risk-requisition/role/{roleCode}/reject")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RejectBARequisition), Tags = new[] { "KYC (Know Your Customer)" })]

        public async Task<IActionResult> RejectBARequisition([RoleCode] string roleCode, RejectRBARequisitionInputDTO rejectRBARequisitionInputDTO)
        {
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);
            RejectRBARequisitionCommand command = new RejectRBARequisitionCommand
            {
                RejectRBARequisitionInputDTO = rejectRBARequisitionInputDTO,
                UserId = User.GetUserId().Value,
                LoginId = subjectId.HasValue ? subjectId.Value : null,

            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[RejectBARequisition] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// RBA Approval Listing
        /// </summary>
        [HttpGet("rba-compliance-internal-risk-requisition/approval-listing")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(PagedResult<GetRBARequisitionListingOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetRBAApprovalListing), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IActionResult> GetRBAApprovalListing([FromQuery] GetRBARequisitionListingInputDTO inputDTO,
        int pageIndex, int pageSize, string sortExpression, SortDirection sortDirection, [TrangloEntityId] string entity
         )
        {
            GetRBARequisitionListingQuery command = new GetRBARequisitionListingQuery
            {
                InputDTO = inputDTO,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortExpression = sortExpression,
                Direction = sortDirection,
                TrangloEntityCode = string.IsNullOrEmpty(entity) ? entity : entity.ToUpper()
            };

            var result = await Mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Request RBA OTP
        /// </summary>
        /// <param name="requisitionCodes"></param>
        /// <returns></returns>
        [HttpPost("request-rba-requisition-otp")]
        [ProducesResponseType(typeof(Result<RBARequisitionOTPOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RequestOTPCommand), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IActionResult> RequestOTPCommand(List<RBARequisitionOTPInputDTO> requisitionCodes)
        {
            RequestRBAOTPCommand command = new RequestRBAOTPCommand
            {
                RequisitionCodes = requisitionCodes,
                UserId = User.GetUserId().Value
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[RequestRBAOTPCommand] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result);
        }
        /// <summary>
        /// Get Compliance Internal Risk Edit By Users
        /// </summary>
        [HttpGet("rba-compliance-internal-risk-requisition/edited-by-users")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<GetComplianceInternalRiskRequisitionEditedByUserOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetRBAComplianceInternalRiskRequisitionEditedByUsers), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IEnumerable> GetRBAComplianceInternalRiskRequisitionEditedByUsers(long complianceSettingTypeCode)
        {
            GetComplianceInternalRiskRequisitionEditedByUsersQuery query = new GetComplianceInternalRiskRequisitionEditedByUsersQuery
            {
                ComplianceSettingTypeCode = complianceSettingTypeCode
            };

            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Get Compliance Internal Risk Requisition Approved By Users
        /// </summary>
        [HttpGet("rba-compliance-internal-risk-requisition/approved-by-users")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<GetComplianceInternalRiskRequisitionApproveByUserOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetComplianceInternalRiskRequisitionApproveByUsers), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IEnumerable> GetComplianceInternalRiskRequisitionApproveByUsers(long complianceSettingTypeCode)
        {
            GetComplianceInternalRiskRequisitionApproveByUsersQuery query = new GetComplianceInternalRiskRequisitionApproveByUsersQuery
            {
                ComplianceSettingTypeCode = complianceSettingTypeCode
            };
   
            var result = await Mediator.Send(query);

            return result;
        }

        /// <summary>
        /// Get Compliance Internal Risk Requisition Requested By Users
        /// </summary>
        [HttpGet("rba-compliance-internal-risk-requisition/requested-by-users")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<GetComplianceInternalRiskRequisitionRequestedByUserOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetComplianceRequisitionRequestedByUsers), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IEnumerable> GetComplianceRequisitionRequestedByUsers(long complianceSettingTypeCode)
        {
            GetComplianceRequisitionRequestedByUsersQuery query = new GetComplianceRequisitionRequestedByUsersQuery
            {
                ComplianceSettingTypeCode = complianceSettingTypeCode
            };

            var result = await Mediator.Send(query);

            return result;
        }
        #endregion
    }
}