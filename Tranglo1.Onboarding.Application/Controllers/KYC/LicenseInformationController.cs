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
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.LicenseInformation;
using Tranglo1.Onboarding.Application.DTO.LicenseInformation;
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
    public class LicenseInformationController : ControllerBase
    {
        private readonly ILogger<LicenseInformationController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public LicenseInformationController(ILogger<LicenseInformationController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve License Information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/license-information")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(LicenseInformationOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetLicenseInformationById), Tags = new[] { "KYC (Know Your Customer) - License Information" })]
        public async Task<IActionResult> GetLicenseInformationById([BusinessProfileId] int businessProfileCode)
        {
            var command = new GetLicenseInformationByIdQuery { BusinessProfileCode = businessProfileCode };
            var licenseInformation = await Mediator.Send(command);

            if (licenseInformation.Value != null)
            {
                return Ok(licenseInformation);
            }

            else
            {
                return Ok(licenseInformation);
            }
        }




        /// <summary>
        /// Add License Information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="licenseinformation"></param>
        /// <param name="licenseInfoConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/license-information")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(CreateLicenseInformation), Tags = new[] { "KYC (Know Your Customer) - License Information" })]
        
        public async Task<IActionResult> CreateLicenseInformation([BusinessProfileId] int businessProfileCode, [FromBody] LicenseInformationInputDTO licenseinformation, Guid? licenseInfoConcurrencyToken)
        {
            SaveLicenseInformationCommand command = _mapper.Map<SaveLicenseInformationCommand>(licenseinformation);
            command.BusinessProfileCode = businessProfileCode;
            command.LoginId = User.GetSubjectId();

            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.LicenseInfoConcurrencyToken = licenseInfoConcurrencyToken;

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
                _logger.LogError($"[SaveLicenseInfo] {result.Error}");
                return ValidationProblem();
            }

            return Ok(new { licenseInformationCode = result.Value });
        }

        /// <summary>
        /// Update License Information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="licenseinformation"></param>
        /// <param name="adminSolution"></param>
        /// <param name="licenseInfoConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/license-information")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(Result<LicenseInformation>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdateLicenseInformation), Tags = new[] { "KYC (Know Your Customer) - License Information" })]
        
        public async Task<IActionResult> UpdateLicenseInformation([BusinessProfileId] int businessProfileCode, [FromBody] LicenseInformationInputDTO licenseinformation, long? adminSolution, Guid? licenseInfoConcurrencyToken, bool fromComment)
        {
            UpdateLicenseInformationCommand command = _mapper.Map<UpdateLicenseInformationCommand>(licenseinformation);
            command.BusinessProfileCode = businessProfileCode;
            command.LoginId = User.GetSubjectId();

            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;
            command.LicenseInfoConcurrencyToken = licenseInfoConcurrencyToken;
            command.FromComment = fromComment;

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
                _logger.LogError($"[UpdateLicenseInfo] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result);
        }

        /// <summary>
        /// Regulator Letter Document Upload
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [RequestSizeLimit(52428800)]
        [HttpPost("{businessProfileCode}/license-information/document-upload")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(UploadRegulatorLetterDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UploadRegulatorLetterDocument), Tags = new[] { "KYC (Know Your Customer) - License Information" })]
        
        public async Task<IActionResult> UploadRegulatorLetterDocument(int businessProfileCode, IFormFile uploadedFile)
        {
            UploadRegulatorLetterDocumentCommand command = new UploadRegulatorLetterDocumentCommand
            {
                BusinessProfileCode = businessProfileCode,
                UploadedFile = uploadedFile
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadRegulatorLetterDocument] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get Regulator Letter Document download
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/license-information/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DownloadRegulatorLetterDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetRegulatorLetterDocument), Tags = new[] { "KYC (Know Your Customer) - License Information" })]
        public async Task<ActionResult> GetRegulatorLetterDocument([BusinessProfileId] int businessProfileCode, Guid documentId)
        {
            GetRegulatorLetterDocumentQuery query = new GetRegulatorLetterDocumentQuery()
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