using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.SignUpCode
{
    public class SignUpCodesInputDTO
    {
        public string CompanyName { get; set; }
        public int LeadsOriginCode { get; set; }
        public string AgentLoginId { get; set; }
        public long partnerCode { get; set; }
        public string entity { get; set; }
        public bool IsTrangloConnectExist { get; set; }
        public bool IsTrangloBusinessExist { get; set; }
        public long SolutionCode { get; set; }
    }
}
