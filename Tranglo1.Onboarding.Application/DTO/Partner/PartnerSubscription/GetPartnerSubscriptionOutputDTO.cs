using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription
{
    public class GetPartnerSubscriptionOutputDTO
    {
        public long PartnerCode { get; set; }
        public string TrangloEntity { get; set; }
        public List<SubscriptionList> SubscriptionList { get; set; }
    }

    public class SubscriptionList
    {
        public long? PartnerSubscriptionCode { get; set; }
        public long? PartnerType { get; set; }
        public long? Solution { get; set; }
        public string Currency { get; set; }
        public bool DisplayDefaultPackage { get; set; }
        public long? PricingPackage { get; set; }
        public bool isLive { get; set; }
    }
}
