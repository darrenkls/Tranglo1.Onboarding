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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
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
    public class CustomerVerificationController : ControllerBase
    {
        private readonly ILogger<CustomerVerificationController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public CustomerVerificationController(ILogger<CustomerVerificationController> logger,IMediator mediator,IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Add or Update Customer Verification per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        ///  /// <param name="inputDTO"></param>
        ///   /// <param name="adminSolution"></param>
        /// <param name="customerVerificationConcurrencyToken"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [HttpPost("{businessProfileCode}/customer-verifications")]
        [ProducesResponseType(typeof(CustomerVerificationOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveCustomerVerification), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<IActionResult> SaveCustomerVerification([BusinessProfileId] int businessProfileCode, [FromBody] CustomerVerificationInputDTO inputDTO, long? adminSolution, Guid? customerVerificationConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User); // convert Maybe<string> to string

            SaveCustomerVerificationCommand command = new SaveCustomerVerificationCommand
            {
                BusinessProfileCode = businessProfileCode,
                InputDTO = inputDTO,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                CustomerVerificationConcurrencyToken = customerVerificationConcurrencyToken
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
                _logger.LogError($"[SaveCustomerVerificationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }


        /// <summary>
        /// Add Customer Verification Documents
        /// </summary>
        /// <param name="customerVerificationCode"></param>
        /// <param name="verificationIDTypeSectionCode"></param>
        /// <param name="adminSolution"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [HttpPost("customer-verifications/document-uploads")]
        [ProducesResponseType(typeof(CustomerVerificationDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload Document Files into Customer Verification Documents", Required = false)]
        [SwaggerOperation(OperationId = nameof(SaveCustomerVerificationDocument), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> SaveCustomerVerificationDocument(long? customerVerificationCode,long? adminSolution, long? verificationIDTypeSectionCode,IFormFile uploadedFile)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SaveCustomerVerificationDocumentCommand command = new SaveCustomerVerificationDocumentCommand()
            {
                CustomerVerificationCode = customerVerificationCode,
                VerificationIDTypeSectionCode = verificationIDTypeSectionCode,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution,
                UploadedFile = uploadedFile
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SaveVerificationDocumentCommand] {result.Error}");
                return BadRequest(result);
            }


            return Ok(result.Value);
        }


        /// <summary>
        /// Add Customer Verification Watermark Documents
        /// </summary>
        /// <param name="customerVerificationDocumentCode"></param>
        /// <param name="adminSolution"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [HttpPost("customer-verifications/document-watermark-uploads")]
        [ProducesResponseType(typeof(CustomerVerificationWatermarkDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload Watermark Document Files into Customer Verification Documents", Required = false)]
        [SwaggerOperation(OperationId = nameof(SaveCustomerVerificationWatermarkDocument), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> SaveCustomerVerificationWatermarkDocument(long? customerVerificationDocumentCode, long? adminSolution,IFormFile uploadedFile)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SaveCustomerVerificationWatermarkDocumentCommand command = new SaveCustomerVerificationWatermarkDocumentCommand()
            {
                CustomerVerificationDocumentCode = customerVerificationDocumentCode,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution,
                UploadedFile = uploadedFile
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SaveVerificationDocumentCommand] {result.Error}");
                return BadRequest(result);
            }


            return Ok(result.Value);
        }



        /// <summary>
        /// Get Customer Verification by Business Profile Code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        ///  <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetCustomerVerificationOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerification), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        [HttpGet("customer-verifications")]
        public async Task<IActionResult> GetCustomerVerification(long? businessProfileCode)
        {
            //Change to Business Profile Code
            GetCustomerVerificationByBusinessProfileCodeQuery query = new GetCustomerVerificationByBusinessProfileCodeQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete  Customer Verification Document 
        /// </summary>
        /// <param name="customerVerificationDocumentCode"></param>
        /// <param name="adminSolution"></param>
        ///  <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(CustomerVerificationDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(DeleteCustomerVerificationDocument), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        [HttpDelete("customer-verifications/documents/{customerVerificationDocumentCode}")]
        public async Task<IActionResult> DeleteCustomerVerificationDocument(long? customerVerificationDocumentCode, long? adminSolution)
        {
            DeleteCustomerVerificationDocumentCommand command = new DeleteCustomerVerificationDocumentCommand
            {
                CustomerVerificationDocumentCode = customerVerificationDocumentCode,
                AdminSolution = adminSolution

            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            _logger.LogError($"Failed to delete Customer Verification Document with Customer Verification Document Code: {customerVerificationDocumentCode}");

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Get Customer Verification Document Details
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("customer-verifications/document-details/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetCustomerVerificationDocumentDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerificationDocumentDetails), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> GetCustomerVerificationDocumentDetails(Guid? documentId, int? businessProfileCode)
        {
            GetCustomerVerificationDocumentDetailsQuery query = new GetCustomerVerificationDocumentDetailsQuery()
            {
                DocumentId = documentId,
                BusinessProfileCode = businessProfileCode
            };

            var results = await Mediator.Send(query);
            if (results != null && results.Any())
            {
                if (results.Count == 1) //Return file
                {
                    var result = results.FirstOrDefault();
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    return File(result.File, result.ContentType, result.FileName);
                }
                else if (results.Count > 1) //Return zip file
                {
                    var archiveStream = new MemoryStream();
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var result in results)
                        {
                            var entry = archive.CreateEntry(result.FileName);
                            using (var entryStream = entry.Open())
                            {
                                await result.File.CopyToAsync(entryStream);
                            }
                        }
                    }

                    archiveStream.Position = 0;
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    return File(archiveStream, "application/zip", "Face-To-Face Verification Forms.zip");
                }
                else
                    return null;
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Get Customer Verification Watermark Document Details
        /// </summary>
        /// <param name="customerVerificationDocumentCode"></param>
        /// <returns></returns>
        [HttpGet("customer-verifications/{customerVerificationDocumentCode}/document-details")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetCustomerVerificationDocumentDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerificationWatermarkDocumentDetails), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> GetCustomerVerificationWatermarkDocumentDetails(long? customerVerificationDocumentCode)
        {
            GetCustomerVerificationWatermarkDocumentDetailsQuery query = new GetCustomerVerificationWatermarkDocumentDetailsQuery()
            {
                CustomerVerificationDocumentCode = customerVerificationDocumentCode,
            };

            var result = await Mediator.Send(query);
            if (result != null)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return File(result.File, result.ContentType, result.FileName);

            }
            else
            {
                return NotFound("Watermark document not found.");
            }
        }


        /// <summary>
        /// Get Customer Verification Thumbnail Document Detail
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("customer-verifications/document-thumbnail-details/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetCustomerVerificationDocumentThumbnailDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerificationDocumentThumbnailDetails), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> GetCustomerVerificationDocumentThumbnailDetails(Guid documentId)
        {
            GetCustomerVerificationDocumentThumbnailDetailsQuery query = new GetCustomerVerificationDocumentThumbnailDetailsQuery()
            {
                DocumentId = documentId,
            };

            var result = await Mediator.Send(query);
            if (result != null && result.FileData != null && result.FileData.Length > 0)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                return File(result.FileData, result.ContentType, result.FileName);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Get Customer Verification Thumbnail Document Detail
        /// </summary>
        /// <param name="customerVerificationDocumentCode"></param>
        /// <returns></returns>
        [HttpGet("customer-verifications/document-watermark-thumbnail/{customerVerificationDocumentCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetCustomerVerificationDocumentWatermarkThumbnailDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerificationDocumentWatermarkThumbnailDetails), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> GetCustomerVerificationDocumentWatermarkThumbnailDetails(long? customerVerificationDocumentCode)
        {
            GetCustomerVerificationDocumentWatermarkThumbnailDetailsQuery query = new GetCustomerVerificationDocumentWatermarkThumbnailDetailsQuery()
            {
                CustomerVerificationDocumentCode = customerVerificationDocumentCode,
            };

            var result = await Mediator.Send(query);
            if (result != null && result.FileData != null && result.FileData.Length > 0)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                return File(result.FileData, result.ContentType, result.FileName);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Upload Customer Verification Templates
        /// </summary>
        /// <param name="customerVerificationCode"></param>
        /// <param name="adminSolution"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [HttpPost("customer-verifications/templates")]
        [ProducesResponseType(typeof(CustomerVerificationDocumentTemplatesOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload Document Templates ", Required = false)]
        [SwaggerOperation(OperationId = nameof(SaveCustomerVerificationTemplateDocument), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> SaveCustomerVerificationTemplateDocument(long? customerVerificationCode, long? adminSolution, IFormFile uploadedFile)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SaveCustomerVerificationTemplateDocumentCommand command = new SaveCustomerVerificationTemplateDocumentCommand()
            {
                CustomerVerificationCode = customerVerificationCode,
                CustomerSolution = solution.HasValue ? solution.Value : null, // convert Maybe<string> to string
                AdminSolution = adminSolution,
                UploadedFile = uploadedFile
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SaveVerificationDocumentCommand] {result.Error}");
                return BadRequest(result);
            }


            return Ok(result.Value);
        }

        /// <summary>
        /// Delete  Customer Verification Template 
        /// </summary>
        /// <param name="customerVerificationCode"></param>
        /// <param name="adminSolution"></param>
        ///  <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(CustomerVerificationTemplateOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(DeleteCustomerVerificationTemplate), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        [HttpDelete("customer-verifications/{customerVerificationCode}/documents-templates")]
        public async Task<IActionResult> DeleteCustomerVerificationTemplate(long? customerVerificationCode, long? adminSolution)
        {
            DeleteCustomerVerificationTemplateCommand command = new DeleteCustomerVerificationTemplateCommand
            {
                CustomerVerificationCode = customerVerificationCode,
                AdminSolution = adminSolution

            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            _logger.LogError($"Failed to delete  Customer Verification  Code: {customerVerificationCode}");

            return BadRequest(result.Error);
        }


        /// <summary>
        /// Retrieve Customer Verification Templates by Customer Verification Code
        /// </summary>
        /// <param name="customerVerificationCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [HttpGet("customer-verifications/{customerVerificationCode}/documents-templates")]
        [ProducesResponseType(typeof(GetCustomerVerificationDocumentTemplatesOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerificationTemplate), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<ActionResult> GetCustomerVerificationTemplate(long? customerVerificationCode, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            GetCustomerVerificationTemplateQuery query = new GetCustomerVerificationTemplateQuery()
            {
                CustomerVerificationCode = customerVerificationCode,
            };

            var result = await Mediator.Send(query);
            if (result != null && result.File != null && result.File.Length > 0)
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
        /// Get Customer Verification Risk Results by Customer Verification Code
        /// </summary>
        /// <param name="customerVerificationCode"></param>
        ///  <returns></returns>
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetCustomerVerificationRiskResultsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerVerificationRiskResults), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        [HttpGet("customer-verifications/{customerVerificationCode}/risk-results")]
        public async Task<IActionResult> GetCustomerVerificationRiskResults(long? customerVerificationCode)
        {
            //Change to Business Profile Code
            GetCustomerVerificationRiskResultsByVerificationCodeQuery query = new GetCustomerVerificationRiskResultsByVerificationCodeQuery
            {
                CustomerVerificationCode = customerVerificationCode
            };

            var result = await Mediator.Send(query);

            return Ok(result.Value);
        }

        /// <summary>
        /// Submit for Jumio Verification
        /// </summary>
        /// <param name="customerVerificationCode"></param>
        /// <returns></returns>
        [HttpPost("customer-verifications/{customerVerificationCode}/submit-verification")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(SubmitVerificationOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SubmitVerification), Tags = new[] { "KYC (Know Your Customer) - Customer Verification" })]
        public async Task<IActionResult> SubmitVerification(long customerVerificationCode)
        {
            SubmitVerificationCommand command = new SubmitVerificationCommand
            {
                CustomerVerificationCode = customerVerificationCode
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SubmitVerification] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }
    }
}