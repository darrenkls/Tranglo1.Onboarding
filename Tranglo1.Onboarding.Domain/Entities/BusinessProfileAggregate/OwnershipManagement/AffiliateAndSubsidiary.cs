using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AffiliateAndSubsidiary : Entity
    {
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public CountryMeta Country { get; set; }
        public BusinessProfile BusinessProfile { get;  private set; }
        public string Relationship { get; set; }


        private AffiliateAndSubsidiary()
        {

        }
/*        public void SetCountry(CountryMeta countryMeta)
        {
            if (this.Country != countryMeta)
            {
                this.Country = countryMeta;
            }
        }*/

        public AffiliateAndSubsidiary(BusinessProfile businessProfile ,string companyName, string companyRegNo, 
                                      DateTime? dateOfIncorporation , CountryMeta country, string relationship)
        {
            this.BusinessProfile = businessProfile;
            this.CompanyName = companyName;
            this.CompanyRegNo = companyRegNo;
            this.DateOfIncorporation = dateOfIncorporation;
            this.Country = country;
            this.Relationship = relationship;
        }

        public bool IsCompleted()
        {
            if (string.IsNullOrEmpty(CompanyName) ||
                       !DateOfIncorporation.HasValue ||
                       string.IsNullOrEmpty(CompanyRegNo) ||
                       Country == null ||
                       string.IsNullOrEmpty(Relationship))
            {
                return false;
            }
            return true;
        }
    }
}

