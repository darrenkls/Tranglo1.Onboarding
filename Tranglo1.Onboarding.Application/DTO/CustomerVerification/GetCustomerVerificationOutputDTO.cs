using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class GetCustomerVerificationOutputDTO
    {
        public long? CustomerVerificationCode { get; set; }
        public long? VerificationIDTypeCode { get; set; }
        public string VerificationIDTypeDescription { get; set; }
        public long? EKYCVerificationStatusCode { get; set; }
        public string EKYCVerificationStatusDescription { get; set; }
        public long? F2FVerificationStatusCode { get; set; }
        public string F2FVerificationStatusDescription { get; set; }
        public string JustificationRemark { get; set; }
        public long? RiskScoreCode { get; set; }
        public string RiskScoreDescription { get; set; }
        public long? RiskTypeCode { get; set; }
        public string RiskTypeDescription { get; set; }
        public Guid? TemplateID { get; set; }
        public long? SubmissionCount { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public Guid? CustomerVerificationConcurrencyToken { get; set; }
        public List<GetCustomerVerificationDocumentOutputDTO> GetCustomerVerificationDocuments { get; set; }
    }
}
