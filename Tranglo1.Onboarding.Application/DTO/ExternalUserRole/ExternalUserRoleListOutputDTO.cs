using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class ExternalUserRoleListOutputDTO
    {
        public int ExternalUserRoleCode { get; set; }
        public string RoleCode { get; set; }
        public string ExternalUserRoleName { get; set; }
        public int ExternalUserRoleStatusCode { get; set; }
        public string ExternalUserRoleStatus { get; set; }
        public long SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public int PortalCode { get; set; }
    }
}