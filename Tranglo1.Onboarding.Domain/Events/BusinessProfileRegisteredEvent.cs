using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class BusinessProfileRegisteredEvent : DomainEvent
    {
        public BusinessProfile BusinessProfile { get; set; }
        public Email Email { get; set; }
        public long? CustomerTypeCode { get; set; }
        public string IMID { get; set; }
        public string TrangloEntity { get; set; }
        public virtual string AgentLoginId { get; set; }
        public long? RspStagingId { get; set; }
        public long? SupplierPartnerStagingId { get; set; }
        public Solution Solution { get; set; }
        public bool IsTncTick { get; set; }

        public PartnerRegistrationLeadsOrigin? LeadsOrigin { get; set; }
        public string? OtherLeadsOrigin { get; set; }
        public CountryMeta CountryISO2 { get; set; }

        public BusinessProfileRegisteredEvent(BusinessProfile businessProfile, Email email, long? customerType, string iMID, string trangloEntity,
                                        string agentLoginId, long? rspStagingId, long? supplierPartnerStagingId, Solution solution, bool isTncTick,
                                        PartnerRegistrationLeadsOrigin? leadsOrigin, string? otherLeadsOrigin, CountryMeta countryISO2)
        {
            this.BusinessProfile = businessProfile;
            this.Email = email;
            this.CustomerTypeCode = customerType;
            this.IMID = iMID;
            this.TrangloEntity = trangloEntity;
            this.AgentLoginId = agentLoginId;
            this.RspStagingId = rspStagingId;
            this.SupplierPartnerStagingId = supplierPartnerStagingId;
            this.Solution = solution;
            this.IsTncTick = isTncTick;
            this.LeadsOrigin = leadsOrigin;
            this.OtherLeadsOrigin = otherLeadsOrigin;
            this.CountryISO2 = countryISO2;
        }
    }
}
