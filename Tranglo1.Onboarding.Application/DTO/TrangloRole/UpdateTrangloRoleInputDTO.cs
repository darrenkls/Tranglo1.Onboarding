using System.Collections.Generic;

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
