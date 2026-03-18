using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class TrangloStaffByClaimsOutputDTO
    {
        public string Timezone { get; set; }
        public List<TrangloStaffs> TrangloStaffs { get; set; }
    }
    public class TrangloStaffs
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public int UserAccountStatusCode { get; set; }
        public int BlockStatusCode { get; set; }
        public List<TrangloStaffRoles> TrangloStaffRole { get; set; }
    }
    public class TrangloStaffRoles
    {
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public int AuthorityLevelCode { get; set; }
        public bool IsSuperApprover { get; set; }
    }
}
