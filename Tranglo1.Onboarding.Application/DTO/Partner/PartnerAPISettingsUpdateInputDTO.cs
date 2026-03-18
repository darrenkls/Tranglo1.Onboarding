using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAPISettingsUpdateInputDTO
    {
        public UpdatePartnerAPISettings Staging { get; set; }
        public UpdatePartnerAPISettings Production { get; set; }
        public bool IsPartnerConfirmWhitelisted { get; set; }
    }

    public class UpdatePartnerAPISettings
    {
        public int PartnerAPISettingId { set; get; }
        public string APIStatusCallbackURL { get; set; }
        public List<IPAddress> IPAddress { get; set; }
        public bool IsREST { get; set; }
        public bool IsSOAP { get; set; }
    }
}