using AutoMapper;
using IdentityServer4.Extensions;
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
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.Onboarding.Application.DTO.Declarations;
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
    public class DeclarationController : ControllerBase
    {
        private readonly ILogger<DeclarationController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public DeclarationController(ILogger<DeclarationController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve declaration information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/declarations")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DeclarationsOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetDeclarationInfoByCode), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> GetDeclarationInfoByCode([BusinessProfileId] int businessProfileCode)
        {
            GetDeclarationByIdQuery query = new GetDeclarationByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetDeclarationByIdQuery] {result.Error}");
                return NotFound();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Insert declaration information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="viewModel"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/declarations")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(CreateDeclarationInfoByCode), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> CreateDeclarationInfoByCode([BusinessProfileId] int businessProfileCode, [FromBody] DeclarationsInputDTO viewModel,long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            CreateDeclarationCommand command = _mapper.Map<CreateDeclarationCommand>(viewModel);
            command.BusinessProfileCode = businessProfileCode;
            command.LoginId = User.GetSubjectId();
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[CreateDeclarationInfoByCode] {result.Error}");
                return ValidationProblem();
            }

            return Ok(new { DeclarationCode = result.Value });

        }

        /// <summary>
        /// Update declaration information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/declarations")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdateDeclarationInfo), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> UpdateDeclarationInfo([BusinessProfileId] int businessProfileCode, [FromBody] DeclarationsInputDTO viewModel)
        {
            UpdateDeclarationInformationCommand command = _mapper.Map<UpdateDeclarationInformationCommand>(viewModel);
            command.BusinessProfileCode = businessProfileCode;
            command.LoginId = User.GetSubjectId();

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdateDeclarationInformationCommand] {result.Error}");
                return ValidationProblem();
            }

            return Ok();

        }

        /// <summary>
        /// Retrieve declaration signature by business profile code and document ID
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{businessProfileCode}/declarations/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DeclarationSignatureOutputDto), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetDeclarationSignature), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> GetDeclarationSignature([BusinessProfileId] int businessProfileCode, Guid documentId)
        {
            GetDeclarationSignatureByDocIdQuery query = new GetDeclarationSignatureByDocIdQuery
            {
                BusinessProfileCode = businessProfileCode,
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
        /// Upload signature image file
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{businessProfileCode}/declarations/signature")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Add authorized declaration signature", Required = false)]
        [SwaggerOperation(OperationId = nameof(AddSignature), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> AddSignature([BusinessProfileId] int businessProfileCode, IFormFile uploadedFile)
        {
            AddDeclarationSignatureCommand command = new AddDeclarationSignatureCommand()
            {
                BusinessProfileCode = businessProfileCode,
                uploadedFile = uploadedFile,
                LoginId = User.GetSubjectId()
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[AddDeclarationSignatureCommand] {result.Error}");
                return ValidationProblem();
            }

            return Ok(new { DocumentId = result.Value });
        }

        /// <summary>
        /// Delete signature image file
        /// </summary>
        /// <param name="declarationCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("declaration/{declarationCode}/signature")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(DeleteDeclarationSignature), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> DeleteDeclarationSignature(int declarationCode, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            DeleteDeclarationSignatureCommand command = new DeleteDeclarationSignatureCommand()
            {
                DeclarationCode = declarationCode,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId()
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[DeleteBusinessUserDeclarationSignatureCommand] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        #region Business User Declaration
        /// <summary>
        /// Retrieve declaration information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/business-user-declaration")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetBusinessUserDeclarationOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetBusinessUserDeclaration), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> GetBusinessUserDeclaration([BusinessProfileId] int businessProfileCode,long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            GetBusinessUserDeclarationByBusinessProfileCodeQuery query = new GetBusinessUserDeclarationByBusinessProfileCodeQuery
            {
                BusinessProfileCode = businessProfileCode,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId()
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetBusinessUserDeclarationByBusinessProfileCodeQuery] {result.Error}");
                return NotFound();
            }

            return Ok(result.Value);

        }

        /// <summary>
        /// Insert declaration information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="inputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <param name="businessUserDeclarationConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/business-user-declarations")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(CreateBusinessUserDeclaration), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> CreateBusinessUserDeclaration([BusinessProfileId] int businessProfileCode, [FromBody] BusinessUserDeclarationInputDTO inputDTO,
            long? adminSolution, Guid? businessUserDeclarationConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            CreateBusinessUserDeclarationCommand command = new CreateBusinessUserDeclarationCommand()
            {
                BusinessProfileCode = businessProfileCode,
                InputDTO = inputDTO,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId(),
                BusinessUserDeclarationConcurrencyToken = businessUserDeclarationConcurrencyToken,
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
                _logger.LogError($"[SaveComplianceOfficers] {result.Error}");
                return ValidationProblem();
            }


            return Ok(result.Value);

        }
        /// <summary>
        /// Update declaration information by business profile code
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="inputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <param name="businessUserDeclarationConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPut("{businessProfileCode}/business-user-declarations")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdateBusinessUserDeclaration), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> UpdateBusinessUserDeclaration([BusinessProfileId] int businessProfileCode, 
            [FromBody] BusinessUserDeclarationInputDTO inputDTO,long? adminSolution, Guid? businessUserDeclarationConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            UpdateBusinessUserDeclarationCommand command = new UpdateBusinessUserDeclarationCommand()
            {
                BusinessProfileCode = businessProfileCode,
                InputDTO = inputDTO,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId(),
                BusinessUserDeclarationConcurrencyToken = businessUserDeclarationConcurrencyToken
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
                _logger.LogError($"[SaveComplianceOfficers] {result.Error}");
                return ValidationProblem();
            }


            return Ok(result.Value);

        }

        /// <summary>
        /// Upload signature image file
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="uploadedFile"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{businessProfileCode}/business-user-declaration/signature")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Add authorized declaration signature", Required = false)]
        [SwaggerOperation(OperationId = nameof(AddBusinessUserDeclarationSignature), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> AddBusinessUserDeclarationSignature([BusinessProfileId] int businessProfileCode, IFormFile uploadedFile,long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            AddBusinessUserDeclarationSignatureCommand command = new AddBusinessUserDeclarationSignatureCommand()
            {
                BusinessProfileCode = businessProfileCode,
                uploadedFile = uploadedFile,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId()
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[AddBusinessUserDeclarationSignatureCommand] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieve declaration signature by business profile code and document ID
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="documentId"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{businessProfileCode}/business-user-declaration/{documentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(DeclarationSignatureOutputDto), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetBusinessUserDeclarationSignature), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> GetBusinessUserDeclarationSignature([BusinessProfileId] int businessProfileCode, Guid documentId, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            GetBusinessUserDeclarationSignatureByDocumentID query = new GetBusinessUserDeclarationSignatureByDocumentID
            {
                BusinessProfileCode = businessProfileCode,
                DocumentId = documentId,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId()
            };

            var result = await Mediator.Send(query);
            if (result.IsSuccess)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return File(result.Value.File, result.Value.ContentType, result.Value.FileName);
            }
            else
            {
                return NotFound();
            }
        }
       
        /// <summary>
        /// Delete signature image file
        /// </summary>
        /// <param name="businessUserDeclarationCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("business-user-declaration/{businessUserDeclarationCode}/signature")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(DeleteBusinessUserDeclarationSignature), Tags = new[] { "KYC (Know Your Customer) - Declaration" })]
        public async Task<IActionResult> DeleteBusinessUserDeclarationSignature( int businessUserDeclarationCode, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            DeleteBusinessUserDeclarationSignatureCommand command = new DeleteBusinessUserDeclarationSignatureCommand()
            {
                BusinessUserDeclarationCode = businessUserDeclarationCode,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = User.GetSubjectId()
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[DeleteBusinessUserDeclarationSignatureCommand] {result.Error}");
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        #endregion
    }
}
