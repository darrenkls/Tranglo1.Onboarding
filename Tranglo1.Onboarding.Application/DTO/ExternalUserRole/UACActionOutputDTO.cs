using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class UACActionOutputDTO
    {
        public string SubMenuCode { get; set; }
        public string SubMenuPermissionCode { get; set; }
        public int UACActionCode { get; set; }
        public bool IsChecked { get; set; }
    }
}
