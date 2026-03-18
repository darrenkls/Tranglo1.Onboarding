using AutoMapper;
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
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.AuthorisedPerson;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Security;

namespace Tranglo1.Onboarding.Application.Controllers.KYC
{
    [ApiController]
    [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/documentation")]
    [LogInputDTO]
    [LogOutputDTO]
    public class DocumentationController : ControllerBase
    {
        private readonly ILogger<DocumentationController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public DocumentationController(ILogger<DocumentationController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve Document Category List by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/business-document-categories")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<BusinessDocumentCategoryListOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetDocumentCategories), Tags = new[] { "KYC (Know Your Customer)" })]
        public async Task<IEnumerable<BusinessDocumentCategoryListOutputDTO>> GetDocumentCategories([BusinessProfileId] int businessProfileCode, int solutionCode, int trangloEntityCode)
        {
            GetBusinessDocumentsCategoriesListQuery query = new GetBusinessDocumentsCategoriesListQuery()
            {
                BusinessProfileCode = businessProfileCode,
                SolutionCode = solutionCode,
                TrangloEntityCode = trangloEntityCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Get document details
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("document-details/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DocumentDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetDocumentDetails), Tags = new[] { "KYC (Know Your Customer) - Document Details" })]
        public async Task<ActionResult> GetDocumentDetails(Guid documentId)
        {
            GetDocumentDetailsQuery  query = new GetDocumentDetailsQuery()
            {
                DocumentId = documentId,
            };

            var result = await Mediator.Send(query);
            if (result != null)
            {
                return Ok(result);  
              
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Validation overall documentation
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/overall-documentation")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AuthorisedPersonOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(ValidateOverallDocumentationCommand), Tags = new[] { "KYC (Know Your Customer) - Documentation Details" })]
        public async Task<IActionResult> ValidateOverallDocumentationCommand([BusinessProfileId] int businessProfileCode, bool fromComment)
        {
            ValidateOverallDocumentationCommand command = new ValidateOverallDocumentationCommand
            {
                BusinessProfileCode = businessProfileCode,
                FromComment = fromComment
            };

            var result = await Mediator.Send(command);

            return Ok(result);
        }
    }
}
