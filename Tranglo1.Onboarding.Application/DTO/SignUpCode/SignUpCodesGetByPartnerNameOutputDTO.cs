using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.SignUpCode
{
    public class SignUpCodesGetByPartnerNameOutputDTO
    {
        public string CompanyName { get; set; }
        public string Agent { get; set; }
        public long PartnerCode { get; set; }
        public bool IsTrangloConnectExist { get; set; }
        public bool IsTrangloBusinessExist { get; set; }
    }
}
