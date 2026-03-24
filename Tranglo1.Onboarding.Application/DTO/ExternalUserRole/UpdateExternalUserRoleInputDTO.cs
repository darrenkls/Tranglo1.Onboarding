using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class UpdateExternalUserRoleInputDTO
    {
        public List<ExternalPermissionDetail> PermissionInfoList { get; set; }
    }

    public class ExternalPermissionDetail
    {
        public string PermissionInfoCode { get; set; }
        public bool IsSelected { get; set; }
    }
}
