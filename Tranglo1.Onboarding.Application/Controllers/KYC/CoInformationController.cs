using AutoMapper;
using CSharpFunctionalExtensions;
using IdentityServer4.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
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
    public class CoInformationContoller : ControllerBase
    {
        private readonly ILogger<CoInformationContoller> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public CoInformationContoller(ILogger<CoInformationContoller> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve CO Information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/compliance-officers")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ComplianceOfficersOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetComplianceOfficersById), Tags = new[] { "KYC (Know Your Customer) - Compliance Officer Information" })]
        public async Task<IActionResult> GetComplianceOfficersById([BusinessProfileId] int businessProfileCode)
        {
            GetCoInformationByIdQuery query = new GetCoInformationByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetComplianceOfficers] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Add CO Information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="coInformation"></param>
        /// /// <param name="adminSolution"></param>
        /// <param name="coInformationConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/compliance-officers")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(CreateComplianceOfficer), Tags = new[] { "KYC (Know Your Customer) - Compliance Officer Information" })]
        public async Task<IActionResult> CreateComplianceOfficer([BusinessProfileId] int businessProfileCode, [FromBody] ComplianceOfficersInputDTO coInformation, long? adminSolution, Guid? coInformationConcurrencyToken)
        {
            SaveCoInformationCommand command = _mapper.Map<SaveCoInformationCommand>(coInformation);
            command.BusinessProfileCode = businessProfileCode;
            command.LoginId = User.GetSubjectId();

            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); 
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;
            command.COInformationConcurrencyToken = coInformationConcurrencyToken;

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
                _logger.LogError($"[SaveComplianceOfficers] {result.Error}");
                return ValidationProblem();
            }

            return Ok(new { COInformationCode = result.Value });
        }

        /// <summary>
        /// Update CO Information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="coInformation"></param>
        ///  /// <param name="adminSolution"></param>
        /// <param name="coInformationConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/compliance-officers")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(Result<COInformation>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdateComplianceOfficer), Tags = new[] { "KYC (Know Your Customer) - Compliance Officer Information" })]
        public async Task<IActionResult> UpdateComplianceOfficer([BusinessProfileId] int businessProfileCode, [FromBody] ComplianceOfficersInputDTO coInformation, long? adminSolution, Guid? coInformationConcurrencyToken)
        {
            UpdateCoInformationCommand command = _mapper.Map<UpdateCoInformationCommand>(coInformation);
            command.BusinessProfileCode = businessProfileCode;
            command.LoginId = User.GetSubjectId();

            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); 
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;
            command.COInformationConcurrencyToken = coInformationConcurrencyToken;

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
                _logger.LogError($"[UpdateComplianceOfficers] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result);
        }

        /// <summary>
        /// Upload CO Signature Document
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="uploadedFile"></param>
        /// /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/compliance-officers/signature")]
        //later need change back to external only
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload Compliance Officer Signature Document", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadCOSignature), Tags = new[] { "KYC (Know Your Customer) - Compliance Officer Information" })]
        public async Task<IActionResult> UploadCOSignature([BusinessProfileId] int businessProfileCode, IFormFile uploadedFile, long? adminSolution)
        {
            try
            {
                SaveCOSignatureCommand command = new SaveCOSignatureCommand()
                {
                    BusinessProfileCode = businessProfileCode,
                    uploadedFile = uploadedFile,
                    LoginId = User.GetSubjectId()
                };

                var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);
                command.CustomerSolution = solution.HasValue ? solution.Value : null;
                command.AdminSolution = adminSolution;

                var result = await Mediator.Send(command);

                if (result.IsFailure)
                {
                    ModelState.AddModelError("Error", result.Error);
                    _logger.LogError($"[UploadCOSignature] {result.Error}");
                    return ValidationProblem();
                }

                return Ok(new { DocumentId = result.Value });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Delete CO Signature Document
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentId"></param>
        /// /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpDelete("{businessProfileCode}/compliance-officers/signature/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(DeleteCOSignature), Tags = new[] { "KYC (Know Your Customer) - Compliance Officer Information" })]
        public async Task<IActionResult> DeleteCOSignature([BusinessProfileId] int businessProfileCode, Guid documentId, long? adminSolution)
        {
            DeleteCOSignatureCommand command = new DeleteCOSignatureCommand()
            {
                BusinessProfileCode = businessProfileCode,
                CoSignatureDocumentId = documentId,
                LoginId = User.GetSubjectId()
            };

            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[DeleteCOSignature] {result.Error}");
                return ValidationProblem();
            }

            return Ok();

        }

        /// <summary>
        /// Get CO Signature Document download
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/compliance-officers/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(COSignatureOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCOSignature), Tags = new[] { "KYC (Know Your Customer) - Compliance Officer Information" })]
        public async Task<ActionResult> GetCOSignature([BusinessProfileId] int businessProfileCode, Guid documentId)
        {
            GetCOSignatureQuery query = new GetCOSignatureQuery()
            {
                BusinessProfileCode = businessProfileCode,
                DocumentId = documentId,
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
    }
}
