using Dapper;
using MediatR;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Microsoft.Extensions.Configuration;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.MediatR.Behaviours;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Exceptions;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCManagement, UACAction.View)]
    [Permission(Permission.KYCManagement.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]

    internal class GetKYCManagementQuery : BaseQuery<PagedResult<KYCManagementOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string PartnerName { get; set; }
        public string CountryISO2 { get; set; }
        public string CountryDescription { get; set; }
        public string ComplianceOfficerAssignedLoginId { get; set; }
        public string ComplianceOfficerAssignedName { get; set; }
        public string fromRegistrationDate { get; set; }
        public string toRegistrationDate { get; set; }
        public long? WorkFlowStatusCode { get; set; }
        public long? CustomerTypeCode { get; set; }
        public string WorkFlowStatusDescription { get; set; }
        public long KYCStatusCode { get; set; }
        public string KYCStatusDescription { get; set; }
        public string EntityCode { get; set; }
        public long SolutionCode { get; set; }
        public string FullRegisteredCompanyLegalName { get; set; }
        public int IsComplianceTeamReview { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();

        public override Task<string> GetAuditLogAsync(PagedResult<KYCManagementOutputDTO> result)
        {
            string _description = $"Searched KYC records";
            return Task.FromResult(_description);
        }

        public class GetKYCManagementQueryHandler : IRequestHandler<GetKYCManagementQuery, PagedResult<KYCManagementOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly IIdentityContext identityContext;
            private readonly TrangloUserManager _userManager;
            private readonly IApplicationUserRepository _applicationUserRepository;
            private readonly IBusinessProfileRepository _businessProfileRepository;

            public GetKYCManagementQueryHandler(IConfiguration config, IIdentityContext identityContext, TrangloUserManager userManager, IApplicationUserRepository applicationUserRepository, IBusinessProfileRepository businessProfileRepository)
            {
                _config = config;
                this.identityContext = identityContext;
                _userManager = userManager;
                _applicationUserRepository = applicationUserRepository;
                _businessProfileRepository = businessProfileRepository;
            }

            public async Task<PagedResult<KYCManagementOutputDTO>> Handle(GetKYCManagementQuery request, CancellationToken cancellationToken)
            {
                var solutionDescription = await _businessProfileRepository.GetSolutionByCodeAsync(request.SolutionCode);
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

                PagedResult<KYCManagementOutputDTO> result = new PagedResult<KYCManagementOutputDTO>();
                var _connectionString = _config.GetConnectionString("DefaultConnection");               
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var _sortExpression = string.IsNullOrEmpty(request.PagingOptions.SortExpression) ? "" : request.PagingOptions.SortExpression + " " + (request.PagingOptions.Direction == SortDirection.Ascending ? "" : "DESC");
                    var reader = await connection.QueryMultipleAsync(
                        "GetKYCManagementQuery",
                        new
                        {
                            PartnerNameFilter = !string.IsNullOrWhiteSpace(request.PartnerName) ? string.Format("%{0}%", request.PartnerName) : null,
                            CountryISO2Filter = !string.IsNullOrWhiteSpace(request.CountryISO2) ? string.Format("%{0}%", request.CountryISO2) : null,
                            ComplianceOfficerAssignedFilter = !string.IsNullOrWhiteSpace(request.ComplianceOfficerAssignedLoginId) ? request.ComplianceOfficerAssignedLoginId : null,
                            RegistrationFromFilter = request.fromRegistrationDate,
                            RegistrationToFilter = request.toRegistrationDate,
                            KYCStatusCodeFilter = request.KYCStatusCode,
                            PageIndex = request.PagingOptions.PageIndex,
                            PageSize = request.PagingOptions.PageSize,
                            sortExpression = _sortExpression,
                            EntityCode = request.EntityCode,
                            SolutionCode = request.SolutionCode,
                            SolutionDescription = solutionDescription.Name, 
                            FullRegisteredCompanyLegalName = request.FullRegisteredCompanyLegalName,
                            WorkflowStatusCode = request.WorkFlowStatusCode,
                            CustomerTypeCode = request.CustomerTypeCode,
                            IsComplianceTeamReview = request.IsComplianceTeamReview
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    result.Results = await reader.ReadAsync<KYCManagementOutputDTO>();
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
