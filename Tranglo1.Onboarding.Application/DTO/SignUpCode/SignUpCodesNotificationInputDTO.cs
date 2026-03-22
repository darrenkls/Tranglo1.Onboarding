using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.SignUpCode
{
    public class SignUpCodesNotificationInputDTO
    {
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public long PartnerCode { get; set; }
    }
}
