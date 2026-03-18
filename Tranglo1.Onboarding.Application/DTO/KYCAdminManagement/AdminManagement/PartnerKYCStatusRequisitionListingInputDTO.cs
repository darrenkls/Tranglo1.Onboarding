using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class PartnerKYCStatusRequisitionListingInputDTO
    {
        public string RequisitionCode { get; set; }
        public int? KYCStatusCode { get; set; }
        public int? BusinessProfileCode { get; set; }
        public string CreatedBy { get; set; }
        public string L1Approval { get; set; }
        public string L2Approval { get; set; }
        public int? ApprovalStatusCode { get; set; }
        public DateTime? CreatedDateStart { get; set; }
        public DateTime? CreatedDateEnd { get; set; }
        public DateTime? L1ApprovalDateStart { get; set; }
        public DateTime? L1ApprovalDateEnd { get; set; }
        public DateTime? L2ApprovalDateStart { get; set; }
        public DateTime? L2ApprovalDateEnd { get; set; }
    }
}
