using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class UACLoginSubMenuActionOutputDTO
    {
        public string SubMenuCode { get; set; }
        public List<UACLoginSubMenuPermissionOutputDTO> UACActions { get; set; }
    }

    public class UACLoginSubMenuPermissionOutputDTO
    {
        public string SubMenuCode { get; set; }
        public string SubMenuPermissionCode { get; set; }
        public string UACAction { get; set; }
    }
}
