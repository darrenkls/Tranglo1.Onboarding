using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class GetRBARequisitionListingOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public string RequisitionCode { get; set; }
        public int? ComplianceRequisitionTypeCode { get; set; }
        public string ComplianceRequisitionTypeDescription { get; set; }
        public int? ApprovalLevel { get; set; }
        public int? RequisitionStatus { get; set; }
        public string RequisitionStatusDescription { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ApproveBy { get; set; }
        public int? ApprovalStatus { get; set; }
        public string TrangloEntity { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public int? FinalApprovalStatus { get; set; }
        public string FinalApprovalStatusDescription { get; set; }
        public string RiskRanking { get; set; }
        public string Remarks { get; set; }

       
    }
}
