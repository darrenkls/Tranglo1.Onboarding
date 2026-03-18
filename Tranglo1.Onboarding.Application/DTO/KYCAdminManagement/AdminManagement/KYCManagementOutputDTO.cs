using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCManagementOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public string PartnerName { get; set; }
        public string FullRegisteredCompanyLegalName { get; set; }
        public string CountryISO2 { get; set; }
        public string CountryDescription { get; set; }
        public string ComplianceOfficerAssignedLoginId { get; set; }
        public string ComplianceOfficerAssignedName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public long WorkFlowStatusCode { get; set; }
        public string WorkFlowStatusDescription { get; set; }
        public long? CustomerTypeCode { get; set; }
        public string CustomerTypeDescription { get; set; }
        public long KYCStatusCode { get; set; }
        public string KYCStatusDescription { get; set; }
        public long SolutionCode { get; set; }
        public string SolutionDescription { get; set; }       
    }
}
