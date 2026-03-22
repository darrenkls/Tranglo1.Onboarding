using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class GetAdminPermissionInfoOutputDTO
    {
        public string PermissionGroup { get; set; }
        public bool IsView { get; set; }
        public bool IsCreate { get; set; }
        public bool IsEdit { get; set; }
        public bool IsApprove { get; set; }
        public string Menu { get; set; }
    }
}
