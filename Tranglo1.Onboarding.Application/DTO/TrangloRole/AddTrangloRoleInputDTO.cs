using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.IdentityServer.Queries;

namespace Tranglo1.CustomerIdentity.IdentityServer.Command
{
    public class AddTrangloRoleInputDTO
    {
        public int TrangloDepartmentCode { get; set; }
        public string RoleDescription { get; set; }
        public bool IsSuperApprover { get; set; }
        public int AuthorityLevelCode { get; set; }
        public List<string> PermissionInfoCodeList { get; set; }

    }
}
