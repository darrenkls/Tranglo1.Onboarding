using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class AdminPermissionInfoOutputDTO
    {
        public string MainMenu { get; set; }
        public List<PermissionInfoAction> PermissionInfoActions { get; set; }
        public AdminPermissionInfoOutputDTO()
        {
            PermissionInfoActions = new List<PermissionInfoAction>();
        }
    }
    public class PermissionInfoAction
    {
        public string PermissionGroup { get; set; }
        public bool? View { get; set; }
        public bool? Create { get; set; }
        public bool? Edit { get; set; }
        public bool? Approve { get; set; }
    }
 
}
