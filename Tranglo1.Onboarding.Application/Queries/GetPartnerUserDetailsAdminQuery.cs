using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Domain.Entities;
using static Tranglo1.Onboarding.Application.Queries.GetPartnerUserDetailsAdminQuery;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.View)]
    [Permission(Permission.ManagePartnerUser.Action_ViewDetail_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerUser.Action_View_Code })]

    internal class GetPartnerUserDetailsAdminQuery : BaseQuery<Result<IEnumerable<ViewPartnerUserAdminOutputDTO>>>
    {
        public string Email { get; set; }
        public long CustomerUserBusinessProfileCode { get; set; } 
        public int BusinessProfileCode { get; set; }
        public override Task<System.String> GetAuditLogAsync(Result<IEnumerable<ViewPartnerUserAdminOutputDTO>> result)
        {
            return Task.FromResult("Searched partner user details");
        }

        public class ViewPartnerUserAdmin
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public int CompanyCode { get; set; }
            public string Company { get; set; }
            public string UserRoleCode { get; set; }
            public string UserRole { get; set; }
            public long SolutionCode { get; set; }
            public string Solution { get; set; }
            public int UserEnvironmentCode { get; set; }
            public string UserEnvironment { get; set; }
            public int AccountStatusCode { get; set; }
            public string AccountStatus { get; set; }
            public string Timezone { get; set; }
            public long CustomerUserBusinessProfileCode { get; set; }
        }
    }

    internal class GetPartnerUserDetailsAdminQueryHandler : IRequestHandler<GetPartnerUserDetailsAdminQuery, Result<IEnumerable<ViewPartnerUserAdminOutputDTO>>>
    {
        private readonly ILogger<GetPartnerUserDetailsAdminQueryHandler> _logger;
        private readonly IConfiguration _config;
        private readonly TrangloUserManager _userManager;
        private readonly IApplicationUserRepository _applicationUserRepository;

        public GetPartnerUserDetailsAdminQueryHandler(ILogger<GetPartnerUserDetailsAdminQueryHandler> logger, IConfiguration config, TrangloUserManager userManager, IApplicationUserRepository applicationUserRepository)
        {
            _logger = logger;
            _config = config;
            _userManager = userManager;
            _applicationUserRepository = applicationUserRepository;
        }

        public async Task<Result<IEnumerable<ViewPartnerUserAdminOutputDTO>>> Handle(GetPartnerUserDetailsAdminQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                return Result.Failure<IEnumerable<ViewPartnerUserAdminOutputDTO>>($"No existing Partner User for Email: {request.Email}");
            }            

            Result<IEnumerable<ViewPartnerUserAdminOutputDTO>> result;

            var outputDTO = new List<ViewPartnerUserAdminOutputDTO>();

            var _connectionString = _config.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetAdminPartnerUserDetails",
                    new
                    {
                        email = request.Email,
                        customerUserBusinessProfileCode = request.CustomerUserBusinessProfileCode,
                        businessProfileCode = request.BusinessProfileCode,
                        userId = user.Id
                    },
                    null, null, CommandType.StoredProcedure);

                var resultList = await reader.ReadAsync<ViewPartnerUserAdmin>();

                var userResultGroup = resultList.GroupBy(x => new { x.Email }).Select(y => y.First()).ToList();

                foreach (var u in userResultGroup)
                {
                    var partnerUser = new ViewPartnerUserAdminOutputDTO
                    {
                        Name = u.Name,
                        Email = u.Email,
                        UserEnvironmentCode = u.UserEnvironmentCode,
                        UserEnvironment = u.UserEnvironment,
                        AccountStatusCode = u.AccountStatusCode,
                        AccountStatus = u.AccountStatus,
                        Timezone = u.Timezone,
                        CustomerUserBusinessProfileCode = u.CustomerUserBusinessProfileCode

                    };

                    var rolesGroup = resultList.Where(x => x.Email == u.Email).ToList();
                    foreach (var r in rolesGroup)
                    {
                        var partnerUserRoles = new ViewPartnerUserAdminOutputDTO.BusinessProfileRoles
                        {
                            CompanyCode = r.CompanyCode,
                            Company = r.Company,
                            UserRoleCode = r.UserRoleCode,
                            UserRole = r.UserRole,
                            SolutionCode = r.SolutionCode,
                            Solution = r.Solution
                        };
                        partnerUser.BusinessProfileRole.Add(partnerUserRoles);
                    }

                    outputDTO.Add(partnerUser);

                }

                result = outputDTO;
            }

            if (result.IsFailure)
            {
                return Result.Failure<IEnumerable<ViewPartnerUserAdminOutputDTO>>($"Failed to retrieve Partner User details for Email: {request.Email}");
            }

            return Result.Success<IEnumerable<ViewPartnerUserAdminOutputDTO>>(result.Value);

        }

    }
}