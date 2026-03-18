using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class LicenseInformation : Entity
    {
        public bool? IsLicenseRequired { get; set; }
        public string LicenseType { get; set; }
        public string LicenseCertNumber { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string PrimaryRegulatorLicenseService  { get; set; }
        public string PrimaryRegulatorAMLCFT { get; set; }
        public string ActLawRemittanceLicense { get; set; }
        public string ActLawAMLCFT { get; set; }
        //public string LastModifiedBy { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public int BusinessProfileCode { get; set; }
        public string Remark { get; set; }
        public Guid? RegulatorDocumentId { get; set; }
        public string RegulatorDocumentName { get; set; }
        public string RegulatorWebsite { get; set; }

        // For saving license info
        public Guid? LicenseInfoConcurrencyToken { get; set; }

        private LicenseInformation()
        {

        }
        public LicenseInformation(BusinessProfile businessProfile)
        {
            this.BusinessProfile = businessProfile;
            this.BusinessProfileCode = businessProfile.Id;
        }
    }
}
