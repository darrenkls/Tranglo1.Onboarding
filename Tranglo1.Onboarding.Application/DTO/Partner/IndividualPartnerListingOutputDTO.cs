using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class IndividualPartnerListingOutputDTO
    {
        public long PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string RegisteredCompanyName { get; set; }
        public long? CustomerTypeCode { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public string TradeName { get; set; }
        public string Email { get; set; }
        public string CompanyRegisteredNo { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string DialCode { get; set; }
        public string ContactNumber { get; set; }
        public string IMID { get; set; }
        public long? BusinessNature { get; set; }
        public string CompanyAddress { get; set; }
        public string ZipcodePostcode { get; set; }
        public string CountryISO2 { get; set; }
        public string MailingAddress { get; set; }
        public string Entity { get; set; }
        //public long PartnerType { get; set; }
        //public long Solution { get; set; }
        //public string Currency { get; set; }
        //public string Timezone { get; set; }
        //public long PricePackageCode { get; set; }
        public string Agent { get; set; }
        public string MailingCountry { get; set; }
        public string MailingZipCodePostCode { get; set; }
        public string CompanyRegisteredCountryISO2 { get; set; }
        public string CompanyRegisteredZipCodePostCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public string ProductLoginId { get; set; }
        public string SalesOperationLoginId { get; set; }
		//public bool DisplayDefaultPackage { get; set; }

        public string FormerRegisteredCompanyName { get; set; }
        public string TaxIdentificationNo { get; set; }
        public string ContactPersonName { get; set; }

        public string TelephoneDialCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string TelephoneNumberCountryISO2 { get; set; }
        public string FacsimileDialCode { get; set; }
        public string FacsimileNumber { get; set; }
        public string FacsimileNumberCountryISO2 { get; set; }
        public string ForOthers { get; set; }
        public string AliasName { get; set; }
        public string Nationality { get; set; }
        public string NationalityISO2 { get; set; }
    }
}