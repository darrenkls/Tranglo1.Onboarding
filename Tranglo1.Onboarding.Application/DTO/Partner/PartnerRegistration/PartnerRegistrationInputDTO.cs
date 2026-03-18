using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration
{
    public class PartnerRegistrationInputDTO
    {
        [MaxLength(150, ErrorMessage = "Registered Company Name maximum length is 150 character")]
        //[Required(ErrorMessage ="Company Registered Name is required")]
        [RegularExpression("^[ A-Za-zÀ-ÖØ-öø-įĴ-őŔ-žǍ-ǰǴ-ǵǸ-țȞ-ȟȤ-ȳɃɆ-ɏḀ-ẞƀ-ƓƗ-ƚƝ-ơƤ-ƥƫ-ưƲ-ƶẠ-ỿÀ-ž0-9\\W|_-]*$", ErrorMessage = "Chinese Character is not Allowed")]
        public string RegisteredCompanyName { get; set; }

        [MaxLength(150, ErrorMessage = "Trade Name maximum length is 150 character")]
        public string TradeName { get; set; }

        /*      [MaxLength(150, ErrorMessage = " Company Registered No maximum length is 150 character")]
              public string CompanyRegisteredNo { get; set; }*/
        [MaxLength(150, ErrorMessage = "Contact Email Address maximum length is 150 character")]
        //[RegularExpression("^[ A-Za-z0-9_@./#&+- ]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string Email { get; set; }
        [MaxLength(15, ErrorMessage = "Contact Number maximum length is 15 characters")]
        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression("^[0-9 ]*$", ErrorMessage = "Only Numbers are Allowed.")]
        public string ContactNumber { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string IMID { get; set; }
        public long? BusinessNature { get; set; }

        [MaxLength(15, ErrorMessage = " ZipCode/PostCode maximum length is 15 characters")]
        public string ZipCodePostCode { get; set; }

        public string CountryISO2 { get; set; }

        //[Required(ErrorMessage = " Tranglo Entity Type is required")]
        public string Entity { get; set; }
        //public long? PartnerType { get; set; }

        [MaxLength(150, ErrorMessage = " Company Address maximum length is 150 character")]
        //[RegularExpression("^[ A-Za-z0-9_!@$%^*=~,./#&+\\s+-]*$", ErrorMessage = "Only Alphanumerics and Symbols Allowed.")]
        public string CompanyAddress { get; set; }


        //public long? Solution { get; set; }

        //[Required(ErrorMessage = " Type of Currency is required")]
        //public string Currency { get; set; }

/*        public string TimeZone { get; set; }
*/
        [Required(ErrorMessage = "Tranglo Agent Name is required")]
        public string Agent { get; set; }

        //public long? PricePackageCode { get; set; }

        public string PartnerName { get; set; }
		//public bool DisplayDefaultPackage { get; set; }

        //New Fields
        [MaxLength(150, ErrorMessage = "Contact Person Name maximum length is 150 character")]
        public string ContactPersonName { get; set; }
        [MaxLength(150, ErrorMessage = "For Others maximum length is 150 character")]
        [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only Alphabets Allowed.")]
        public string ForOthers { get; set; }

        //Phase 3 Additional Fields
        public long? CustomerTypeCode { get; set; }
        public string FullName { get; set; }
        public string PersonInChargeName { get; set; }
        public string AliasName { get; set; }
        public string NationalityISO2 { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public string FormerRegisteredCompanyName { get; set; }


        public List<long?> SolutionTypeCode { get; set; }




    }
}
