using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class ConnectPermissionInfoOutputDTO
    {
        public string MainMenu { get; set; }
        public List<PermissionInfoAction2> PermissionInfoActions { get; set; }
        public ConnectPermissionInfoOutputDTO()
        {
            PermissionInfoActions = new List<PermissionInfoAction2>();
        }
    }
    public class PermissionInfoAction2
    {
        public string PermissionGroup { get; set; }
        public bool? View { get; set; }
        public bool? Create { get; set; }
        public bool? Edit { get; set; }
        public bool? Approve { get; set; }
        public string Menu { get; set; }
    }
}
