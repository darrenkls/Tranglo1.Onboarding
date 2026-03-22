using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.IdentityServer.DTO.ExternalUserRole;
using Tranglo1.Onboarding.IdentityServer.DTO.TrangloRole;
using Tranglo1.Onboarding.IdentityServer.Helper.ACL;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class GetTrangloRoleByRoleCodeOutputDTO
    {
        public string RoleName { get; set; }
        public int AuthorityLevelCode { get; set; }
        public int DepartmentCode{ get; set; }
        public bool IsSuperApprover { get; set; }

        public List<ScreenAccessMenu> ScreenAccessMenuList { get; set; }
    }
}
