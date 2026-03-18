using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class CallbackURLOutputDTO
    {
        public long PartnerCode { get; set; }
        public long PartnerAPISettingId { get; set; }
        public string APIStatusCallbackURL { get; set; }
        public bool IsConfigured { get; set; }
        public int EnvironmentCode { get; set; }
        public string EnvironmentDescription { get; set; }
    }
}
