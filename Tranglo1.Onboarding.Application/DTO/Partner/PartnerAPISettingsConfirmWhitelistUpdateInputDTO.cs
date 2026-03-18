using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAPISettingsConfirmWhitelistUpdateInputDTO
    {
        public bool IsPartnerConfirmWhitelisted { get; set; }
        public long PartnerApiSettingId { get; set; }
    }
}