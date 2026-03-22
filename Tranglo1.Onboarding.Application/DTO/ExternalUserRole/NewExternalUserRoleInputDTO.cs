using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class NewExternalUserRoleInputDTO
    {
        public string RoleName { get; set; }
        public long SolutionCode { get; set; }
        public List<string> PermissionInfoCodeList { get; set; }

    }
}
