using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.BusinessProfile
{
    public class BusinessProfileApprovalHistoryOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegisteredCountryISO2 { get; set; }
        public string CountryDescription { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? RejectDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public string ComplianceOfficerLoginId { get; set; }
    }
}
