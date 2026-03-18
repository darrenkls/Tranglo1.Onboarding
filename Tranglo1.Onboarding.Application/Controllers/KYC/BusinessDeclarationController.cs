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
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO.BusinessDeclaration;
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
    public class BusinessDeclarationController : ControllerBase
    {
        private readonly ILogger<BusinessDeclarationController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public BusinessDeclarationController(ILogger<BusinessDeclarationController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve Business Declaration per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/business-declarations")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<GetCustomerBusinessDeclarationOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerBusinessDeclaration), Tags = new[] { "KYC (Know Your Customer) - Business Declaration" })]
        public async Task<GetCustomerBusinessDeclarationOutputDTO> GetCustomerBusinessDeclaration(int businessProfileCode)
        {
            GetCustomerBusinessDeclarationQuery query = new GetCustomerBusinessDeclarationQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Business Declaration Document Upload
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="customerBusinessDeclarationAnswerCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [RequestSizeLimit(52428800)]
        [HttpPost("{businessProfileCode}/business-declarations/document-uploads")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(UploadBusinessDeclarationDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UploadBusinessDeclarationDocument), Tags = new[] { "KYC (Know Your Customer) - Business Declaration" })]
        public async Task<IActionResult> UploadBusinessDeclarationDocument(int businessProfileCode, long customerBusinessDeclarationAnswerCode, IFormFile uploadedFile)
        {
            UploadBusinessDeclarationDocumentCommand command = new UploadBusinessDeclarationDocumentCommand
            {
                BusinessProfileCode = businessProfileCode,
                CustomerBusinessDeclarationAnswerCode = customerBusinessDeclarationAnswerCode,
                UploadedFile = uploadedFile
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UploadBusinessDeclarationDocument] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Save Customer Business Declaration
        /// </summary>
        /// <param name="inputDTO"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/business-declarations/submit")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(SaveCustomerBusinessDeclarationAnswersInputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveCustomerBusinessDeclarationAnswers), Tags = new[] { "KYC (Know Your Customer) - Business Declaration" })]
        public async Task<Result<SaveCustomerBusinessDeclarationAnswersInputDTO>> SaveCustomerBusinessDeclarationAnswers(SaveCustomerBusinessDeclarationAnswersInputDTO inputDTO)
        {
            SaveCustomerBusinessDeclarationAnswersCommand command = new SaveCustomerBusinessDeclarationAnswersCommand
            {
                InputDTO = inputDTO
            };

            return await Mediator.Send(command);
        }


        /// <summary>
        /// Unblock Customer Business Declaration
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="customerBusinessDeclarationCode"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/business-declarations/unblock")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(SaveCustomerBusinessDeclarationAnswersInputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UnblockCustomerBusinessDeclaration), Tags = new[] { "KYC (Know Your Customer) - Business Declaration" })]
        public async Task<IActionResult> UnblockCustomerBusinessDeclaration(int businessProfileCode, long customerBusinessDeclarationCode)
        {
            UnblockCustomerBusinessDeclarationCommand command = new UnblockCustomerBusinessDeclarationCommand
            {
                BusinessProfileCode = businessProfileCode,
                CustomerBusinessDeclarationCode = customerBusinessDeclarationCode
            };

            Result result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UnblockCustomerBusinessDeclaration] {result.Error}");
                return BadRequest(result);
            }

            return Ok();
        }

        /// <summary>
        /// Retrieve Business Declaration Details per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/business-declarations/details")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<GetCustomerBusinessDeclarationDetailsOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetCustomerBusinessDeclarationDetails), Tags = new[] { "KYC (Know Your Customer) - Business Declaration" })]
        public async Task<GetCustomerBusinessDeclarationDetailsOutputDTO> GetCustomerBusinessDeclarationDetails(int businessProfileCode)
        {
            GetCustomerBusinessDeclarationDetailsQuery query = new GetCustomerBusinessDeclarationDetailsQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// File download
        /// </summary>
        /// <param name="customerBusinessDeclarationAnswerCode"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("business-declarations/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DownloadDeclarationDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetRegulatorLetterDocument), Tags = new[] { "KYC (Know Your Customer) - Business Declaration" })]
        public async Task<ActionResult> GetRegulatorLetterDocument(long customerBusinessDeclarationAnswerCode, Guid documentId)
        {
            DownloadDeclarationDocumentQuery query = new DownloadDeclarationDocumentQuery()
            {
                CustomerBusinessDeclarationAnswerCode = customerBusinessDeclarationAnswerCode,
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
