using CSharpFunctionalExtensions;
using System;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class COInformation : Entity
    {
        public string ComplianceOfficer { get; set; }
        public string PositionTitle { get; set; }
        public string CompanyAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public int CallingCode { get; set; }
        public ContactNumber ContactNumber { get; set; }
        public Email EmailAddress { get; set; }
        public string ReportingTo { get; set; }
        public bool? IsRegisteredRegulator { get; set; }
        public bool? IsCertifiedByAML { get; set; }
        public string CertificationProgram { get; set; }
        public string CertificationBodyOrganization { get; set; }
        //public DateTime? DateCreated { get; set; }
        //public DateTime? LastModifiedDate { get; set; }
        //public string LastModifiedBy { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public int BusinessProfileCode { get; set; }
        public Guid CoSignatureDocumentId { get; set; }
        public string COSignatureDocumentName { get; set; }

        // Concurrency Token
        public Guid? COInformationConcurrencyToken { get; set; }

        private COInformation()
        {

        }
        public COInformation(BusinessProfile businessProfile)
        {
            this.BusinessProfile = businessProfile;
            this.BusinessProfileCode = businessProfile.Id;
        }

       


    }
}
