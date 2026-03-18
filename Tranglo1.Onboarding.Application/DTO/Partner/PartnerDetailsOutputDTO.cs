using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerDetailsOutputDTO
    {
        public long PartnerCode { get; set; }
        public List<PartnerDetail> PartnerDetails { get; set; }
    }

    public class PartnerDetail
    {
        public long PartnerSubscriptionCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerType { get; set; }
        public string TrangloEntity { get; set; }
        public string Solution { get; set; }
        public string CurrencyCode { get; set; }
        public string Country { get; set; }
        public string Agent { get; set; }
        public string CompliancePIC { get; set; }
        public string ProductPIC { get; set; }
        public string SalesOperationPIC { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
    }
}