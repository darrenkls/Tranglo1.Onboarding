using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class TrangloStaffOutputDTO
    {
        public string FullName { get; set; }
        public string LoginId { get; set; }
        public string Email { get; set; }
        public string AccountStatus { get; set; }
        public long AccountStatusId { get; set; }
        public string Timezone { get; set; }

        public List<TrangloStaffEntities> TrangloStaffEntity { get; set; } = new List<TrangloStaffEntities>();

        public class TrangloStaffEntities
        {
            public string TrangloRoleCode { get; set; }
            public string TrangloRoleDesc { get; set; }
            public string TrangloEntityId { get; set; }
            public string TrangloEntityDesc { get; set; }
            public int TrangloDepartmentCode { get; set; }
            public string TrangloDepartmentDesc { get; set; }
        }

    }
   
}
