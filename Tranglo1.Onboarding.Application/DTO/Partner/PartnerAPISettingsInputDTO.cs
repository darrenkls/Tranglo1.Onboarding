using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAPISettingsInputDTO
    {
        public PartnerAPISettings Staging { get; set; }
        public PartnerAPISettings Production { get; set; }
        public bool IsPartnerConfirmWhitelisted { get; set; }
    }

    public class PartnerAPISettings
    {
        public int PartnerAPISettingID { get; set; }
        public string APIStatusCallbackURL { get; set; }
        public string APIUserId { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; }
        public List<IPAddress> IPAddress { get; set; }
        public bool IsREST { get; set; }
        public bool IsSOAP { get; set; }
    }

    public class IPAddress
    {
        public int WhitelistIPId { get; set; }
        public string IPAddressStart { get; set; }
        public string IPAddressEnd { get; set; }
        public bool IsRangeIPAddress { get; set; }
        public int EnvironmentCode { get; set; }
    }
}