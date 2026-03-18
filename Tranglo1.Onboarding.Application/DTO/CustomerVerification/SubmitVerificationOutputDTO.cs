using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class SubmitVerificationOutputDTO
    {
        public long CustomerVerificationCode { get; set; }
        public long? EKYCVerificationStatusCode { get; set; }
        public string EKYCVerificationStatusDescription { get; set; }       
        public long? F2FVerificationStatusCode { get; set; }
        public string F2FVerificationStatusDescription { get; set; }
        public List<CustomerVerificationDocumentList> CustomerVerificationDocuments { get; set; }
    }

    public class CustomerVerificationDocumentList
    {
        public long CustomerVerificationDocumentCode { get; set; }
        public long? SubmissionResultCode { get; set; }
        public string SubmissionResultDescription { get; set; }
    }
}
