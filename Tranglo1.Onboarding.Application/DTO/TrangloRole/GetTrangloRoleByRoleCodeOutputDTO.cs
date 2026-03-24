using System.Collections.Generic;
using Tranglo1.Onboarding.Application.Helper.ACL;

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
