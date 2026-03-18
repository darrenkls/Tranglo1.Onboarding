using System;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCSummaryFeedbackOutputDTO
    {
        public int KYCSummaryFeedbackCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public long KYCCategoryCode { get; set; }
        public string KYCCategoryDescription { get; set; }
        public string IncorrectItem { get; set; }
        public string InternalRemarks { get; set; }
        public string FeedbackToUser { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string CreatedByName { get; set; }
        public bool IsUnread { get; set; }
        public bool IsResolved { get; set; }
    }
}
