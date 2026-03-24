using Microsoft.Extensions.Configuration;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.MediatR.Behaviours;
using Tranglo1.Onboarding.Application.Common.Exceptions;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCApprovalHistory, UACAction.View)]
    [Permission(Permission.KYCApprovalHistory.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {  })]
    internal class GetBusinessProfilesApproveHistoryQuery : BaseQuery<PagedResult<BusinessProfileApprovalHistoryOutputDTO>>
    {
        public string PartnerName { set; get; }
        public string CountryISO2 { get; set; }
        public string ComplianceOfficerLoginId { get; set; }
        public string FromRegistrationDate { set; get; }
        public string ToRegistrationDate { set; get; }
        public string FromApproveDate { set; get; }
        public string ToApproveDate { set; get; }
        public string FromRejectDate { set; get; }
        public string ToRejectDate { set; get; }
        public string EntityCode { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();

        public override Task<string> GetAuditLogAsync(PagedResult<BusinessProfileApprovalHistoryOutputDTO> result)
        {
            string _description = $"Searched KYC Approval History records";
            return Task.FromResult(_description);
        }

        public class GetBusinessProfilesApproveHistoryQueryHandler : IRequestHandler<GetBusinessProfilesApproveHistoryQuery, PagedResult<BusinessProfileApprovalHistoryOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly IIdentityContext identityContext;
            private readonly TrangloUserManager _userManager;
            private IApplicationUserRepository _applicationUserRepository;

            public GetBusinessProfilesApproveHistoryQueryHandler(IConfiguration config, IIdentityContext identityContext, TrangloUserManager userManager, IApplicationUserRepository applicationUserRepository
            )
            {
                _config = config;
                this.identityContext = identityContext;
                _userManager = userManager;
                _applicationUserRepository = applicationUserRepository;
            }

            public async Task<PagedResult<BusinessProfileApprovalHistoryOutputDTO>> Handle(GetBusinessProfilesApproveHistoryQuery request, CancellationToken cancellationToken)
            {
                var _CurrentUser = identityContext.CurrentUser;
                var _Sub = _CurrentUser.GetSubjectId();
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



                PagedResult<BusinessProfileApprovalHistoryOutputDTO> result = new PagedResult<BusinessProfileApprovalHistoryOutputDTO>();
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var _sortExpression = string.IsNullOrEmpty(request.PagingOptions.SortExpression) ? "" : request.PagingOptions.SortExpression + " " + (request.PagingOptions.Direction == SortDirection.Ascending ? "" : "DESC");
                    var reader = await connection.QueryMultipleAsync(
                        "GetAdminKYCBusinessProfile",
                        new
                        {
                            PageIndex = request.PagingOptions.PageIndex,
                            PageSize = request.PagingOptions.PageSize,
                            Name = !string.IsNullOrWhiteSpace(request.PartnerName) ? string.Format("%{0}%", request.PartnerName) : null,
                            country = !string.IsNullOrWhiteSpace(request.CountryISO2) ? string.Format("%{0}%", request.CountryISO2) : null,
                            complianceOfficerLoginId = !string.IsNullOrWhiteSpace(request.ComplianceOfficerLoginId) ? request.ComplianceOfficerLoginId : null,
                            fromRegistrationDate = request.FromRegistrationDate,
                            toRegistrationDate = request.ToRegistrationDate,
                            fromApproveDate = request.FromApproveDate,
                            toApproveDate = request.ToApproveDate,
                            fromRejectDate = request.FromRejectDate,
                            toRejectDate = request.ToRejectDate,
                            sortExpression = _sortExpression,
                            EntityCode = request.EntityCode
                        },
                        null, null, CommandType.StoredProcedure);
                    result.Results = await reader.ReadAsync<BusinessProfileApprovalHistoryOutputDTO>();
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
