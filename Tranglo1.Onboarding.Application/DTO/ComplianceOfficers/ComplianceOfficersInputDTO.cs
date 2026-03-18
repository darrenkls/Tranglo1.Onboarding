using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ComplianceOfficers
{
    public class ComplianceOfficersInputDTO
    {
        public bool? IsRegisteredRegulator { get; set; }
        public bool? IsCertifiedByAML { get; set; }

        [RegularExpression("^[a-zA-Z\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphabetic and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Compliance officer name maximum length is 150 character")]
        public string ComplianceOfficer { get; set; }

        [RegularExpression("^[a-zA-Z\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphabetic and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Position title maximum length is 150 character")]
        public string PositionTitle { get; set; }

        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Company address maximum length is 150 character")]
        public string CompanyAddress { get; set; }

        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        [MaxLength(15, ErrorMessage = "Zipcode maximum length is 15 character")]
        public string ZipCodePostCode { get; set; }

        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only numerics Allowed")]
        [MaxLength(15, ErrorMessage = "Contact number maximum length is 15 character")]
        public string ContactNumber { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
  
        [RegularExpression("^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$", ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150, ErrorMessage = "Email address maximum length is 150 character")]
        public string EmailAddress { get; set; }

        [RegularExpression("^[a-zA-Z\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphabetic and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Reporting to maximum length is 150 character")]
        public string ReportingTo { get; set; }

        [MaxLength(150, ErrorMessage = "Certification program maximum length is 150 character")]
        public string CertificationProgram { get; set; }

        [MaxLength(150, ErrorMessage = "Certification body organization maximum length is 150 character")]
        public string CertificationBodyOrganization { get; set; }
    }
}
