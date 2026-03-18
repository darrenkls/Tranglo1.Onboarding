using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAPISettingsUserUpdateInputDTO
    {
        public UpdatePartnerAPISettingsUser Staging { get; set; }
        public UpdatePartnerAPISettingsUser Production { get; set; }
        public bool IsPartnerConfirmWhitelisted { get; set; }
    }

    public class UpdatePartnerAPISettingsUser
    {
        public int PartnerAPISettingId { set; get; }
        public string APIUserId { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; }
    }
}