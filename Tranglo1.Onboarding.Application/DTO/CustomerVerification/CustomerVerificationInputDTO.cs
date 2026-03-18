using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class CustomerVerificationInputDTO
    {
        //public long? CustomerVerificationCode { get; set; }
        public long? EKYCVerificationStatusCode { get; set; }
        public long? F2FVerificationStatusCode { get; set; }
        public long? VerificationIDTypeCode { get; set; }
        public string JustificationRemarks { get; set; }


    }
}
