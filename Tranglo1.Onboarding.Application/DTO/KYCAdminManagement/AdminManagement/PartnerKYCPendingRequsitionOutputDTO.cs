using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class PartnerKYCPendingRequsitionOutputDTO
    {
        public bool PendingRequisition { get; set; }
        public string RequisitionCode { get; set; }
        public int? KYCStatusCode { get; set; }
        public string KYCStatus { get; set; }
    }
}
