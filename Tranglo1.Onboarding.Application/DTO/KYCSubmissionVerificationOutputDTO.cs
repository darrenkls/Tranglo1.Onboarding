using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO
{
    public class KYCSubmissionVerificationOutputDTO
    {
        public long? KYCSubmissionStatusCode { get; set; }
        public string KYCSubmissionStatusName { get; set; }
        public long? BusinessProfileCode { get; set; }
    }
}
