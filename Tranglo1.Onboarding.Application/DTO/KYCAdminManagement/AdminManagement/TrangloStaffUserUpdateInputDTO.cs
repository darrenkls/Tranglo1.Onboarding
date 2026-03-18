using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class TrangloStaffUserUpdateInputDTO
    {
        [MaxLength(150, ErrorMessage = "Name maximum length is 150 characters")]
        public string Name { get; set; }
        [MaxLength(150, ErrorMessage = "Email maximum length is 150 characters")]
        public string Email { get; set; }
        [MaxLength(150, ErrorMessage = "Timezone maximum length is 150 characters")]
        public string Timezone { get; set; }
        public long AccountStatus { get; set; }
        public List<RoleDepartmentEntityInputDTO> trangloStaffEntity { get; set; }
    }

}
