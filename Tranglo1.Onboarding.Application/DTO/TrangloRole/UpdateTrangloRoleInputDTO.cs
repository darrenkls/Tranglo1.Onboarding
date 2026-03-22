using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.IdentityServer.DTO.TrangloRole;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class UpdateTrangloRoleInputDTO
    {
        public bool IsSuperApprover { get; set; }
        public List<UpdatePermissionDetail> PermissionInfoList { get; set; }
    }
    public class UpdatePermissionDetail
    {
        public string PermissionInfoCode { get; set; }
        public bool IsSelected { get; set; }
    }
}
