using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCRequisitionRejectInputDTO
    {
        public string OTP { get; set; }
        public string Remarks { get; set; }
        public string RequisitionCode { get; set; }
    }
}
