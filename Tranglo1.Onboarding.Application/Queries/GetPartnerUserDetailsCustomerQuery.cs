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
using static Tranglo1.Onboarding.Application.Queries.GetPartnerUserDetailsCustomerQuery;
//using static Tranglo1.Onboarding.Application.Queries.GetPartnerUserDetailsCustomerQuery;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.View)]
    internal class GetPartnerUserDetailsCustomerQuery : BaseQuery<Result<IEnumerable<ViewPartnerUserCustomerOutputDTO>>>
    {
        public int BusinessProfileCode { get; set; }
        public string Email { get; set; }

        public class ViewPartnerUserCustomer
        {
            public int BusinessProfileCode { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string UserRoleCode { get; set; }
            public string UserRole { get; set; }
            public int UserEnvironmentCode { get; set; }
            public string UserEnvironment { get; set; }
            public int AccountStatusCode { get; set; }
            public string AccountStatus { get; set; }
            public string Timezone { get; set; }
            public long? CompanyUserAccountStatusCode { get; set; }
            public string CompanyUserAccountStatus { get; set; }
            public long? CompanyUserBlockStatusCode { get; set; }
            public string CompanyUserBlockStatus { get; set; }
        }        
    }

    internal class GetPartnerUserDetailsCustomerQueryHandler : IRequestHandler<GetPartnerUserDetailsCustomerQuery, Result<IEnumerable<ViewPartnerUserCustomerOutputDTO>>>
    {
        private readonly ILogger<GetPartnerUserDetailsCustomerQueryHandler> _logger;
        private readonly IConfiguration _config;
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public GetPartnerUserDetailsCustomerQueryHandler(ILogger<GetPartnerUserDetailsCustomerQueryHandler> logger, IConfiguration config, TrangloUserManager userManager, IBusinessProfileRepository businessProfileRepository)
        {
            _logger = logger;
            _config = config;
            _userManager = userManager;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<IEnumerable<ViewPartnerUserCustomerOutputDTO>>> Handle(GetPartnerUserDetailsCustomerQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Result.Failure<IEnumerable<ViewPartnerUserCustomerOutputDTO>>($"No existing Partner User for Email: {request.Email}");
            }

            var customerUserBusinessProfile = await _businessProfileRepository.GetCustomerUserBusinessProfilesByUserIdAsync(user.Id, request.BusinessProfileCode);

            if (customerUserBusinessProfile == null)
            {
                return Result.Failure<IEnumerable<ViewPartnerUserCustomerOutputDTO>>($"No existing Partner User for Email: {request.Email} and BusinessProfileCode: {request.BusinessProfileCode}");
            }

            Result<IEnumerable<ViewPartnerUserCustomerOutputDTO>> result;

            var outputDTO = new List<ViewPartnerUserCustomerOutputDTO>();

            var _connectionString = _config.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetCustomerPartnerUserDetails",
                    new
                    {
                        businessProfileCode = request.BusinessProfileCode,
                        email = request.Email
                    },
                    null, null, CommandType.StoredProcedure);

                var resultList = await reader.ReadAsync<ViewPartnerUserCustomer>();

                var userResultGroup = resultList.GroupBy(x => new { x.Email } ).Select(y => y.First()).ToList();

                foreach (var u in userResultGroup)
                {
                    var partnerUser = new ViewPartnerUserCustomerOutputDTO
                    {
                        BusinessProfileCode = u.BusinessProfileCode,
                        Name = u.Name,
                        Email = u.Email,
                        UserEnvironmentCode = u.UserEnvironmentCode,
                        UserEnvironment = u.UserEnvironment,
                        AccountStatusCode = u.AccountStatusCode,
                        AccountStatus = u.AccountStatus,                        
                        Timezone = u.Timezone,
                        CompanyUserAccountStatusCode = u.CompanyUserAccountStatusCode,
                        CompanyUserAccountStatus = u.CompanyUserAccountStatus,
                        CompanyUserBlockStatusCode = u.CompanyUserBlockStatusCode,
                        CompanyUserBlockStatus = u.CompanyUserBlockStatus
                    };

                    var rolesGroup = resultList.Where(x => x.Email == u.Email).ToList();
                    foreach (var r in rolesGroup)
                    {
                        var partnerUserRoles = new ViewPartnerUserCustomerOutputDTO.UserRoles
                        {
                            UserRoleCode = r.UserRoleCode,
                            UserRole = r.UserRole
                        };
                        partnerUser.UserRole.Add(partnerUserRoles);
                    }

                    outputDTO.Add(partnerUser);
                }

                result = outputDTO;
            }

            if (result.IsFailure)
            {
                return Result.Failure<IEnumerable<ViewPartnerUserCustomerOutputDTO>>($"Failed to retrieve Partner User details for UserID: {request.Email} and BusinessProfileCode: {request.BusinessProfileCode}");
            }

            return Result.Success<IEnumerable<ViewPartnerUserCustomerOutputDTO>>(result.Value);

        }

    }    
}