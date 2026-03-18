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

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.View)]
    [Permission(Permission.ManageStagingUser.Action_View_Code,
        new int[] { (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    public class GetPartnerUserListCustomerQuery : PagingOptions, IRequest<PagedResult<PartnerUserListCustomerOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string NameFilter { get; set; }
        public string UserRoleFilter { get; set; }
        public int? AccountStatusFilter { get; set; }
        public int? UserEnvironmentFilter { get; set; }

        // Filters used in Tranglo Business portal
        public string CompanyAccountStatusFilter { get; set; }
        public string CompanyBlockStatusFilter { get; set; }
        public string NameOrEmailFilter { get; set; }
        public string MultipleRoleFilter { get; set; }

        public class GetPartnerUserListCustomerQueryHandler : IRequestHandler<GetPartnerUserListCustomerQuery, PagedResult<PartnerUserListCustomerOutputDTO>>
        {
            private class GetPartnerUsersView
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string UserRole { get; set; }
                public string Email { get; set; }
                public string UserEnvironment { get; set; }
                public long CompanyUserBlockStatusCode { get; set; }
                public string BlockStatus { get; set; }
                public long CompanyUserAccountStatusCode { get; set; }
                public string AccountStatus { get; set; }
                public string RoleCode { get; set; }
            }

            private readonly IConfiguration _config;
            public GetPartnerUserListCustomerQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<PagedResult<PartnerUserListCustomerOutputDTO>> Handle(GetPartnerUserListCustomerQuery request, CancellationToken cancellationToken)
            {
                PagedResult<PartnerUserListCustomerOutputDTO> result = new PagedResult<PartnerUserListCustomerOutputDTO>();
                var outputDTO = new List<PartnerUserListCustomerOutputDTO>();

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var _sortExpression = string.IsNullOrEmpty(request.SortExpression) ? "" : request.SortExpression + " " + (request.Direction == SortDirection.Ascending ? "" : "DESC");
                    var reader = await connection.QueryMultipleAsync(
                        "GetPartnerUserListCustomer",
                        new
                        {
                            PageIndex = request.PageIndex,
                            PageSize = request.PageSize,
                            BusinessProfileCode = request.BusinessProfileCode,
                            NameFilter = !string.IsNullOrWhiteSpace(request.NameFilter) ? string.Format("%{0}%", request.NameFilter) : null,
                            UserRoleFilter = request.UserRoleFilter ?? null,
                            AccountStatusFilter = request.AccountStatusFilter ?? null,
                            UserEnvironmentFilter = request.UserEnvironmentFilter ?? null,
                            CompanyAccountStatusFilter = request.CompanyAccountStatusFilter ?? null,
                            CompanyBlockStatusFilter = request.CompanyBlockStatusFilter ?? null,
                            NameOrEmailFilter = request.NameOrEmailFilter ?? null,
                            MultipleRoleFilter = request.MultipleRoleFilter ?? null,
                            sortExpression = _sortExpression
                        },
                        null, null, CommandType.StoredProcedure);

                    var results = await reader.ReadAsync<GetPartnerUsersView>();
                    var resultsGroup = results.GroupBy(x => new { x.Id }).Select(y => y.First()).ToList();

                    foreach (var r in resultsGroup)
                    {
                        var partnerUsers = new PartnerUserListCustomerOutputDTO()
                        {
                            Id = r.Id,
                            Name = r.Name,
                            UserRole = r.UserRole,
                            Email = r.Email,
                            UserEnvironment = r.UserEnvironment,
                            CompanyUserBlockStatusCode = r.CompanyUserBlockStatusCode,
                            BlockStatus = r.BlockStatus,
                            CompanyUserAccountStatusCode = r.CompanyUserAccountStatusCode,
                            AccountStatus = r.AccountStatus,
                            RoleCode = r.RoleCode                            
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
