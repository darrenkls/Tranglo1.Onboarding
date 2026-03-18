using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription
{
    public class ExternalPartnerSubsciptionListOutputDTO
    {
        public long PartnerType { get; set; }
        public string PartnerTypeDescription { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public string TrangloEntity { get; set; }
        public string SettlementCurrencyCode { get; set; }
    }
}
