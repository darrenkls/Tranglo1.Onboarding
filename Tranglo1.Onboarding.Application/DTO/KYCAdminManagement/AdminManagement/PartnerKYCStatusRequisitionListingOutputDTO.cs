using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class PartnerKYCStatusRequisitionListingOutputDTO
    {
        public string RequisitionCode { get; set; }
        public int RequisitionStatus { get; set; }
        public string RequestType { get; set; }
        public string KYCStatus { get; set; }
        public int KYCStatusCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public string CompanyName { get; set; }
        public string TrangloEntity { get; set; }
        public string Country { get; set; }
        public int ApprovalLevel { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CORemarks { get; set; }
        public string Level1ApprovedBy { get; set; }
        public DateTime? Level1CreatedDate { get; set; }
        public string Level2ApprovedBy { get; set; }
        public DateTime? Level2CreatedDate { get; set; }
        public string ApproverRemarks { get; set; }
        public string ApprovalStatus { get; set; }
        public int SolutionCode { get; set; }
        public string CompanyRegistrationName { get; set; }
        public long? CollectionTierCode { get; set; }
        public string CollectionTierDescription { get; set; }
        public int CustomerTypeCode { get; set; }
        public string CustomerTypeDescription { get; set; }
    }
}
