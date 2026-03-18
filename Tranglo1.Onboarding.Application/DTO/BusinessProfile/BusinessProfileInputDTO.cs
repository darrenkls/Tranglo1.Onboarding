using System;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.DTO.BusinessProfile
{
    public class BusinessProfileInputDTO
    {
        [MaxLength(150, ErrorMessage = "Company Name maximum length is 150 character")]
        //[RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Only Alphanumerics Allowed.")]
        public string CompanyName { get; set; }
        [MaxLength(500, ErrorMessage = "Registered Company Name maximum length is 500 character")]
        //[RegularExpression("^[ A-Za-zÀ-ÖØ-öø-įĴ-őŔ-žǍ-ǰǴ-ǵǸ-țȞ-ȟȤ-ȳɃɆ-ɏḀ-ẞƀ-ƓƗ-ƚƝ-ơƤ-ƥƫ-ưƲ-ƶẠ-ỿÀ-ž0-9\\W|_-]*$", ErrorMessage = "Chinese Character is not Allowed")]
        public string CompanyRegistrationName { get; set; }
        [MaxLength(150, ErrorMessage = "Trade Name maximum length is 150 character")]
        //[RegularExpression("^[ A-Za-z0-9_!@$%^*=~,./#&+\\s+-]*$", ErrorMessage = "Error Saving Business Profile.")]
        public string TradeName { get; set; }
        [MaxLength(500, ErrorMessage = "Registered Company Address maximum length is 500 characters")]
        //[RegularExpression("^[ A-Za-z0-9_!@$%^*=~,./#&+\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string CompanyRegisteredAddress { get; set; }
        //[MaxLength(15, ErrorMessage = "Registered Company Name ZipCode/Postcode length is 15 character")]
        [RegularExpression("^[^ ]*$", ErrorMessage = "Only Alphanumerics and Symbol are Allowed.")]
        public string CompanyRegisteredZipCodePostCode { get; set; }
        public string CompanyRegisteredCountryISO2 { get; set; }
        [MaxLength(500, ErrorMessage = "Principal Place of Business Address maximum length is 150 character")]
        // [RegularExpression("^[ A-Za-z0-9_!@$%^*=~,./#&+\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string MailingAddress { get; set; }
        [MaxLength(15, ErrorMessage = "MailingZipCode/Postcode length is 15 character")]
        public string MailingZipCodePostCode { get; set; }
        public string MailingCountryISO2 { get; set; }
        public long? BusinessNatureCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        /*public string CorporationType { get; set; }*/
        [RegularExpression("^[ A-Za-z0-9\u00C0-\u017F\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric, symbols and special characters  are allowed.")]
        [MaxLength(150, ErrorMessage = "Company Registration No maximum length is 150 character")]
        public string CompanyRegistrationNo { get; set; }
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only numerics Allowed")]
        public int? NumberOfBranches { get; set; }
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only numerics Allowed")]
        public string ContactNumber { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        [RegularExpression("^[ A-Za-z0-9\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string Website { get; set; }
        public bool? IsCompanyListed { get; set; }
        [RegularExpression("^[ A-Za-z0-9\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        [MaxLength(150, ErrorMessage = "Stock Exchange Name maximum length is 150 character")]
        public string StockExchangeName { get; set; }
        [RegularExpression("^[ A-Za-z0-9\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        [MaxLength(150, ErrorMessage = "Stock Code maximum length is 150 character")]
        public string StockCode { get; set; }

        public bool? IsMoneyTransferRemittance { get; set; }
        public bool? IsForeignCurrencyExchange { get; set; }
        public bool? IsRetailCommercialBankingServices { get; set; }
        public bool? IsForexTrading { get; set; }
        public bool? IsEMoneyEwallet { get; set; }
        public bool? IsIntermediataryRemittance { get; set; }
        public bool? IsCryptoCurrency { get; set; }
        public bool? IsOther { get; set; }
        public string OtherReason { get; set; }

        [MaxLength(150, ErrorMessage = "Former Registered Company Name maximum length is 150 character")]
        //[RegularExpression("^[ A-Za-zÀ-ÖØ-öø-įĴ-őŔ-žǍ-ǰǴ-ǵǸ-țȞ-ȟȤ-ȳɃɆ-ɏḀ-ẞƀ-ƓƗ-ƚƝ-ơƤ-ƥƫ-ưƲ-ƶẠ-ỿÀ-ž0-9\\W|_-]*$", ErrorMessage = "Chinese Character is not Allowed")]
        public string FormerRegisteredCompanyName { get; set; }
        [MaxLength(150, ErrorMessage = "Registered Company Name maximum length is 150 character")]
        public string ForOthers { get; set; }
        public int? EntityTypeCode { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
        [MaxLength(150, ErrorMessage = "Tax Identification No maximum length is 150 character")]
        [RegularExpression("^[ A-Za-z0-9\u00C0-\u017F\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric, symbols and special characters  are allowed.")]
        public string TaxIdentificationNo { get; set; }
        public string FacsimileDialCode { get; set; }
        [MaxLength(15, ErrorMessage = "FacsimileNumber maximum length is 15 character")]
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only numerics Allowed")]
        public string FacsimileNumber { get; set; }
        public string FacsimileNumberCountryISO2 { get; set; }

        public string TelephoneDialCode { get; set; }
        [MaxLength(15, ErrorMessage = "Telephone maximum length is 15 character")]
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only numerics Allowed")]
        public string TelephoneNumber { get; set; }
        public string TelephoneNumberCountryISO2 { get; set; }

        [MaxLength(150, ErrorMessage = "Contact Person Name length is 150 character")]
        //[RegularExpression("^[ A-Za-z0-9\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string ContactPersonName { get; set; }
        [MaxLength(150, ErrorMessage = "Email maximum length is 150 character")]
        [RegularExpression("^[ A-Za-z0-9\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string ContactEmailAddress { get; set; }


        //Phase 3 Changes 
        public long? CustomerTypeCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? BusinessProfileIDTypeCode { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public long? ServiceTypeCode { get; set; }
        public long? CollectionTierCode { get; set; }
        public string AliasName { get; set; }
        public string NationalityISO2 { get; set; }
        public bool? IsMicroEnterprise { get; set; }

        //Ticket 55839
        [MaxLength(17, ErrorMessage = "SST Registration Number length is 17 character")]
        [RegularExpression("^[a-zA-Z0-9/-]+$", ErrorMessage = "Only Alphanumerics and En Dash Symbols Allowed.")]
        public string SSTRegistrationNumber { get; set; }
        public string SenderCity { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }

    }
}
