using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCSubCategory : Enumeration
    {
        public long? KycCategory { get; set; }

        public KYCSubCategory() : base()
        {
        }

        public KYCSubCategory(int id, string name, long? kyccategory)
            : base(id, name)
        {
            this.KycCategory = kyccategory;
        }
    
        public static readonly KYCSubCategory Business_Profile_Address = new KYCSubCategory(1, "Address", KYCCategory.Business_BusinessProfile.Id);
        public static readonly KYCSubCategory Business_Ownership_AuthorisedPerson = new KYCSubCategory(2, "Authorised person", KYCCategory.Business_Ownership.Id);
        public static readonly KYCSubCategory Business_Ownership_BOD = new KYCSubCategory(3, "Board of director", KYCCategory.Business_Ownership.Id);
        public static readonly KYCSubCategory Business_Profile_CompanyContact = new KYCSubCategory(4, "Company Contact", KYCCategory.Business_BusinessProfile.Id);
        public static readonly KYCSubCategory Business_Profile_CompanyDetails = new KYCSubCategory(5, "Company Details", KYCCategory.Business_BusinessProfile.Id);
        public static readonly KYCSubCategory Business_Profile_ContactPerson = new KYCSubCategory(6, "Contact Person", KYCCategory.Business_BusinessProfile.Id);
        public static readonly KYCSubCategory Business_Ownership_ShareHolder = new KYCSubCategory(7, "Shareholder", KYCCategory.Business_Ownership.Id);
        public static readonly KYCSubCategory Business_Ownership_UBO = new KYCSubCategory(8, "Ultimate beneficial owner", KYCCategory.Business_Ownership.Id);
        public static readonly KYCSubCategory Business_Ownership_PrincipalOfficer = new KYCSubCategory(18, "Principal officer", KYCCategory.Business_Ownership.Id);

        public static readonly KYCSubCategory Connect_Profile_CompanyDetails = new KYCSubCategory(9, "Company details", KYCCategory.Connect_BusinessProfile.Id);
        public static readonly KYCSubCategory Connect_Profile_Address = new KYCSubCategory(10, "Address", KYCCategory.Connect_BusinessProfile.Id);
        public static readonly KYCSubCategory Connect_Profile_ContactPerson = new KYCSubCategory(11, "Contact Person", KYCCategory.Connect_BusinessProfile.Id);
        public static readonly KYCSubCategory Connect_Ownership_Shareholder = new KYCSubCategory(12, "Shareholder", KYCCategory.Connect_Ownership.Id);
        public static readonly KYCSubCategory Connect_Ownership_UltimateOwner = new KYCSubCategory(13, "Ultimate beneficial owner", KYCCategory.Connect_Ownership.Id);
        public static readonly KYCSubCategory Connect_Ownership_BoardOfDirector = new KYCSubCategory(14, "Board of directors", KYCCategory.Connect_Ownership.Id);
        public static readonly KYCSubCategory Connect_Ownership_PrincipalOfficer = new KYCSubCategory(15, "Principal officer", KYCCategory.Connect_Ownership.Id);
        public static readonly KYCSubCategory Connect_Ownership_AuthorisedPerson = new KYCSubCategory(16, "Authorised person", KYCCategory.Connect_Ownership.Id);
        public static readonly KYCSubCategory Connect_Ownership_LicensedParentCompany = new KYCSubCategory(17, "Licensed parent company / Subsidiary and Affiliate", KYCCategory.Connect_Ownership.Id);

        public static List<KYCSubCategory> GetBusinessCustomerUserAllowedSubCategoriesForBusinessProfileCategory()
        {
            var allowedSubCategory = new List<KYCSubCategory>
            {
                Business_Profile_CompanyDetails,
                Business_Profile_Address,
                Business_Profile_CompanyContact,
                Business_Profile_ContactPerson
            };

            return allowedSubCategory;
        }

        public static List<KYCSubCategory> GetBusinessCustomerUserAllowedSubCategoriesForOwnershipCategory(CustomerType customerType)
        {
            // Sequence is important as it will be used to display in UI, so do not change the sequence unless discussed with FE
            var allowedSubCategory = new List<KYCSubCategory>
            {
                Business_Ownership_ShareHolder,
                Business_Ownership_BOD,
                Business_Ownership_AuthorisedPerson,
                Business_Ownership_UBO
            };

            // For mass payout customer, principal officer is also required
            if (customerType == CustomerType.Corporate_Mass_Payout)
            {
                allowedSubCategory.Add(Business_Ownership_PrincipalOfficer);
            }

            return allowedSubCategory;
        }
    }

}
