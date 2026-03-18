using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCApprovalDetailsOutputDTO
    {
        public string PartnerName { get; set; }
        public string CountryISO2 { get; set; }
        public string CountryDescription { get; set; }
        public string ComplianceOfficerAssignedLoginId { get; set; }
        public string ComplianceOfficerAssignedName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public long WorkFlowStatusCode { get; set; }
        public string WorkFlowStatusDescription { get; set; }
        public long KYCStatusCode { get; set; }
        public string KYCStatusDescription { get; set; }

        public DateTime? KYCSubmissionDate { get; set; }
        public long? PartnerTypeCode { get; set; }
        public string PartnerTypeDescription { get; set; }

        public long? CustomerTypeCode { get; set; }
        public string CustomerTypeDescription { get; set; }
        public long? SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public DateTime? BusinessKYCSubmissionDate { get; set; }
        public Guid? ReviewAndFeedbackConcurrencyToken { get; set; }
        public DateTime? ReviewAndFeedbackConcurrentLastModified { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
    }
}