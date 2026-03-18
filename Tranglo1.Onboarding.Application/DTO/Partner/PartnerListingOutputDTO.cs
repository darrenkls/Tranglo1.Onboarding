using System;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerListingOutputDTO
    {
        public long PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string TradeName { get; set; }
        public string Country { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string Agent { get; set; }
        public string AgreementStatus { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string FinalApprovalStatus { get; set; }
        public int ApprovalLevel { get; set; }
        public string ApprovalStatus { get; set; }
        public long? KYCStatusCode { get; set; }
        public string KYCStatusCodeDescription { get; set; }
        public int RequisitionStatus { get; set; }
        public string KYCReminderStatus { get; set; }
        public List<Subscription> Subscriptions { get; set; }
        public string LeadsOriginDescription { get; set; }
        public string OtherLeadsOrigin { get; set; }
        public string FullLeadsOriginDescription { get; set; }
    }

    public class Subscription
    {
        public long PartnerSubscriptionCode { get; set; }
        public string Entity { get; set; }
        public string Solution { get; set; }
        public long SolutionCode { get; set; }
        public string PartnerType { get; set; }
        public string EnvironmentDescription { get; set; }
        public long EnvironmentCode { get; set; }
        public string Status { get; set; }
        public long StatusCode { get; set; }
        public string WorkFlowStatus { get; set; }
        public long PartnerCode { get; set; }
        public bool HasRequestedGoLive { get; set; }
        public int KYCReminderSubscriptionCode { get; set; }
        public string KYCReminderSubscriptionDescription { get; set; }
    }
}
