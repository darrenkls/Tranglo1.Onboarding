using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.Common.Exceptions;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.MediatR.Behaviours;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.Compliance, UACAction.View)]
    [Permission(Permission.Compliance.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class GetKYCWatchlistQuery : BaseQuery<PagedResult<KYCWatchListReviewOutputDTO>>
    {
        public int? BusinessProfileCode { get; set; }
        public int? OwnershipStructureTypeCode { get; set; }
        public int? ScreeningEntityTypeCodeFilter { get; set; }
        public string FullName { get; set; }
        public string CountryISO2 { get; set; }
        public int? ScreeningTypeId { get; set; }
        public int? WatchlistStatusCode { get; set; }
        public string ComplianceOfficerId { get; set; }
        public string ScreeningStartDate { get; set; }
        public string ScreeningEndDate { get; set; }
        public string LastReviewedDateFrom { get; set; }
        public string LastReviewedDateTo { get; set; }
        public int? ScreeningType { get; set; }
        public string EntityCode { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();

        public override Task<string> GetAuditLogAsync(PagedResult<KYCWatchListReviewOutputDTO> result)
        {
            string _description = $"Searched Personnel Watchlist";
            return Task.FromResult(_description);
        }

        public class GetKYCWatchlistQueryHandler : IRequestHandler<GetKYCWatchlistQuery, PagedResult<KYCWatchListReviewOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly IIdentityContext identityContext;
            private readonly TrangloUserManager _userManager;
            private  IApplicationUserRepository _applicationUserRepository;


            public GetKYCWatchlistQueryHandler(IConfiguration config, IIdentityContext identityContext, TrangloUserManager userManager, IApplicationUserRepository applicationUserRepository)
            {
                _config = config;
                this.identityContext = identityContext;
                _userManager = userManager;
                _applicationUserRepository = applicationUserRepository;
            }
            public async Task<PagedResult<KYCWatchListReviewOutputDTO>> Handle(GetKYCWatchlistQuery request, CancellationToken cancellationToken)
            {
                var _CurrentUser = identityContext.CurrentUser;
                var _SubResult = System.Security.Claims.ClaimsPrincipalExtensions.GetSubjectId(_CurrentUser);
                var _Sub = _SubResult.HasValue ? _SubResult.Value : null;
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(_Sub);
                if (applicationUser is TrangloStaff trangloStaff)
                {
                    var trangloEntities = await _applicationUserRepository.GetTrangloStaffEntityAssignmentByUserId(trangloStaff.Id);
                    var TrangloEntityResult = EntityVerificationBehavior.TrangloEntityChecking(trangloEntities, request.EntityCode);
                    if (TrangloEntityResult.IsFailure)
                    {
                        throw new ForbiddenException($"User account: {_Sub} is not authorized to access Tranglo Entity {request.EntityCode}");
                    }
                }



                PagedResult<KYCWatchListReviewOutputDTO> result = new PagedResult<KYCWatchListReviewOutputDTO>();
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                var _sortExpression = string.IsNullOrEmpty(request.PagingOptions.SortExpression) ? "" : request.PagingOptions.SortExpression + " " + (request.PagingOptions.Direction == SortDirection.Ascending ? "" : "DESC");

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                       "GetKYCWatchlistReview",
                       new
                       {
                           BusinessProfileCodeFilter = request.BusinessProfileCode,
                           OwnershipStructureType = request.OwnershipStructureTypeCode,
                           ScreeningEntityTypeCodeFilter = request.ScreeningEntityTypeCodeFilter,
                           CountryISO2Filter = request.CountryISO2,
                           WatchlistStatusCode = request.WatchlistStatusCode,
                           ComplianceOfficerId = request.ComplianceOfficerId,
                           ScreeningTypeId = request.ScreeningTypeId,
                           ScreeningStartDate = request.ScreeningStartDate,
                           ScreeningEndDate = request.ScreeningEndDate,
                           LastReviewedDateFrom = request.LastReviewedDateFrom,
                           LastReviewedDateTo = request.LastReviewedDateTo,
                           FullName = request.FullName,
                           PageSize = request.PagingOptions.PageSize,
                           PageIndex = request.PagingOptions.PageIndex,
                           SortExpression = _sortExpression,
                           EntityCode = request.EntityCode
                       },
                       null, null, CommandType.StoredProcedure);

                    result.Results = await reader.ReadAsync<KYCWatchListReviewOutputDTO>();
                    IEnumerable<PaginationInfoDTO> _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();
                    result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                    result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                    result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;


                }
                return result;
            }

        }
        }
}
