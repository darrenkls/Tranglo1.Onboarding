using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class UpdateTrangloRoleOutputDTO
    {
        public bool IsSuperApprover { get; set; }
        public List<UpdatePermissionDetail> PermissionInfoList { get; set; }
    }
}
