using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class GetCustomerVerificationRiskResultsOutputDTO
    {
        public long? CustomerVerificationCode { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public long? SubmissionCount { get; set; }
    }
}
