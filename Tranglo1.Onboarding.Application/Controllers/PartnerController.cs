using AutoMapper;
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
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerOnboarding;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.Infrastructure.Swagger;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Security;

namespace Tranglo1.Onboarding.Application.Controllers
{
    #region Partner
    [ApiController]
    [Authorize]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/partners")]
    [LogInputDTO]
    [LogOutputDTO]
    public class PartnerController : ControllerBase
    {
        private readonly ILogger<PartnerController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;

        public PartnerController(ILogger<PartnerController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get partner name list by solutionCode 
        /// </summary>
        [HttpGet("partner-name-list")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerNameBySolutions), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerNameListBySolutionOutputDTO>> PartnerNameBySolutions(long? adminSolution, string entityCode)
        {
            GetPartnerNameListBySolutionQuery query = new GetPartnerNameListBySolutionQuery { };

            query.AdminSolution = adminSolution;
            query.EntityCode = entityCode;

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[PartnerNameBySolutions] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get trade name list by solutionCode 
        /// </summary>
        [HttpGet("trade-name-list")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(TradeNameBySolutions), Tags = new[] { "Partner" })]
        public async Task<ActionResult<TradeNameListBySolutionOutputDTO>> TradeNameBySolutions(long? adminSolution)
        {
            GetTradeNameListBySolutionQuery query = new GetTradeNameListBySolutionQuery { };

            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[TradeNameBySolutions] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get all partner subscriptions  by partnerCode
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/partner-all-subscriptions")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(GetAllPartnerSubscriptionOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerAgreementTemplates), Tags = new[] { "Partner" })]
        public async Task<ActionResult<GetAllPartnerSubscriptionOutputDTO>> GetPartnerAllSubscriptions([PartnerCode] long partnerCode)
        {
            GetAllPartnerSubscriptionQuery query = new GetAllPartnerSubscriptionQuery()
            {
                PartnerCode = partnerCode
            };


            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPartnerAgreementTemplates] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Upload partner agreement template
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="uploadedFile"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/agreement-templates")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload partner agreement template", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadPartnerAgreementTemplate), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UploadPartnerAgreementTemplate([PartnerCode] long partnerCode, IFormFile uploadedFile, long? adminSolution)
        {
            SavePartnerAgreementTemplateCommand command = new SavePartnerAgreementTemplateCommand()
            {
                PartnerCode = partnerCode,
                UploadedFile = uploadedFile
            };

            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UploadPartnerAgreementTemplate] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Remove uploaded partner agreement template
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/agreement-templates/{templateId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RemovePartnerAgreementTemplate), Tags = new[] { "Partner" })]
        public async Task<IActionResult> RemovePartnerAgreementTemplate([PartnerCode] long partnerCode, Guid templateId)
        {
            RemovePartnerAgreementTemplateCommand command = new RemovePartnerAgreementTemplateCommand()
            {
                PartnerCode = partnerCode,
                TemplateId = templateId
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[RemovePartnerAgreementTemplate] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get partner agreement templates by partnerCode
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/agreement-templates")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(PartnerAgreementTemplateOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerAgreementTemplates), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerAgreementTemplateOutputDTO>> GetPartnerAgreementTemplates([PartnerCode] long partnerCode, long? adminSolution)
        {
            GetPartnerAgreementTemplatesQuery query = new GetPartnerAgreementTemplatesQuery()
            {
                PartnerCode = partnerCode
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;
            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPartnerAgreementTemplates] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get partner agreement template by templateId
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/agreement-templates/{templateId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(PartnerAgreementTemplateOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerAgreementTemplateByTemplateId), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerAgreementTemplateOutputDTO>> GetPartnerAgreementTemplateByTemplateId([PartnerCode] long partnerCode, Guid templateId)
        {
            GetPartnerAgreementTemplateByTemplateIdQuery query = new GetPartnerAgreementTemplateByTemplateIdQuery()
            {
                PartnerCode = partnerCode,
                TemplateId = templateId
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPartnerAgreementTemplateByTemplateId] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Upload signed partner agreement
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="uploadedFile"></param>
        /// <param name="adminSolution"><</param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/signed-agreements")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Upload signed partner agreement", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadSignedPartnerAgreement), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UploadSignedPartnerAgreement([PartnerCode] long partnerCode, IFormFile uploadedFile, long? adminSolution)
        {
            SaveSignedPartnerAgreementCommand command = new SaveSignedPartnerAgreementCommand()
            {
                PartnerCode = partnerCode,
                UploadedFile = uploadedFile,
                UserType = User.GetUserType()
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UploadSignedPartnerAgreement] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }


        /// <summary>
        /// Remove signed partner agreement
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="signedDocumentId"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/signed-agreements/{signedDocumentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RemoveSignedPartnerAgreement), Tags = new[] { "Partner" })]
        public async Task<IActionResult> RemoveSignedPartnerAgreement([PartnerCode] long partnerCode, Guid signedDocumentId)
        {
            RemoveSignedPartnerAgreementCommand command = new RemoveSignedPartnerAgreementCommand()
            {
                PartnerCode = partnerCode,
                SignedDocumentId = signedDocumentId
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[RemoveSignedPartnerAgreement] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Update signed partner agreement isDisplay
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="signedDocumentId"></param>
        /// <param name="signedPartnerInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/signed-partner-agreements/{signedDocumentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdateSignedPartnerAgreement), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdateSignedPartnerAgreement([PartnerCode] long partnerCode, Guid signedDocumentId, [FromBody] SignedPartnerAgreementInputDTO signedPartnerInputDTO)
        {
            UpdateSignedPartnerAgreementCommand command = _mapper.Map<UpdateSignedPartnerAgreementCommand>(signedPartnerInputDTO);
            command.PartnerCode = partnerCode;
            command.SignedDocumentId = signedDocumentId;
            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdateSignedPartnerAgreement] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get signed partner agreements by partnerCode
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/signed-agreements")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetSignedPartnerAgreements), Tags = new[] { "Partner" })]
        public async Task<ActionResult<List<SignedPartnerAgreementOutputDTO>>> GetSignedPartnerAgreements([PartnerCode] long partnerCode, long? adminSolution)
        {
            GetSignedPartnerAgreementsQuery query = new GetSignedPartnerAgreementsQuery()
            {
                PartnerCode = partnerCode,
                UserType = User.GetUserType()
            };
            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;
            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetSignedPartnerAgreements] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get signed partner agreement by signedDocumentId
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="signedDocumentId"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/signed-agreements/{signedDocumentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(SignedPartnerAgreementOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetSignedPartnerAgreementBySignedDocumentId), Tags = new[] { "Partner" })]
        public async Task<ActionResult<SignedPartnerAgreementOutputDTO>> GetSignedPartnerAgreementBySignedDocumentId([PartnerCode] long partnerCode, Guid signedDocumentId)
        {
            GetSignedPartnerAgreementBySignedDocumentIdQuery query = new GetSignedPartnerAgreementBySignedDocumentIdQuery()
            {
                PartnerCode = partnerCode,
                SignedDocumentId = signedDocumentId
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetSignedPartnerAgreementBySignedDocumentId] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Add HelloSign document 
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="helloSignDocumentInputDTO"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/hellosign-document")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(AddHelloSignDocument), Tags = new[] { "Partner" })]
        public async Task<IActionResult> AddHelloSignDocument([PartnerCode] long partnerCode, [FromBody] HelloSignDocumentInputDTO helloSignDocumentInputDTO, long? adminSolution)
        {
            SaveHelloSignDocumentCommand command = _mapper.Map<SaveHelloSignDocumentCommand>(helloSignDocumentInputDTO);
            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            command.PartnerCode = partnerCode;
            command.CustomerSolution = solution.HasValue ? solution.Value : null;
            command.AdminSolution = adminSolution;

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[AddHelloSignDocument] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Remove HelloSign document
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="helloSignDocumentId"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/hellosign-document/{helloSignDocumentId}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RemoveHelloSignDocument), Tags = new[] { "Partner" })]
        public async Task<IActionResult> RemoveHelloSignDocument([PartnerCode] long partnerCode, long helloSignDocumentId)
        {
            RemoveHelloSignDocumentCommand command = new RemoveHelloSignDocumentCommand()
            {
                PartnerCode = partnerCode,
                HelloSignDocumentId = helloSignDocumentId
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[RemoveHelloSignDocument] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get HelloSign documents list
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/hellosign-documents")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetHelloSignDocuments), Tags = new[] { "Partner" })]
        public async Task<ActionResult<List<HelloSignDocumentOutputDTO>>> GetHelloSignDocuments([PartnerCode] long partnerCode, long? adminSolution)
        {
            GetHelloSignDocumentsQuery query = new GetHelloSignDocumentsQuery()
            {
                PartnerCode = partnerCode
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;
            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetHelloSignDocuments] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get HelloSign document by helloSignDocumentId
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="helloSignDocumentId"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/hellosign-document")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(HelloSignDocumentOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetHelloSignDocumentByHelloSignDocumentId), Tags = new[] { "Partner" })]
        public async Task<ActionResult<HelloSignDocumentOutputDTO>> GetHelloSignDocumentByHelloSignDocumentId([PartnerCode] long partnerCode, long helloSignDocumentId)
        {
            GetHelloSignDocumentByHelloSignDocumentIdQuery query = new GetHelloSignDocumentByHelloSignDocumentIdQuery()
            {
                PartnerCode = partnerCode,
                HelloSignDocumentId = helloSignDocumentId
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetHelloSignDocumentByHelloSignDocumentId] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get partner agreement status by partnerCode
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/agreement-status")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerAgreementStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerAgreementStatusOutputDTO>> GetPartnerAgreementStatus([PartnerCode] long partnerCode)
        {
            GetPartnerAgreementStatusQuery query = new GetPartnerAgreementStatusQuery()
            {
                PartnerCode = partnerCode
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPartnerAgreementStatus] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Check agreement expiration date and do necessary update on agreement status 
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/agreement-status-expiration")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerAgreementExpirationStatus), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerAgreementExpirationStatus([PartnerCode] long partnerCode)
        {
            UpdatePartnerAgreementExpirationStatusCommand command = new UpdatePartnerAgreementExpirationStatusCommand()
            {
                PartnerCode = partnerCode
            };
            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePartnerAgreementExpirationStatus] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Update Partner Agreement Status
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerAgreementStatusInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/agreement-status")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerAgreementStatus), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerAgreementStatus([PartnerCode] long partnerCode, [FromBody] PartnerAgreementStatusInputDTO partnerAgreementStatusInputDTO)
        {
            UpdatePartnerAgreementStatusCommand command = _mapper.Map<UpdatePartnerAgreementStatusCommand>(partnerAgreementStatusInputDTO);
            command.PartnerCode = partnerCode;
            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePartnerAgreementStatus] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get partner details by partnerCode
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="trangloEntity"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/partner-details")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(PartnerDetailsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerDetailsByPartnerCode), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerDetailsOutputDTO>> GetPartnerDetailsByPartnerCode([PartnerCode] long partnerCode, [FromQuery] string trangloEntity)
        {
            GetPartnerDetailsByPartnerCodeQuery query = new GetPartnerDetailsByPartnerCodeQuery()
            {
                PartnerCode = partnerCode,
                TrangloEntity = trangloEntity
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPartnerDetailsByPartnerCode] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }


        /// <summary>
        /// Update Partner Account Status
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="PartnerAccountStatusInputDTO"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/partner-account-statuses")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerAccountStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerAccountStatusInputDTO>> UpdatePartnerAccountStatus([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] PartnerAccountStatusInputDTO PartnerAccountStatusInputDTO)
        {
            SavePartnerAccountStatusCommand command = new SavePartnerAccountStatusCommand()
            {
                PartnerCode = partnerCode,
                PartnerSusbscriptionCode = partnerSubscriptionCode,
                PartnerAccStatus = PartnerAccountStatusInputDTO
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdatePartnerAccountStatus] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok();
        }

        /// <summary>
        /// Get Partner Account Status
        /// </summary>
        /// <returns></returns>
        [HttpGet("{partnerCode}/partner-account-statuses-history")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<PartnerAccountStatusOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerAccountStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerAccountStatusOutputDTO>> GetPartnerAccountStatus([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerAccountStatusQuery command = new GetPartnerAccountStatusQuery()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetPartnerAccountStatus] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Register New Partner Account 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(PartnerRegistrationOutputDTO), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SavePartnerRegistration), Tags = new[] { "Partner" })]
        public async Task<IActionResult> SavePartnerRegistration([FromBody] PartnerRegistrationInputDTO partnerRegistrationInputDTO)
        {

            //var access_token = await HttpContext.GetTokenAsync("access_token");
            var access_token = Request.Headers["Authorization"].ToString();

            SavePartnerRegistrationCommand command = _mapper.Map<SavePartnerRegistrationCommand>(partnerRegistrationInputDTO);
            command.UserBearerToken = access_token;

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[SavePartnerRegistration] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Update Partner Account 
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerRegistration), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerRegistration([PartnerCode] long partnerCode, [FromBody] UpdatePartnerInputDTO updatePartnerRegistrationInputDTO, long? adminSolution)
        {
            UpdatePartnerRegistrationCommand command = new UpdatePartnerRegistrationCommand()
            {
                PartnerCode = partnerCode,
                UpdatePartnerRegistration = updatePartnerRegistrationInputDTO,
                UserBearerToken = Request.Headers["Authorization"].ToString(),
                LoginId = User.GetSubjectId().Value
            };

            command.AdminSolution = adminSolution;
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdatePartnerRegistration] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Get Partner Listing in Admin portal.
        /// </summary>
        /// <param name="PartnerName"></param>
        /// <param name="TradeName"></param>
        /// <param name="Entity"></param>
        /// <param name="PartnerType"></param>
        /// <param name="CountryISO2"></param>
        /// <param name="Agent"></param>
        /// <param name="AgreementStatusCode"></param>
        /// <param name="AgreementStartDate"></param>
        /// <param name="AgreementEndDate"></param>
        /// <param name="WorkFlowStatusCode"></param>
        /// <param name="StatusCode"></param>
        /// <param name="EnvironmentCode"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="adminSolution"></param>
        /// <param name="kycApprovalStatusCode"></param>
        /// <param name="kycStatusCode"></param>
        /// <param name="kycReminderStatusCode"></param>
        /// <param name="leadsOriginCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(DisplayPartnerListing), Tags = new[] { "Partner" })]
        public async Task<ActionResult<IEnumerable<PartnerListingOutputDTO>>> DisplayPartnerListing
        (string PartnerName, string TradeName, [TrangloEntityId] string Entity, string PartnerType, string CountryISO2, string Agent, long AgreementStatusCode, string AgreementStartDate,
            string AgreementEndDate, long WorkFlowStatusCode, long StatusCode, long EnvironmentCode,
            int pageSize, int page, string sortExpression, SortDirection sortDirection, long? adminSolution, int? kycApprovalStatusCode, int? kycStatusCode, int? kycReminderStatusCode, int? leadsOriginCode)
        {

            GetPartnerListingQuery query = new GetPartnerListingQuery
            {
                PartnerName = PartnerName,
                TradeName = TradeName,
                Entity = Entity,
                PartnerType = PartnerType,
                CountryISO2 = CountryISO2,
                Agent = Agent,
                AgreementStatusCode = AgreementStatusCode,
                AgreementStartDate = AgreementStartDate,
                AgreementEndDate = AgreementEndDate,
                WorkFlowStatusCode = WorkFlowStatusCode,
                StatusCode = StatusCode,
                EnvironmentCode = EnvironmentCode,
                KYCApprovalStatusCode = kycApprovalStatusCode,
                KYCStatusCode = kycStatusCode,
                KYCReminderStatusCode = kycReminderStatusCode,
                PagingOptions = new PagingOptions
                {
                    PageSize = pageSize,
                    PageIndex = page,
                    SortExpression = sortExpression,
                    Direction = sortDirection
                },
                UserBearerToken = Request.Headers["Authorization"].ToString(),
                LeadsOriginCode = leadsOriginCode
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;
            query.AdminSolution = adminSolution;

            var result = await Mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("{partnerCode}")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(DisplayIndividualPartnerListing), Tags = new[] { "Partner" })]
        public async Task<ActionResult<IndividualPartnerListingOutputDTO>> DisplayIndividualPartnerListing([PartnerCode] long partnerCode, [TrangloEntityId] string trangloEntity)
        {
            GetIndividualPartnerListingQuery query = new GetIndividualPartnerListingQuery
            {
                PartnerCode = partnerCode,
                TrangloEntity = trangloEntity
            };

            var result = await Mediator.Send(query);
            return Ok(result.Value);
        }

        #endregion Partner

        #region - API Settings
        /// <summary>
        /// Get partner listing result based on the search criteria
        /// </summary>
        /// <param name="partnerName"></param>
        /// <param name="partnerTypeCode"></param>
        /// <param name="solutionCode"></param>
        /// <param name="entityCode"></param>
        /// <param name="currentAPIEnvironmentCode"></param>
        /// <param name="agreementStatusCode"></param>
        /// <param name="partnerAccountStatusCode"></param>
        /// <param name="registrationDateFrom"></param>
        /// <param name="registrationDateTo"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet("partner-listing")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(PagedResult<PartnerListingSearchResultOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPartnerListingSearchResult), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PagedResult<PartnerListingSearchResultOutputDTO>>> GetPartnerListingSearchResult(string partnerName, int? partnerTypeCode, int? solutionCode, [TrangloEntityId] string entityCode,
            int? currentAPIEnvironmentCode, int? agreementStatusCode, int? partnerAccountStatusCode, string registrationDateFrom, string registrationDateTo,
            int pageIndex, int pageSize, string sortExpression, SortDirection sortDirection)
        {
            GetPartnerListingSearchResultQuery query = new GetPartnerListingSearchResultQuery
            {
                PartnerName = partnerName,
                PartnerTypeCode = partnerTypeCode,
                SolutionCode = solutionCode,
                EntityCode = entityCode,
                CurrentAPIEnvironmentCode = currentAPIEnvironmentCode,
                AgreementStatusCode = agreementStatusCode,
                PartnerAccountStatusCode = partnerAccountStatusCode,
                FromRegistrationDate = registrationDateFrom,
                ToRegistrationDate = registrationDateTo,
                PagingOptions = new PagingOptions
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortExpression = sortExpression,
                    Direction = sortDirection
                }
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Get API Settings by partner code
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/api-settings")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(PartnerAPISettingsOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAPISettingsByPartnerCode), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerAPISettingsOutputDTO>> GetAPISettingsByPartnerCode([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerAPISettingsByPartnerCodeQuery query = new GetPartnerAPISettingsByPartnerCodeQuery
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };
            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetAPISettingsByPartnerCode] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// GET all environments API URL values
        /// </summary>
        /// <returns></returns>
        [HttpGet("api-settings/environments-api-url")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(APIURLOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetAllEnvironmentsAPIURL), Tags = new[] { "Partner" })]
        public async Task<ActionResult<APIURLOutputDTO>> GetAllEnvironmentsAPIURL()
        {
            GetAllEnvironmentsAPIURLQuery query = new GetAllEnvironmentsAPIURLQuery();
            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[GetAllEnvironmentsAPIURL] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Add API Settings for Partner
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="partnerAPISettingsInputDTO"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/api-settings")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(AddPartnerAPISettings), Tags = new[] { "Partner" })]
        public async Task<IActionResult> AddPartnerAPISettings([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] PartnerAPISettingsInputDTO partnerAPISettingsInputDTO)
        {
            SavePartnerAPISettingsCommand command = _mapper.Map<SavePartnerAPISettingsCommand>(partnerAPISettingsInputDTO);
            command.PartnerCode = partnerCode;
            command.PartnerSubscriptionCode = partnerSubscriptionCode;
            command.Staging = partnerAPISettingsInputDTO.Staging;
            command.Production = partnerAPISettingsInputDTO.Production;
            command.InputDTO = partnerAPISettingsInputDTO;

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[AddPartnerAPISettings] {result.Error}");
                return BadRequest(result);
            }
            return Ok();
        }

        /// <summary>
        /// Update API Settings for Partner
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="partnerAPISettingsInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/api-settings")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerAPISettings), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerAPISettings([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] PartnerAPISettingsUpdateInputDTO partnerAPISettingsInputDTO)
        {
            UpdatePartnerApiSettingsCommand command = new UpdatePartnerApiSettingsCommand()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                Staging = partnerAPISettingsInputDTO.Staging,
                Production = partnerAPISettingsInputDTO.Production,
                InputDTO = partnerAPISettingsInputDTO
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePartnerAPISettings] {result.Error}");
                return BadRequest(result);
            }
            return Ok();
        }

        /// <summary>
        /// Update API Settings user for Partner
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="partnerAPISettingsInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/api-settings/user")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerAPISettingsUser), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerAPISettingsUser([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] PartnerAPISettingsUserUpdateInputDTO partnerAPISettingsInputDTO)
        {
            UpdatePartnerApiSettingsUserCommand command = new UpdatePartnerApiSettingsUserCommand()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                Staging = partnerAPISettingsInputDTO.Staging,
                Production = partnerAPISettingsInputDTO.Production,
                inputDTO = partnerAPISettingsInputDTO
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePartnerAPISettingsUser] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Update whitelist confirmation user for Partner
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="partnerAPISettingsInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/api-settings/confirmation")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerConfirmWhitelist), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerConfirmWhitelist([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] PartnerAPISettingsConfirmWhitelistUpdateInputDTO partnerAPISettingsInputDTO)
        {
            UpdatePartnerApiSettingsConfirmWhitelistCommand command = new UpdatePartnerApiSettingsConfirmWhitelistCommand()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                InputDTO = partnerAPISettingsInputDTO
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePartnerConfirmWhitelist] {result.Error}");
                return BadRequest(result);
            }
            return Ok();
        }

        /// <summary>
        /// Get Partner Onboarding Status 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{partnerCode}/workflows")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetPartnerOnboardingStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerOnboardingOutputDTO>> GetPartnerOnboardingStatus([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerOnboardingStatusQuery query = new GetPartnerOnboardingStatusQuery
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                UserBearerToken = Request.Headers["Authorization"].ToString()
            };

            var result = await Mediator.Send(query);
            return Ok(result.Value);
        }

        /// <summary>
        /// Get Partner Onboarding Status From Connect Side
        /// </summary>
        /// <returns></returns>
        [HttpGet("{partnerCode}/partner-workflows")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetPartnerWorkflowStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerOnboardingOutputDTO>> GetPartnerWorkflowStatus([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerWorkflowStatusQuery query = new GetPartnerWorkflowStatusQuery
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PartnerOnboarding] {result.Error}");
                return ValidationProblem(result.Error);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Update Partner Onboarding Status 
        /// </summary>
        /// <returns></returns>
        [HttpPut("{partnerCode}/workflows")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerOnboarding), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerOnboarding([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] PartnerOnboardingInputDTO partnerOnboardingInputDTO)
        {
            UpdatePartnerOnboardingCommand command = new UpdatePartnerOnboardingCommand()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                UpdatePartnerOnboarding = partnerOnboardingInputDTO,
                UserBearerToken = Request.Headers["Authorization"].ToString()
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdatePartnerOnboarding] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok();

        }

        /// <summary>
        /// Get Environment Code  
        /// </summary>
        /// <returns></returns>
        [HttpGet("{partnerCode}/environment")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetEnvironmentCodeStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerOnboardingOutputDTO>> GetEnvironmentCodeStatus([PartnerCode] long partnerCode)
        {
            GetEnvironmentCodeStatusQuery query = new GetEnvironmentCodeStatusQuery
            {
                PartnerCode = partnerCode
            };
            var result = await Mediator.Send(query);

            return Ok(result.Value);
        }

        /// <summary>
        /// GET pending whitelisted APIs
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpGet("api-settings/pending-whitelist-ips")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(WhitelistIPAddressOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPendingWhitelistedIPs), Tags = new[] { "Partner" })]
        public async Task<ActionResult<List<WhitelistIPAddressOutputDTO>>> GetPendingWhitelistedIPs([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerPendingWhitelistedIPsQuery query = new GetPartnerPendingWhitelistedIPsQuery
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPendingWhitelistedIPs] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// GET pending configured callback URLs
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpGet("api-settings/pending-configured-callback-urls")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(CallbackURLOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetPendingConfiguredCallbackURLs), Tags = new[] { "Partner" })]
        public async Task<ActionResult<CallbackURLOutputDTO>> GetPendingConfiguredCallbackURLs([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerPendingConfiguredCallbackUrlsQuery query = new GetPartnerPendingConfiguredCallbackUrlsQuery
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetPendingConfiguredCallbackURLs] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Update pending whitelistIPs to IsWhitelisted
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="whitelistIPAddressInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/api-settings/whitelist-ip")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePendingWhitelistIP), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePendingWhitelistIP([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] WhitelistIPAddressInputDTO whitelistIPAddressInputDTO)
        {
            UpdatePendingWhitelistIPCommand command = new UpdatePendingWhitelistIPCommand()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                InputDTO = whitelistIPAddressInputDTO
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePendingWhitelistIP] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update pending configuration callback URL to IsConfigured
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <param name="callbackURLInputDTO"></param>
        /// <returns></returns>
        [HttpPut("{partnerCode}/api-settings/callback-url")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(UpdatePendingConfigureCallbackURL), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePendingConfigureCallbackURL([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode, [FromBody] CallbackURLInputDTO callbackURLInputDTO)
        {
            UpdatePendingConfigureCallbackURLCommand command = new UpdatePendingConfigureCallbackURLCommand()
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                InputDTO = callbackURLInputDTO
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UpdatePendingConfigureCallbackURL] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result);
        }

        #endregion

        /// <summary>
        /// Partner Request GoLive (Connect Portal) 
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/request-go-live")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerRequestGoLive), Tags = new[] { "Partner" })]
        [LogInputDTO]
        public async Task<ActionResult<PartnerRequestGoLiveOutputDTO>> PartnerRequestGoLive([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            PartnerRequestGoLiveCommand command = new PartnerRequestGoLiveCommand
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PartnerRequestGoLive] {result.Error}");
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Approve Partner Go Live (Admin)
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/go-live")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerGoLive), Tags = new[] { "Partner" })]
        [LogInputDTO]
        public async Task<ActionResult<PartnerRequestGoLiveOutputDTO>> PartnerGoLive([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            PartnerGoLiveCommand command = new PartnerGoLiveCommand
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode,
                UserBearerToken = Request.Headers["Authorization"].ToString()
            };
            var result = await Mediator.Send(command);


            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PartnerGoLive] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Approve Partner Go Live (Connect)
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/onboarding-go-live")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerOnboardingGoLive), Tags = new[] { "Partner" })]
        [LogInputDTO]
        public async Task<ActionResult<PartnerOnboardingGoLiveOutputDTO>> PartnerOnboardingGoLive(long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {

            PartnerOnboardingGoLiveCommand command = new PartnerOnboardingGoLiveCommand
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };
            var result = await Mediator.Send(command);


            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PartnerGoLive] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);
        }


        /// <summary>
        /// If the go live button has been pressed and the GoLive is currently pending in CMS
        /// 
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpGet("{partnerCode}/go-live-processed")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerGoLiveProcessStatus), Tags = new[] { "Partner" })]
        [LogInputDTO]
        public async Task<ActionResult<bool>> PartnerGoLiveProcessStatus([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            GetPartnerGoLiveIsProcessedQuery command = new GetPartnerGoLiveIsProcessedQuery
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };

            var result = await Mediator.Send(command);
            return Ok(result.Value);
        }

        /// <summary>
        /// Add or update Partner Subscriptions
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionInputDTO"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/subscription")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<SavePartnerSubscriptionInputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SaveSubscriptions), Tags = new[] { "Partner" })]
        [LogInputDTO]
        public async Task<IActionResult> SaveSubscriptions([FromRoute] long partnerCode, [FromBody] SavePartnerSubscriptionInputDTO partnerSubscriptionInputDTO)
        {
            SavePartnerSubscriptionCommand command = new SavePartnerSubscriptionCommand
            {
                PartnerCode = partnerCode,
                PartnerSubscription = partnerSubscriptionInputDTO,
                UserBearerToken = Request.Headers["Authorization"].ToString()
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[SavePartnerSubscriptionCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieve PartnerKYCStatusRequsitions CreatedBy 
        /// </summary>
        [HttpGet("created-by")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(CreatedBy), Tags = new[] { "Partner" })]
        public async Task<IEnumerable<CreatedByOutputDTO>> CreatedBy()
        {
            GetCreatedByQuery query = new GetCreatedByQuery { };

            return await Mediator.Send(query);
        }

        [HttpGet("{partnerCode}/subscription")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetSubscriptions), Tags = new[] { "Partner" })]
        [LogInputDTO]
        public async Task<ActionResult<GetPartnerSubscriptionOutputDTO>> GetSubscriptions([PartnerCode] long partnerCode, [TrangloEntityId] string trangloEntity)
        {
            GetSubscriptionListQuery query = new GetSubscriptionListQuery
            {
                PartnerCode = partnerCode,
                TrangloEntity = trangloEntity,
                UserBearerToken = Request.Headers["Authorization"].ToString()
            };
            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetSubscriptionListQuery] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        [HttpGet("{partnerCode}/sales-partner-subscription")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetSubscriptionsCodeList), Tags = new[] { "Partner" })]
        public async Task<ActionResult<GetSalesPartnerSubscriptionOutputDTO>> GetSubscriptionsCodeList([PartnerCode] long partnerCode)
        {
            GetPartnerSubscriptionListQuery query = new GetPartnerSubscriptionListQuery
            {
                PartnerCode = partnerCode
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetSubscriptionsCodeList] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }


        [HttpGet("{partnerCode}/sales-partner-subscription/entity-list")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetSubscriptionsEntityRoles), Tags = new[] { "Partner" })]
        public async Task<ActionResult<GetSalesPartnerSubscriptionOutputDTO>> GetSubscriptionsEntityRoles([PartnerCode] long partnerCode, [FromQuery] string entity)
        {
            GetSubscriptionsEntityRolesQuery query = new GetSubscriptionsEntityRolesQuery
            {
                PartnerCode = partnerCode,
                Entity = entity,
                LoginId = User.GetSubjectId().Value
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetSubscriptionsEntityRoles] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        #region terms and conditions
        /// <summary>
        /// Get terms and conditions acceptance status
        /// </summary>
        /// <returns></returns>
        [HttpGet("terms-and-conditions-acceptance-status")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetPartnerTermsAndConditionAcceptanceStatus), Tags = new[] { "Partner" })]
        public async Task<ActionResult<bool>> GetPartnerTermsAndConditionAcceptanceStatus([PartnerCode] long partnerCode, [FromQuery] string roleCode, DateTime termsAcceptanceDate)

        {
            GetTermsAndConditionsAcceptanceStatusQuery query = new GetTermsAndConditionsAcceptanceStatusQuery
            {
                PartnerCode = partnerCode,
                RoleCode = roleCode,
                TermsAcceptanceDate = termsAcceptanceDate
            };

            var result = await Mediator.Send(query);
            return Ok(result.Value);
        }

        /// <summary>
        /// Update terms and conditions acceptance date
        /// </summary>
        /// <return></return>
        [HttpPut("terms-and-conditions-acceptance-status")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(UpdatePartnerTermsAndConditionsDate), Tags = new[] { "Partner" })]
        public async Task<IActionResult> UpdatePartnerTermsAndConditionsDate([PartnerCode] long partnerCode)
        {
            UpdatePartnerTermsAndConditionsAcceptanceDateCommand command = new UpdatePartnerTermsAndConditionsAcceptanceDateCommand()
            {
                PartnerCode = partnerCode,
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[UpdatePartnerTermsAndConditionsDate] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }
        #endregion

        #region external partner subscription list
        /// <summary>
        /// Get external partner subscriptions  by partnerCode and solutionCode
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <returns></returns>
        [HttpGet("external-partner-subscriptions/{partnerCode}/partner-subscriptions")]
        [Authorize(Policy = AuthenticationPolicies.ExternalOnlyPolicy)]
        [ProducesResponseType(typeof(ExternalPartnerSubsciptionListOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetExternalPartnerSubscriptions), Tags = new[] { "Partner" })]
        public async Task<ActionResult<ExternalPartnerSubsciptionListOutputDTO>> GetExternalPartnerSubscriptions([PartnerCode] long partnerCode)
        {
            GetAllExternalPartnerSubscriptionsQuery query = new GetAllExternalPartnerSubscriptionsQuery()
            {
                PartnerCode = partnerCode
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.ExternalSolution = solution.HasValue ? solution.Value : null;
            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[GetAllExternalPartnerSubscriptionsQuery] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }
        #endregion

        /// <summary>
        /// Get partner name list by trangloEntity 
        /// </summary>
        [HttpGet("partner-name-list-by-trangloentity")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerNameByTrangloEntities), Tags = new[] { "Partner" })]
        public async Task<ActionResult<PartnerNameListbyTrangloEntityOutputDTO>> PartnerNameByTrangloEntities(string trangloEntity)
        {
            GetPartnerNameListByTrangloEntityQuery query = new GetPartnerNameListByTrangloEntityQuery { };

            query.TrangloEntity = trangloEntity;

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[PartnerNameByTrangloEntities] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Request 2FA Email Authentication OTP
        /// </summary>
        /// <returns></returns>
        [HttpPost("request-email-authentication-otp")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(RequestEmailOTPCode), Tags = new[] { "2FA" })]

        public async Task<IActionResult> RequestEmailOTPCode()
        {

            var user = User.GetSubjectId().Value;

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                RequestEmailAuthenticationOTPCommand command = new RequestEmailAuthenticationOTPCommand
                {
                    LoginId = user,
                };

                var result = await Mediator.Send(command);

                if (result.IsFailure)
                {
                    ModelState.AddModelError("Error", result.Error);
                    _logger.LogError($"[MFAEmail] {result.Error}");
                    return ValidationProblem(result.Error);
                }
                return Ok(result.Value);
            }

        }

        /// <summary>
        /// Validate Request 2FA Email Authentication OTP
        /// </summary>
        /// <returns></returns>

        [HttpPost("validate-email-authentication-otp")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(ValidateEmailOTPCode), Tags = new[] { "2FA" })]

        public async Task<IActionResult> ValidateEmailOTPCode(string mfaOTP)
        {
            ValidateEmailAuthenticationOTPQuery query = new ValidateEmailAuthenticationOTPQuery
            {
                MFAOTP = mfaOTP
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[ValidateMFAOTP] {result.Error}");
                return ValidationProblem(result.Error);
            }
            return Ok(result.Value);

        }

        /// <summary>
        /// Get Partner List in Excel
        /// </summary>
        /// <param name="trangloEntity"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("partner-export")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(ExportPartnerListing), Tags = new[] { "Partner" })]
        public async Task<ActionResult> ExportPartnerListing([TrangloEntityId] string trangloEntity, [FromBody] GetPartnerListExportInputDTO input)
        {
            GetPartnerListingExportQuery query = new GetPartnerListingExportQuery
            {
                Entity = trangloEntity,
                InputDTO = input,
                AdminSolution = input.Search.AdminSolution,
                UserBearerToken = Request.Headers["Authorization"].ToString(),
            };

            var solution = ClaimsPrincipalExtensions.GetSolutionCode(User);
            query.CustomerSolution = solution.HasValue ? solution.Value : null;

            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                return File(result.Value.File, result.Value.ContentType, result.Value.FileName);
            }

            return NotFound();
        }
    }
}

