using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription
{
    public class GetSalesPartnerSubscriptionOutputDTO
    {
        public long PartnerCode { get; set; }
        public List<SubscriptionDetail> PartnerSubscriptionDetails { get; set; }
    }

    public class SubscriptionDetail
    {
        public long PartnerSubscriptionCode { get; set; }
        public string Entity { get; set; }
        public string PartnerType { get; set; }
        public string Solution { get; set; }
        public string SettlementCurrency { get; set; }

        public SubscriptionDetail(long partnerSubscriptionCode, string entity, string partnerType, string solution, string settlementCurrency)
        {
            PartnerSubscriptionCode = partnerSubscriptionCode;
            Entity = entity;
            PartnerType = partnerType;
            Solution = solution;
            SettlementCurrency = settlementCurrency;
        }
    }
}
