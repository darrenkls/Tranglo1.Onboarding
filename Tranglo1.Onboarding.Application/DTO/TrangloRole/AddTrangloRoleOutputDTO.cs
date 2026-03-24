using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class AddTrangloRoleOutputDTO
    {
        public string RoleName { get; set; }
        public long DepartmentCode { get; set; }
        public long AuthorityLevelCode { get; set; }
        public bool? IsSuperApprover { get; set; }
        public List<string> PermissionInfoCodeList { get; set; }
    }
}
