using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.IdentityServer.Queries;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class UpdateTrangloRoleOutputDTO
    {
        public bool IsSuperApprover { get; set; }
        public List<UpdatePermissionDetail> PermissionInfoList { get; set; }
    }
}
