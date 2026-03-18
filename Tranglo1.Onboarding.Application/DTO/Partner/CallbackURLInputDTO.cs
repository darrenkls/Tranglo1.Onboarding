using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class CallbackURLInputDTO
    {
        public CallbackURL Staging { get; set; }
        public CallbackURL Production { get; set; }
    }

    public class CallbackURL
    {
        public long PartnerAPISettingID { get; set; }
        public string APIStatusCallbackURL { get; set; }
    }
}