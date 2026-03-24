using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.DTO.Watchlist;
using Tranglo1.Onboarding.Application.Helper;
using Tranglo1.Onboarding.Application.Infrastructure.Swagger;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Security;

namespace Tranglo1.Onboarding.Application.Controllers.Watchlist
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/")]
    [LogInputDTO]
    [LogOutputDTO]
    public class WatchlistController : ControllerBase
    {
        private readonly ILogger<WatchlistController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        //Watchlist
        public WatchlistController(ILogger<WatchlistController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost("watchlist/notification")]
        [ApiKeyAuthentication]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(NotifyUser), Tags = new[] { "Watchlist Review" })]
        public async Task<IActionResult> NotifyUser(WatchlistNotificationInputDTO inputDTO)
        {
            NotifyWatchlistUserCommand query = new NotifyWatchlistUserCommand
            {
                InputDTO = inputDTO
            };

            var result = await Mediator.Send(query);

            if (result.IsFailure)
            {
                _logger.LogError("[{MethodName}] {Error}", ExtensionHelper.GetMethodName(), result.Error);
                ModelState.AddModelError("Error", result.Error);
                return ValidationProblem();
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get KYC Watchlist Details
        /// </summary>
        /// <param name="referenceCode"></param>
        /// <returns></returns>
        [HttpGet("screening-results/{referenceCode}")]
        [ProducesResponseType(typeof(List<GetEntityDetailByReferenceCodeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCWatchlistDetails), Tags = new[] { "Watchlist Review" })]
        public async Task<ActionResult<List<GetEntityDetailByReferenceCodeResponse>>> GetKYCWatchlistDetails(Guid referenceCode)
        {
            GetKYCWatchlistDetailsQuery query = new GetKYCWatchlistDetailsQuery
            {
                ReferenceCode = referenceCode
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError("[{MethodName}] {Error}", ExtensionHelper.GetMethodName(), result.Error);
                return ValidationProblem();
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get KYC Watchlist Details By EntityID
        /// </summary>
        /// <param name="referenceCode"></param>
        /// <param name="entityId"></param>
        /// <param name="listSource"></param>
        /// <returns></returns>
        [HttpGet("screening-results/{referenceCode}/details/{entityId}/sources/{listSource}")]
        [ProducesResponseType(typeof(GetEntityDetailByReferenceCodeAndEntityIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCWatchlistDetailsByEntityId), Tags = new[] { "Watchlist Review" })]
        public async Task<ActionResult<GetEntityDetailByReferenceCodeAndEntityIdResponse>> GetKYCWatchlistDetailsByEntityId(Guid referenceCode, long entityId, int listSource)
        {
            GetKYCWatchlistDetailsByEntityIdQuery query = new GetKYCWatchlistDetailsByEntityIdQuery
            {
                ReferenceCode = referenceCode,
                EntityId = entityId,
                ListSource = listSource
            };

            var result = await Mediator.Send(query);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError("[{MethodName}] {Error}", ExtensionHelper.GetMethodName(), result.Error);
                return ValidationProblem();
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get KYC Watchlist Review
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="fullName"></param>
        /// <param name="ownershipStructureCode"></param>
        /// <param name="screeningEntityTypeCode"></param>
        /// <param name="countryISO2"></param>
        /// <param name="screeningTypeId"></param>
        /// <param name="watchlistStatusCode"></param>
        /// <param name="complianceOfficerLoginId"></param>
        /// <param name="screeningStartDate"></param>
        /// <param name="screeningEndDate"></param>
        /// <param name="lastReviewedDateFrom"></param>
        /// <param name="lastReviewedDateTo"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="entityCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("watchlists")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(PagedResult<KYCWatchListReviewOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(KYCWatchlistReview), Tags = new[] { "Watchlist Review" })]
        public async Task<PagedResult<KYCWatchListReviewOutputDTO>> KYCWatchlistReview(int? businessProfileCode, string fullName,
          int? ownershipStructureCode, int? screeningEntityTypeCode, string countryISO2, int? screeningTypeId,
          int? watchlistStatusCode, string complianceOfficerLoginId, string screeningStartDate, string screeningEndDate, string lastReviewedDateFrom,
          string lastReviewedDateTo, int pageSize, int pageIndex, string sortExpression, SortDirection sortDirection, [TrangloEntityId] string entityCode)
        {
            GetKYCWatchlistQuery query = new GetKYCWatchlistQuery
            {
                BusinessProfileCode = businessProfileCode,
                OwnershipStructureTypeCode = ownershipStructureCode,
                ScreeningEntityTypeCodeFilter = screeningEntityTypeCode,
                CountryISO2 = countryISO2,
                WatchlistStatusCode = watchlistStatusCode,
                ComplianceOfficerId = complianceOfficerLoginId,
                ScreeningTypeId = screeningTypeId,
                ScreeningStartDate = screeningStartDate,
                ScreeningEndDate = screeningEndDate,
                LastReviewedDateFrom = lastReviewedDateFrom,
                LastReviewedDateTo = lastReviewedDateTo,
                FullName = fullName,
                EntityCode = entityCode,
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
        /// Get Customer Information Screening
        /// </summary>
        /// <param name="screeningInputCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("watchlists/{screeningInputCode}/profile-details")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(KYCWatchlistProfileOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(KYCWatchlistProfile), Tags = new[] { "Watchlist Review" })]
        public async Task<KYCWatchlistProfileOutputDTO> KYCWatchlistProfile(long screeningInputCode)
        {
            GetKYCWatchlistProfileQuery query = new GetKYCWatchlistProfileQuery
            {
                ScreeningInputCode = screeningInputCode
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Get Customer Screening Counter
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="ownershipStructureType"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("screening-counters")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ScreeningCounterOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetScreeningCounters), Tags = new[] { "Watchlist Review" })]
        public async Task<ScreeningCounterOutputDTO> GetScreeningCounters(int businessProfileCode, int ownershipStructureType, int tableId)
        {
            GetScreeningCountersQuery query = new GetScreeningCountersQuery
            {
                BusinessProfileCode = businessProfileCode,
                OwnershipStructureType = ownershipStructureType,
                TableId = tableId
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Get Customer RBA Counter
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <param name="ownershipStructureType"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("rba-counters")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(RBACounterOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(GetRBACounters), Tags = new[] { "Watchlist Review" })]
        public async Task<RBACounterOutputDTO> GetRBACounters(int businessProfileCode, int ownershipStructureType, int tableId)
        {
            GetRBACountersQuery query = new GetRBACountersQuery
            {
                BusinessProfileCode = businessProfileCode,
                OwnershipStructureType = ownershipStructureType,
                TableId = tableId
            };
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Review True Hit result on each watchlist item
        /// </summary>
        /// <param name="screeningInputCode"></param>
        /// <param name="watchlistReviewInputDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("watchlists/{screeningInputCode}/reviews")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(WatchlistReviewOutputDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = nameof(SaveWatchlistReview), Tags = new[] { "Watchlist Review" })]
        public async Task<IActionResult> SaveWatchlistReview(long screeningInputCode, WatchlistReviewInputDTO watchlistReviewInputDTO)
        {
            SaveWatchlistReviewCommand command = new SaveWatchlistReviewCommand
            {
                ScreeningInputCode = screeningInputCode,
                InputDTO = watchlistReviewInputDTO
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[SaveWatchlistReview] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result.Value);
        }


        /// <summary>
        /// Upload file(s) to support watchlist review action
        /// </summary>
        /// <param name="screeningInputCode"></param>
        /// <param name="watchlistReviewCode"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestSizeLimit(40000000)]
        [Route("watchlists/{screeningInputCode}/reviews/{watchlistReviewCode}/documents")]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [FileParameter(Description = "Add watchlist review document(s)", Required = false)]
        [SwaggerOperation(OperationId = nameof(UploadWatchlistReviewDocuments), Tags = new[] { "Watchlist Review" })]
        public async Task<ActionResult> UploadWatchlistReviewDocuments(long screeningInputCode, long watchlistReviewCode, IFormFile[] uploadedFile)
        {
            UploadWatchlistReviewDocumentsCommand command = new UploadWatchlistReviewDocumentsCommand()
            {
                WatchlistReviewCode = watchlistReviewCode,
                uploadedFiles = uploadedFile
            };

            var result = await Mediator.Send(command);
            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);

                _logger.LogError($"[UploadWatchlistReviewDocuments] {result.Error}");
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get KYC Watchlist Details History of Previous Actions
        /// </summary>
        /// <param name="screeningInputCode"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortExpression"></param>
        /// <returns></returns>
        [HttpGet("watchlists/{screeningInputCode}/reviews")]
        [ProducesResponseType(typeof(PagedResult<KYCWatchlistDetailsHistoryOutputDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthenticationPolicies.InternalOnlyPolicy)]
        [SwaggerOperation(OperationId = nameof(GetKYCWatchlistDetailsHistory), Tags = new[] { "Watchlist Review" })]
        public async Task<PagedResult<KYCWatchlistDetailsHistoryOutputDTO>> GetKYCWatchlistDetailsHistory(int screeningInputCode, int pageIndex, int pageSize, string sortExpression)
        {
            GetKYCWatchlistDetailsHistoryQuery query = new GetKYCWatchlistDetailsHistoryQuery
            {
                ScreeningInputCode = screeningInputCode,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortExpression = sortExpression,
            };

            var result = await Mediator.Send(query);
            return result;
        }

        /// <summary>
        /// Download documents by document id
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("documents/{documentId}")]
        [FileResponse]
        [SwaggerOperation(OperationId = nameof(GetDocumentByDocumentId), Tags = new[] { "Watchlist Review" })]
        [ProducesResponseType(typeof(DocumentsDownloadDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetDocumentByDocumentId(Guid documentId)
        {
            GetDocumentsDownloadByDocIdQuery query = new GetDocumentsDownloadByDocIdQuery
            {
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
    }
}
