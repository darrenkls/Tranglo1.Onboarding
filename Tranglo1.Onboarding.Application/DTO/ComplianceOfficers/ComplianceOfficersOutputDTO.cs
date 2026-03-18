using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ComplianceOfficers
{
    public class ComplianceOfficersOutputDTO
    {
        public int COInformationCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public bool? IsRegisteredRegulator { get; set; }
        public bool? IsCertifiedByAML { get; set; }
        public string ComplianceOfficer { get; set; }
        public string PositionTitle { get; set; }
        public string CompanyAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public string ContactNumber { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string EmailAddress { get; set; }
        public string ReportingTo { get; set; }
        public string CertificationProgram { get; set; }
        public string CertificationBodyOrganization { get; set; }
        public string COSignatureDocumentId { get; set; }
        public string COSignatureDocumentName { get; set; }
        public Guid? COInformationConcurrencyToken { get; set; }
    }
}
