using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class APIEnvironmentSettingsOutputDTO
    {
        public long PartnerAPISettingId { get; set; }
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public int EnvironmentCode { get; set; }
        public string EnvironmentDescription { get; set; }
        public string APIUserId { get; set; }
        public string SecretKey { get; set; }
        public string APIStatusCallbackURL { get; set; }
        public bool IsConfigured { get; set; }
        public bool IsREST { get; set; }
        public bool IsSOAP { get; set; }

        public bool IsPartnerConfirmWhitelisted { get; set; }
        public List<WhitelistIPAddressOutputDTO> APIAccessWhitelistIps { get; set; }
    }
}
