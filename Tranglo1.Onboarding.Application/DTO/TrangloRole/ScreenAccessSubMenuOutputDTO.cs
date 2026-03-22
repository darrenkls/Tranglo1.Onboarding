using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class ScreenAccessSubMenuOutputDTO
    {
        public int SubMenuCode { get; set; }
        public string SubMenuDescription { get; set; }
        public bool IsViewable { get; set; }
        public bool IsCreatable { get; set; }
        public bool IsEditable { get; set; }
        public bool IsApprovable { get; set; }
    }
}
