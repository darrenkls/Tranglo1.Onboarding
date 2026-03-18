using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO
{
    public class CustomerVerificationTemplateOutputDTO
    {
        public long? CustomerVerificationCode { get; set; }
        public long? EKYCVerificationStatusCode { get; set; }
        public string EKYCVerificationStatusDescription { get; set; }
        public long? F2FVerificationStatusCode { get; set; }
        public string F2FVerificationStatusDescription { get; set; }
        public long? VerificationIDTypeCode { get; set; }
        public string VerificationIDTypeDescription { get; set; }
        public string JustificationRemark { get; set; }
    }
}
