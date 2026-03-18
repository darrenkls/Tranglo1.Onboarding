using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine.Interface;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using Tranglo1.Onboarding.Application.Services.Identity;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Managers.KYCApproval
{
    public class RequisitionUserIdentityContext : IUserIdentityContext
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IRoleCodeContext _roleCodeContext;
        private readonly ITrangloRoleRepository _trangloRoleRepository;

        public RequisitionUserIdentityContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IRoleCodeContext roleCodeContext, ITrangloRoleRepository trangloRoleRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _roleCodeContext = roleCodeContext;
            _trangloRoleRepository = trangloRoleRepository;
        }

        public string GetUserIdentity()
        {
            return _httpContextAccessor.HttpContext.User.GetSubjectId().Value;
        }

        public int GetUserLevel()
        {
            string roleCode = "";
            if (_roleCodeContext.CurrentRoleCode.HasValue)
            {
                roleCode = _roleCodeContext.CurrentRoleCode.Value;

            }

            var _connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var reader = connection.Query<int?>(
                        "GetApprovalLevelByUserIdAndRoleCode",
                        new
                        {
                            @LoginId = GetUserIdentity(),
                            @RoleCode = roleCode
                        },
                      commandType: CommandType.StoredProcedure).First();

                return reader ?? 0;
            }
        }

        //public bool IsSuperApproval()
        //{
        //    string roleCode = "";
        //    if (_roleCodeContext.CurrentRoleCode.HasValue)
        //    {
        //        roleCode = _roleCodeContext.CurrentRoleCode.Value;
        //        var trangloRole =  _trangloRoleRepository.GetTrangloRoleByCode(roleCode);

        //        return trangloRole.IsSuperApprover.GetValueOrDefault(false);

        //    }
        //    return false;
        //}

        public async Task<bool> IsSuperApprovalAsync()
        {
            string roleCode = "";
            if (_roleCodeContext.CurrentRoleCode.HasValue)
            {
                roleCode = _roleCodeContext.CurrentRoleCode.Value;

                var trangloRole = await _trangloRoleRepository.GetTrangloRoleByCodeAsync(roleCode);

                return trangloRole.IsSuperApprover.GetValueOrDefault(false);


            }
            return false;
        }
    }
}
