using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.LicenseInformation
{
    public class LicenseInformationInputDTO
    {
        public bool? IsLicenseRequired { get; set; }

        [MaxLength(150, ErrorMessage = "Type of license of registration maximum length is 150 character")]
        public string LicenseType { get; set; }

        [MaxLength(150, ErrorMessage = "License Certification number maximum length is 150 character")]
        public string LicenseCertNumber { get; set; }

        [MaxLength(150, ErrorMessage = "Primary Regulator License Service maximum length is 150 character")]
        public string PrimaryRegulatorLicenseService { get; set; }

        [MaxLength(150, ErrorMessage = "Primary Regulator AML CFT maximum length is 150 character")]
        public string PrimaryRegulatorAMLCFT { get; set; }

        [MaxLength(500, ErrorMessage = "Act/Law for Remittance License maximum length is 500 character")]
        public string ActLawRemittanceLicense { get; set; }

        [MaxLength(500, ErrorMessage = "Act/Law for the AML/CFT maximum length is 500 character")]
        public string ActLawRemittanceAMLCFT { get; set; }
        public DateTime? IssuedDatetime { get; set; }
        public DateTime? ExpiredDatetime { get; set; }

        [MaxLength(2000, ErrorMessage = "Remark for Remittance License maximum length is 2000 character")]
        public string Remark { get; set; }
        public string RegulatorWebsite { get; set; }
        public long? AdminSolution { get; set; }
    }
}
