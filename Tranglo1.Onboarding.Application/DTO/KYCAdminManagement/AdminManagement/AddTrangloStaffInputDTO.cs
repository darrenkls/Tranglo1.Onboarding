using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class AddTrangloStaffInputDTO
    {
        [MaxLength(150, ErrorMessage = "Email maximum length is 150 characters")]
        public string Email { get; set; }
        [MaxLength(150, ErrorMessage = "Timezone maximum length is 150 characters")]
        public string Timezone { get; set; }
        [MaxLength(150, ErrorMessage = "Name maximum length is 150 characters")]
        public string Name { get; set; }
        public long AccountStatus { get; set; }
        //public AddTrangloStaffModel AddTrangloStaffModel { get; set; }
        public List<AddTrangloStaffAssignment> trangloStaffEntity { get; set; }

    }
    public class AddTrangloStaffAssignment
    {
        public string TrangloEntityId { get; set; }
        public string TrangloRoleCode { get; set; }
        public int TrangloDepartmentCode{ get; set; }
    }
}
