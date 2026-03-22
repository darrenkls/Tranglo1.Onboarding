using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.SignUpCode
{
    public class SignUpCodesOutputDTO
    {
        public string Agent { get; set; }
        public string CompanyName { get; set; }
        public int LeadsOriginCode { get; set; }
        public string LeadsOriginDesc { get; set; }
        public string Code { get; set; }
        public long partnerCode { get; set; }
        public string SolutionCode { get; set; }
        public long signupCode { get; set; }
        public int SignUpCodeStatusCode { get; set; }
        public string SignUpCodesStatusDesc { get; set; }
        public DateTime? ExpireAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
