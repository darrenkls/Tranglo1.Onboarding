using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.LicenseInformation
{
    public class LicenseInformationOutputDTO
    {
        public bool? IsLicenseRequired { get; set; }
        public string LicenseType { get; set; }
        public string LicenseCertNumber { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string PrimaryRegulatorLicenseService { get; set; }
        public string PrimaryRegulatorAMLCFT { get; set; }
        public string ActLawRemittanceLicense { get; set; }
        public string ActLawAMLCFT { get; set; }
        public int BusinessProfileCode { get; set; }
        public int LicenseInformationCode { get; set; }
        public string Remark { get; set; }
        public Guid? RegulatorDocumentId { get; set; }
        public string RegulatorDocumentName { get; set; }
        public long FileSizeBytes { get; set; }
        public string RegulatorWebsite { get; set; }
        public Guid? LicenseInfoConcurrencyToken { get; set; }
    }
}