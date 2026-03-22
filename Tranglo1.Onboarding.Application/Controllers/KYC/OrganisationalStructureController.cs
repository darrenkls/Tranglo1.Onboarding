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
using Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary;
using Tranglo1.Onboarding.Application.DTO.BoardofDirector;
using Tranglo1.Onboarding.Application.DTO.LegalEntitiy;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.AuthorisedPerson;
using Tranglo1.Onboarding.Application.DTO.ParentHoldingCompany;
using Tranglo1.Onboarding.Application.DTO.PoliticallyExposedPerson;
using Tranglo1.Onboarding.Application.DTO.PrimaryOfficer;
using Tranglo1.Onboarding.Application.DTO.Shareholder;
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
    public class OrganisationalStructureController : ControllerBase
    {
        private readonly ILogger<OrganisationalStructureController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public OrganisationalStructureController(ILogger<OrganisationalStructureController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        #region GET API's for Organisational Structure 
        /// <summary>
        /// Retrieve Shareholders per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/shareholders")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<ShareholderOutputDTO>), StatusCodes.Status200OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(OperationId = nameof(GetShareholders), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<ShareholderOutputDTO>> GetShareholders([BusinessProfileId] int businessProfileCode, [FromQuery] long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);

            GetShareholderByIdQuery query = new GetShareholderByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Ultimate Beneficial Owners per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/ultimate-beneficial-owners")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<LegalEntitiyOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetUltimateBeneficialOwners), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<LegalEntitiyOutputDTO>> GetUltimateBeneficialOwners([BusinessProfileId] int businessProfileCode, [FromQuery] long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);

            GetUltimateBeneficialOwnersByCode query = new GetUltimateBeneficialOwnersByCode
            {
                BusinessProfileCode = businessProfileCode,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
            };

            return await Mediator.Send(query);
        }


        /// <summary>
        /// Retrieve Affiliate and Subsidiaries per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/affliates-subsidiaries")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AffiliateAndSubsidiaryOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetAffiliateAndSubsidiaries), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<AffiliateAndSubsidiaryOutputDTO>> GetAffiliateAndSubsidiaries([BusinessProfileId] int businessProfileCode)
        {
            GetAffiliateAndSubsidiaryByIdQuery query = new GetAffiliateAndSubsidiaryByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }


        /// <summary>
        /// Retrieve Parent Holding Companies per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [Obsolete(message: "Parent holdings concept is no longer in use", error: true)]
        [HttpGet("{businessProfileCode}/parent-holdings")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<ParentHoldingCompanyOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetParentHoldingCompanies), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<ParentHoldingCompanyOutputDTO>> GetParentHoldingCompanies([BusinessProfileId] int businessProfileCode)
        {
            GetParentHoldingCompanyByIdQuery query = new GetParentHoldingCompanyByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Board of Directors per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/board-of-directors")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<BoardofDirectorOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetBoardOfDirector), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<BoardofDirectorOutputDTO>> GetBoardOfDirector([BusinessProfileId] int businessProfileCode)
        {
            GetBoardOfDirectorByIdQuery query = new GetBoardOfDirectorByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Primary Officers per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/primary-officers")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<PrimaryOfficerOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetPrimaryOfficers), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<PrimaryOfficerOutputDTO>> GetPrimaryOfficers([BusinessProfileId] int businessProfileCode)
        {
            GetPrimaryOfficerByIdQuery query = new GetPrimaryOfficerByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }

        /// <summary>
        /// Retrieve Politicaly Exposed Persons (PEPs) per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [Obsolete(message: "Politically Exposed Person (PEP) concept is no longer in use", error: true)]
        [HttpGet("{businessProfileCode}/politically-exposed-person")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<PoliticallyExposedPersonOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetPoliticallyExposedPersons), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<PoliticallyExposedPersonOutputDTO>> GetPoliticallyExposedPersons([BusinessProfileId] int businessProfileCode)
        {
            GetPoliticallyExposedPersonByIdQuery query = new GetPoliticallyExposedPersonByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }


        /// <summary>
        /// Retrieve Authorised Persons per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/authorised-person")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AuthorisedPersonOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetAuthorisedPersonByBusinessProfileCode), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<AuthorisedPersonOutputDTO>> GetAuthorisedPersonByBusinessProfileCode([BusinessProfileId] int businessProfileCode, [FromQuery] long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);

            GetAuthorisedPersonByIdQuery query = new GetAuthorisedPersonByIdQuery
            {
                BusinessProfileCode = businessProfileCode,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
            };

            return await Mediator.Send(query);
        }


        /// <summary>
        /// Retrieve Ownership Concurrency per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpGet("{businessProfileCode}/ownerships-concurrencies-token")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<OwnershipConcurrencyTokenOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(GetOwnershipConcurrencyTokenByBusinessProfileCode), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IEnumerable<OwnershipConcurrencyTokenOutputDTO>> GetOwnershipConcurrencyTokenByBusinessProfileCode([BusinessProfileId] int businessProfileCode)
        {
            GetOwnershipConcurrencyTokenByIdQuery query = new GetOwnershipConcurrencyTokenByIdQuery
            {
                BusinessProfileCode = businessProfileCode
            };

            return await Mediator.Send(query);
        }
        #endregion

        #region POST API's for Organisational Structure

        /// <summary>
        /// Add or update Affiliate and Subsidiaries per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="affiliateAndSubsidiaryDtos"></param>
        /// <param name="affiliatesAndSubsidiariesConcurrencyToken"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/affliates-subsidiaries")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AffiliateAndSubsidiaryOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SaveAffliatesSubsidiaries), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SaveAffliatesSubsidiaries([BusinessProfileId] int businessProfileCode, 
            [FromBody] IEnumerable<AffiliateAndSubsidiaryInputDTO> affiliateAndSubsidiaryDtos,Guid? affiliatesAndSubsidiariesConcurrencyToken, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);

            SaveAffliateAndSubsidiaryCommand command = new SaveAffliateAndSubsidiaryCommand
            {
                BusinessProfileCode = businessProfileCode,
                AffiliateAndSubsidiaries = affiliateAndSubsidiaryDtos,
                AffiliatesAndSubsidiariesConcurrencyToken = affiliatesAndSubsidiariesConcurrencyToken,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = subjectId.HasValue ? subjectId.Value : null
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }


        /// <summary>
        /// Add or update Board of Directors per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="boardOfDirectorDtos"></param>
        /// <param name="boardOfDirectorConcurrencyToken"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/board-of-directors")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<BoardofDirectorOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SaveBoardOfDirectors), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SaveBoardOfDirectors([BusinessProfileId] int businessProfileCode, 
            [FromBody] IEnumerable<BoardofDirectorInputDTO> boardOfDirectorDtos, Guid? boardOfDirectorConcurrencyToken, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);

            SaveBoardOfDirectorCommand command = new SaveBoardOfDirectorCommand
            {
                BusinessProfileCode = businessProfileCode,
                BoardOfDirectors = boardOfDirectorDtos,
                BoardOfDirectorConcurrencyToken = boardOfDirectorConcurrencyToken,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = subjectId.HasValue ? subjectId.Value : null
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Add or update Legal Entities per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="legalEntityDtos"></param>
        /// <param name="legalEntityConcurrencyToken"></param>
        /// <param name="adminSolution"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/ultimate-beneficial-owners")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<LegalEntitiyOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SaveUltimateBeneficialOwners), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SaveUltimateBeneficialOwners([BusinessProfileId] int businessProfileCode, 
            [FromBody] IEnumerable<LegalEntitiyInputDTO> legalEntityDtos, Guid? legalEntityConcurrencyToken, long? adminSolution)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);

            SaveUltimateBeneficialOwnerCommand command = new SaveUltimateBeneficialOwnerCommand
            {
                BusinessProfileCode = businessProfileCode,
                LegalEntities = legalEntityDtos,
                LegalEntityConcurrencyToken = legalEntityConcurrencyToken,
                AdminSolution = adminSolution,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                LoginId = subjectId.HasValue ? subjectId.Value : null
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Add or update Shareholders per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="shareholderDtos"></param>
        /// <param name="adminSolution"></param>
        /// <param name="shareholderConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/shareholders")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<ShareholderOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveShareholders), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SaveShareholders([BusinessProfileId] int businessProfileCode,
            [FromBody] IEnumerable<ShareholderInputDTO> shareholderDtos, long? adminSolution, Guid? shareholderConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string
            var subjectId = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(User);

            SaveShareholderCommand command = new SaveShareholderCommand
            {
                BusinessProfileCode = businessProfileCode,
                Shareholders = shareholderDtos,
                LoginId = subjectId.HasValue ? subjectId.Value : null,
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                ShareholderConcurrencyToken = shareholderConcurrencyToken 
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }


        /// <summary>
        /// Add or update Parent Holding Companies per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="parentHoldingCompanyDtos"></param>
        /// <param name="adminSolution"></param>
        /// <param name="parentHoldingsConcurrencyToken"></param>
        /// <returns></returns>
        [Obsolete(message: "Parent holdings concept is no longer in use", error: true)]
        [HttpPost("{businessProfileCode}/parent-holdings")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<ParentHoldingCompanyOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SaveParentHoldingCompanies), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SaveParentHoldingCompanies([BusinessProfileId] int businessProfileCode,
            [FromBody] IEnumerable<ParentHoldingCompanyInputDTO> parentHoldingCompanyDtos, long? adminSolution, Guid? parentHoldingsConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SaveParentHoldingCompanyCommand command = new SaveParentHoldingCompanyCommand
            {
                BusinessProfileCode = businessProfileCode,
                ParentHoldingCompany = parentHoldingCompanyDtos,
                LoginId = User.GetSubjectId(),
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                ParentHoldingsConcurrencyToken = parentHoldingsConcurrencyToken
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Add or update Politically Exposed Persons (PEPs) per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="politicallyExposedPersonDtos"></param>
        /// <param name="adminSolution"></param>
        /// <param name="politicalExposedPersonsConcurrencyToken"></param>
        /// <returns></returns>
        [Obsolete(message: "Politically Exposed Person (PEP) concept is no longer in use", error: true)]
        [HttpPost("{businessProfileCode}/politically-exposed-person")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<PoliticallyExposedPersonOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SavePoliticallyExposedPersons), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SavePoliticallyExposedPersons([BusinessProfileId] int businessProfileCode, 
            [FromBody] IEnumerable<PoliticallyExposedPersonInputDTO> politicallyExposedPersonDtos, long? adminSolution, Guid? politicalExposedPersonsConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SavePoliticallyExposedPersonCommand command = new SavePoliticallyExposedPersonCommand
            {
                BusinessProfileCode = businessProfileCode,
                PoliticallyExposedPersons = politicallyExposedPersonDtos,
                LoginId = User.GetSubjectId(),
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                PoliticalExposedPersonsConcurrencyToken = politicalExposedPersonsConcurrencyToken
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Add or update Primary Officers per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="primaryOfficerDtos"></param>
        /// <param name="adminSolution"></param>
        /// <param name="primaryOfficerConcurrencyToken"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/primary-officers")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<PrimaryOfficerOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SavePrimaryOfficers), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SavePrimaryOfficers([BusinessProfileId] int businessProfileCode, 
            [FromBody] IEnumerable<PrimaryOfficerInputDTO> primaryOfficerDtos, long? adminSolution, Guid? primaryOfficerConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User); // convert Maybe<string> to string

            SavePrimaryOfficerCommand command = new SavePrimaryOfficerCommand
            {
                BusinessProfileCode = businessProfileCode,
                PrimaryOfficers = primaryOfficerDtos,
                LoginId = User.GetSubjectId(),
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                PrimaryOfficerConcurrencyToken = primaryOfficerConcurrencyToken
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Add or update Authorised Persons per BusinessProfileCode
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="authorisedPersonInputDTOs"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/authorised-person")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AuthorisedPersonOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(SaveAuthorisedPersonCommand), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> SaveAuthorisedPersonCommand([BusinessProfileId] int businessProfileCode,
            [FromBody] IEnumerable<AuthorisedPersonInputDTO> authorisedPersonInputDTOs, long? adminSolution, Guid? authorisedPersonConcurrencyToken)
        {
            var solution = System.Security.Claims.ClaimsPrincipalExtensions.GetSolutionCode(User);
            SaveAuthorisedPersonCommand command = new SaveAuthorisedPersonCommand
            {
                BusinessProfileCode = businessProfileCode,
                AuthorisedPeople = authorisedPersonInputDTOs,
                LoginId = User.GetSubjectId(),
                CustomerSolution = solution.HasValue ? solution.Value : null,
                AdminSolution = adminSolution,
                AuthorisedPersonConcurrencyToken = authorisedPersonConcurrencyToken
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
                _logger.LogError($"[SaveTransactionEvaluationCommand] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Validation overall ownership
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns></returns>
        [HttpPost("{businessProfileCode}/overall-ownership")]
        [Authorize(Policy = AuthenticationPolicies.InternalOrExternalPolicy)]
        [ProducesResponseType(typeof(IEnumerable<AuthorisedPersonOutputDTO>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(ValidateOverallOwnershipCommand), Tags = new[] { "KYC (Know Your Customer) - Organisational Structure" })]
        public async Task<IActionResult> ValidateOverallOwnershipCommand([BusinessProfileId] int businessProfileCode)
        {
            ValidateOverallOwnershipCommand command = new ValidateOverallOwnershipCommand
            {
                BusinessProfileCode = businessProfileCode
            };

            var result = await Mediator.Send(command);

            return Ok(result);
        }
        #endregion
    }
}
