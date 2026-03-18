using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.MediatR;
using static Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement.TrangloStaffOutputDTO;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetSubscriptionsEntityRolesQuery : BaseQuery<Result<SubscriptionEntityRolesOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public string Entity { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<SubscriptionEntityRolesOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get Subscription Entity List for PartnerCode: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class GetSubscriptionsEntityListQueryHandler : IRequestHandler<GetSubscriptionsEntityRolesQuery, Result<SubscriptionEntityRolesOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetSubscriptionsEntityListQueryHandler> _logger;
            private readonly IApplicationUserRepository _applicationUserRepository;

            private class SubscriptionEntityRoles
            {
                public string Entity { get; set; }
                public string EntityDescription { get; set; }
                public string RoleCode { get; set; }
                public string RoleName { get; set; }               
                public int BlockStatusCode { get; set; }
                public int UserAccountStatusCode { get; set; }
                public int AuthorityLevelCode { get; set; }
                public int IsSuperApprover { get; set; }
            }

            public GetSubscriptionsEntityListQueryHandler(IConfiguration config, 
                IApplicationUserRepository applicationUserRepository, 
                ILogger<GetSubscriptionsEntityListQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
                _applicationUserRepository = applicationUserRepository;
            }

            public async Task<Result<SubscriptionEntityRolesOutputDTO>> Handle(GetSubscriptionsEntityRolesQuery request, CancellationToken cancellationToken)
            {
                //NOTE: The entity list here excludes the entity that the partner has already subscribed to AND the entity that admin users do not have access to

                var currentUserEntity = await _applicationUserRepository.GetTrangloEntityByCodeAsync(request.Entity);

                if (currentUserEntity == null)
                {
                    return Result.Failure<SubscriptionEntityRolesOutputDTO>($"Current User Entity: {request.Entity} does not exist.");
                }

                var userEntities = new List<UserEntity>();
                SubscriptionEntityRolesOutputDTO outputDTO = new SubscriptionEntityRolesOutputDTO();

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                string timezone = String.Empty;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "GetSubscriptionEntityRoles",
                        new
                        {
                            LoginId = request.LoginId,
                            CurrentUserEntity = request.Entity
                        },
                        null, null, CommandType.StoredProcedure);
                    var results = await reader.ReadAsync<SubscriptionEntityRoles>();
                    var userEntityGrouping = results.GroupBy(x => new { x.Entity }).Select(x => x.First()).ToList();

                    foreach (var item in userEntityGrouping)
                    {
                        List<UserEntityRole> userEntityRoles = new List<UserEntityRole>();
                        var userEntityRolesResult = results.Where(x => x.Entity == item.Entity);
                        foreach (var items in userEntityRolesResult)
                        {
                            bool isSuperApprover = Convert.ToBoolean(items.IsSuperApprover);

                            var userEntityRole = new UserEntityRole
                            {
                                RoleCode = items.RoleCode,
                                RoleName = items.RoleName,
                                AuthorityLevelCode = items.AuthorityLevelCode,
                                IsSuperApprover = isSuperApprover
                            };
                            userEntityRoles.Add(userEntityRole);
                        }
                        var userEntity = new UserEntity
                        {
                            UserAccountStatusCode = item.UserAccountStatusCode,
                            Entity = item.Entity,
                            EntityDescription = item.EntityDescription,
                            BlockStatusCode = item.BlockStatusCode,
                            UserEntityRoles = userEntityRoles
                        };
                        userEntities.Add(userEntity);
                    }
                }
                outputDTO.UserEntities = userEntities;

                return Result.Success(outputDTO);
            }
        }
    }
    
}
