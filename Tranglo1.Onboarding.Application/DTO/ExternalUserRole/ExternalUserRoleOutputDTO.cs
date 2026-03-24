using System.Collections.Generic;
using Tranglo1.Onboarding.Application.Helper.ACL;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class ExternalUserRoleOutputDTO
    {
        public string RoleName { get; set; }
        public List<ScreenAccessMenu> ScreenAccessMenuList { get; set; }
    }

}
