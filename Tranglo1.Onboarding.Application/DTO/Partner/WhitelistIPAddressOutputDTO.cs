using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class WhitelistIPAddressOutputDTO
    {
        public long WhitelistIPId { get; set; }
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public string IPAddressStart { get; set; }
        public string IPAddressEnd { get; set; }
        public bool IsRangeIP { get; set; }
        public bool IsWhitelisted { get; set; }
        public int EnvironmentCode { get; set; }
        public string EnvironmentDescription { get; set; }
    }
}
