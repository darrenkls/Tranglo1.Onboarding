using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Queries;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class AdminUserListsOutputDTO
    {
        public string FullName { get; set; }
     
        public string Email { get; set; }
        public string LoginId { get; set; }
        public string AccountStatus { get; set; }
        public string Timezone { get; set; }
        public string TrangloRole { get; set; }
        public string TrangloDepartment { get; set; }
        public string TrangloEntity{ get; set; }
        public string BlockStatus { get; set; }

    }
}
