using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Helper.ACL
{
    public class ScreenAccessMenu
    {
        public string MenuName { get; set; }
        public int Order { get; set; }
        public List<SubMenuDetail> SubMenuList { get; set; }

        public class SubMenuDetail
        {
            public string SubMenuCode { get; set; }
            public string SubMenuName { get; set; }
            public int Order { get; set; }
            public List<PermissionDetail> PermissionInfoList { get; set; }
        }

        public class PermissionDetail
        {
            public string PermissionInfoCode { get; set; }
            public string ActionName { get; set; }
            public List<string> RelatedPermissionInfoCodes { get; set; }
            public bool IsSelected { get; set; }
            public int Order { get; set; }
        }
    }
}
