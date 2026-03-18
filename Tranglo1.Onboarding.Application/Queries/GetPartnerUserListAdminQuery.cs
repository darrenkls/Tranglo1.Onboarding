using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.View)]
    [Permission(Permission.ManagePartnerUser.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]
    internal class GetPartnerUserListAdminQuery : BaseQuery<PagedResult<PartnerUserListAdminOutputDTO>>
    {
        public string NameFilter { get; set; }
        public long? SolutionFilter { get; set; }
        public string EmailFilter { get; set; }
        public string UserRoleFilter { get; set; }
        public int? AccountStatusFilter { get; set; }
        public int? UserEnvironmentFilter { get; set; }
        public int? CompanyFilter { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();


        public class GetPartnerUserListAdminQueryHandler : IRequestHandler<GetPartnerUserListAdminQuery, PagedResult<PartnerUserListAdminOutputDTO>>
        {
            private class GetPartnerUsersView
            {
                public int UserId { get; set; }
                public string Name { get; set; }
                public int BusinessProfileCode { get; set; }
                public string CompanyName { get; set; }
                public string Email { get; set; }
                public string UserEnvironment { get; set; }
                public string BlockStatus { get; set; }
                public string AccountStatus { get; set; }
                public long CustomerUserRegistrationId { get; set; }
                public int AccountStatusCode { get; set; }
                public long CustomerUserBusinessProfileCode { get; set; }
            }

            private class UserRoleSolutionView
            {
                public int UserId { get; set; }
                public string UserRole { get; set; }
                public string Solution { get; set; }
                public int BusinessProfileCode { get; set; }
            }

            private readonly IConfiguration _config;
            public GetPartnerUserListAdminQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<PagedResult<PartnerUserListAdminOutputDTO>> Handle(GetPartnerUserListAdminQuery request, CancellationToken cancellationToken)
            {
                PagedResult<PartnerUserListAdminOutputDTO> result = new PagedResult<PartnerUserListAdminOutputDTO>();
                var outputDTO = new List<PartnerUserListAdminOutputDTO>();

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var _sortExpression = string.IsNullOrEmpty(request.PagingOptions.SortExpression) ? "" : request.PagingOptions.SortExpression + " " + (request.PagingOptions.Direction == SortDirection.Ascending ? "" : "DESC");
                    var reader = await connection.QueryMultipleAsync(
                        "GetPartnerUserListAdmin",
                        new
                        {
                            PageIndex = request.PagingOptions.PageIndex,
                            PageSize = request.PagingOptions.PageSize,
                            NameFilter = !string.IsNullOrWhiteSpace(request.NameFilter) ? string.Format("%{0}%", request.NameFilter) : null,
                            SolutionFilter = request.SolutionFilter ?? null,
                            EmailFilter = request.EmailFilter ?? null,
                            UserRoleFilter = request.UserRoleFilter ?? null,
                            AccountStatusFilter = request.AccountStatusFilter ?? null,
                            CompanyFilter = request.CompanyFilter ?? null,
                            UserEnvironmentFilter = request.UserEnvironmentFilter ?? null,
                            sortExpression = _sortExpression
                        },
                        null, null, CommandType.StoredProcedure);

                    var results = await reader.ReadAsync<GetPartnerUsersView>();
                    var resultsGroup = results.GroupBy(x => new { x.BusinessProfileCode })
                        .Where(g => g.Count() > 0)
                        .SelectMany(z => z).ToList();

                    var userRoleSolutions = await reader.ReadAsync<UserRoleSolutionView>();

                    foreach (var r in resultsGroup)
                    {
                        var userRoles = new List<UserRoleSolution>();
                        userRoles.AddRange(userRoleSolutions.Where(x => x.BusinessProfileCode == r.BusinessProfileCode && x.UserId == r.UserId)
                               .Select(x => new UserRoleSolution
                               {
                                   UserRole = x.UserRole,
                                   Solution = x.Solution
                               }));

                        var partnerUsers = new PartnerUserListAdminOutputDTO()
                        {
                            UserId = r.UserId,
                            Name = r.Name,
                            BusinessProfileCode = r.BusinessProfileCode,
                            CompanyName = r.CompanyName,
                            Email = r.Email,
                            UserEnvironment = r.UserEnvironment,
                            BlockStatus = r.BlockStatus,
                            AccountStatus = r.AccountStatus,
                            AccountStatusCode = r.AccountStatusCode,
                            CustomerUserRegistrationId = r.CustomerUserRegistrationId,
                            UserRoles = userRoles,
                            CustomerUserBusinessProfileCode = r.CustomerUserBusinessProfileCode
                        };

                        outputDTO.Add(partnerUsers);
                    }

                    IEnumerable<PaginationInfoDTO> _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();

                    result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                    result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                    result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;
                    result.Results = outputDTO;
                }

                return result;
            }
        }
    }

}
