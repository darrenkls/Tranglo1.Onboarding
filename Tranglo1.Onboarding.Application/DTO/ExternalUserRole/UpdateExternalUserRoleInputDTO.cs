using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.IdentityServer.Helper.ACL;

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
