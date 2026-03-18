using System;

namespace Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.AuthorisedPerson
{
    public class AuthorisedPersonOutputDTO
    {
        public long? AuthorisedPersonCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public string FullName { get; set; }
        public long? AuthorisationLevelCode { get; set; }
        public string AuthorisationLevelDescription { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NationalityISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IdTypeDescription { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public bool isCompleted { get; set; }
        public long? ShareholderCode { get; set; }
        public bool IsDefault { get; set; }
        public Guid? AuthorisedPersonConcurrencyToken { get; set; }
        public string PositionTitle { get; set; }
        public string TelephoneDialCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string TelephoneNumberCountryISO2 { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }

    }
}
