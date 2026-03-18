using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class GetCustomerVerificationDocumentOutputDTO
    {
        public long? CustomerVerificationCode {get;set;}
        public long? CustomerVerificationDocumentCode {get;set;}
        public Guid? WatermarkDocumentID { get; set; }
        public string WatermarkDocumentName { get; set; }
        public long? VerificationIDTypeSectionCode { get; set; }
        public string VerificationIDTypeSectionDescription { get; set; }
        public DateTime? UploadDateTime { get; set; }
        public long? SubmissionResultCode { get; set; }
        public string SubmissionResultDescription { get; set; }
    }
}
