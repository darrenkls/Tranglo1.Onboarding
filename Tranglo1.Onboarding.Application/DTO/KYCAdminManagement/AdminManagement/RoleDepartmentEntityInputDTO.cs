using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class RoleDepartmentEntityInputDTO
    {
        public string TrangloEntityId { get; set; }
        public string TrangloRoleCode { get; set; }
        public int TrangloDepartmentCode { get; set; }
        public int Action { get; set; }
    }
}
