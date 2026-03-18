using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration
{
    public class UpdatePartnerInputDTO
    {
        public string addrRegex = "";
        [MaxLength(150, ErrorMessage = "Partner Name maximum length is 150 character")]
        [Required(ErrorMessage = "Partner Name is required")]
        public string PartnerName { get; set; }

        [MaxLength(150, ErrorMessage = "Registered Company Name maximum length is 150 character")]
        //[Required(ErrorMessage = "Company Registered Name is required")]
        [RegularExpression("^[ A-Za-zÀ-ÖØ-öø-įĴ-őŔ-žǍ-ǰǴ-ǵǸ-țȞ-ȟȤ-ȳɃɆ-ɏḀ-ẞƀ-ƓƗ-ƚƝ-ơƤ-ƥƫ-ưƲ-ƶẠ-ỿÀ-ž0-9\\W|_-]*$", ErrorMessage = "Chinese Character is not Allowed")]
        public string RegisteredCompanyName { get; set; }

        //[Required(ErrorMessage = "Trade  Name is required")]
        [MaxLength(150, ErrorMessage = "Trade Name maximum length is 150 character")]
        public string TradeName { get; set; }

        [MaxLength(150, ErrorMessage = " Company Registered No maximum length is 150 character")]
        public string CompanyRegisteredNo { get; set; }

        [MaxLength(150, ErrorMessage = "Contact Email Address maximum length is 150 character")]
        [Required(ErrorMessage = "Contact Email Address is required")]
        //[RegularExpression("^[ A-Za-z0-9_@./#&+- ]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string Email { get; set; }
        [MaxLength(15, ErrorMessage = "Contact Telephone maximum length is 15 characters")]
        [Required(ErrorMessage = "Contact Telephone is required")]
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only Numbers are Allowed.")]
        public string ContactNumber { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }

        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "IM Id maximum length is 150 character")]
        public string IMID { get; set; }
        public long? BusinessNature { get; set; }

        //[MaxLength(15, ErrorMessage = " PostCode maximum length is 15 characters")]
        //[Required(ErrorMessage = " Postcode is required")]
        public string ZipCodePostCode { get; set; }

        [Required(ErrorMessage = " Country is required")]
        public string CountryISO2 { get; set; }

        //[Required(ErrorMessage = " Tranglo Entity Type is required")]
        //public string Entity { get; set; }

        public long? PartnerCode { get; set; }

        //public long? PartnerType { get; set; }

        [MaxLength(150, ErrorMessage = " Company Address maximum length is 150 character")]
        //[RegularExpression(@"^[ A-Za-z0-9\\\w\\W\\s+]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string CompanyAddress { get; set; }


        //public long? Solution { get; set; }

        //[Required(ErrorMessage = " Type of Currency is required")]
        //public string Currency { get; set; }

        /*        public string TimeZone { get; set; }*/

        public string Agent { get; set; }

        //public long? PricePackageCode { get; set; }

        [MaxLength(150, ErrorMessage = " Company Mailing Address maximum length is 150 character")]
        //[RegularExpression(@"^[ A-Za-z0-9\\\w\\W\\s+]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string MailingAddress { get; set; }

        //[MaxLength(15, ErrorMessage = " Mailing PostCode maximum length is 15 characters")]
        public string MailingZipCodePostCode { get; set; }

        //[Required(ErrorMessage = " Country MailingISO2 is required")]
        public string MailingCountryISO2 { get; set; }

        public string ProductLoginId { get; set; }
        public string SalesOperationLoginId { get; set; }
        //public bool DisplayDefaultPackage { get; set; }

        //New Fields

        [MaxLength(150, ErrorMessage = "Contact Person Name maximum length is 150 character")]
        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        public string ContactPersonName { get; set; }
        [MaxLength(150, ErrorMessage = "For Others maximum length is 150 character")]
        [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only Alphabets Allowed.")]
        public string ForOthers { get; set; }
        [MaxLength(150, ErrorMessage = "Former Registered Company Name maximum length is 150 character")]
        /*        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Only Alphanumerics Allowed.")]
        */
        [RegularExpression("^[ A-Za-zÀ-ÖØ-öø-įĴ-őŔ-žǍ-ǰǴ-ǵǸ-țȞ-ȟȤ-ȳɃɆ-ɏḀ-ẞƀ-ƓƗ-ƚƝ-ơƤ-ƥƫ-ưƲ-ƶẠ-ỿÀ-ž0-9\\W|_-]*$", ErrorMessage = "Chinese Character is not Allowed")]
        public string FormerRegisteredCompanyName { get; set; }
        [MaxLength(150, ErrorMessage = "Tax Identification No maximum length is 150 character")]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Only Alphanumerics Allowed.")]
        //[Required(ErrorMessage = " Tax Identification No is required")] //#49869: Remove for both TC & TB 
        public string TaxIdentificationNo { get; set; }

        public string TelephoneDialCode { get; set; }
        [MaxLength(15, ErrorMessage = "Telephone Number maximum length is 15 characters")]
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only Numbers are Allowed.")]
        public string TelephoneNumber { get; set; }
        public string TelephoneNumberCountryISO2 { get; set; }
        public string FacsimileDialCode { get; set; }
        [MaxLength(15, ErrorMessage = "Facsimile Number maximum length is 15 characters")]
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only Numbers are Allowed.")]
        public string FacsimileNumber { get; set; }
        public string FacsimileNumberCountryISO2 { get; set; }

        //Phase 3
        public long? CustomerType { get; set; }

        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Former Name maximum length is 150 character")]
        public string FormerName { get; set; }

        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Former Name maximum length is 150 character")]
        public string PersonInChargeName { get; set; }

        [RegularExpression("^[a-zA-Z0-9\\p{S}\\W|_\\s+-]*$", ErrorMessage = "Only Alphanumeric and Symbols are allowed.")]
        [MaxLength(150, ErrorMessage = "Former Name maximum length is 150 character")]
        public string AliasName { get; set; }

        public string NationalityISO2 { get; set; }

        public long? RelationshipTieUp { get; set; }

    }
}
