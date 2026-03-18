using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class CustomerVerificationOutputDTO
    {
        public long? CustomerVerificationCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public long? EKYCVerificationStatusCode { get; set; }
        public string EKYCVerificationStatusDescription { get; set; }
        public long? F2FVerificationStatusCode { get; set; }
        public string F2FVerificationStatusDescription { get; set; }
        public long? VerificationIDTypeCode { get; set; }
        public string VerificationIDTypeDescription { get; set; }
        public string JustificationRemark { get; set; }
    }



}
