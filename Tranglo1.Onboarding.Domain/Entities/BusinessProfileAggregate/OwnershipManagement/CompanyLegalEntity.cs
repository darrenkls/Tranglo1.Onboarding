using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CompanyLegalEntity : LegalEntity
    {
        public CountryMeta Country { get; set; }
        public DateTime? DateOfIncorporation { get; set; }

/*        public void SetCountry (CountryMeta country)
        {
            if (this.Country != country) 
            {
                this.Country = country;
            }
        }*/


        private CompanyLegalEntity()
        {

        }

        public CompanyLegalEntity(BusinessProfile businessProfile, string companyName, string companyRegNo,
                                    string nameOfShareAboveTenPercent, string percentageOfOwnership, DateTime? dateOfIncorporation, CountryMeta country) :
            base (businessProfile, companyName, companyRegNo, nameOfShareAboveTenPercent, percentageOfOwnership)
        {
            this.Country = country;
            this.DateOfIncorporation = dateOfIncorporation;
        }

        public bool IsTCCompleted()
        {
            if (string.IsNullOrEmpty(CompanyName) ||
                        string.IsNullOrEmpty(NameOfSharesAboveTenPercent) ||
                        string.IsNullOrEmpty(CompanyRegNo) ||
                        string.IsNullOrEmpty(EffectiveShareholding) ||
                         Country == null ||
                         DateOfIncorporation == null)

            {
                return false;
            }

            return true;
        }

        public bool IsTBCompleted()
        {
            if (string.IsNullOrEmpty(CompanyName) ||
                        string.IsNullOrEmpty(NameOfSharesAboveTenPercent) ||
                        string.IsNullOrEmpty(CompanyRegNo) ||
                        string.IsNullOrEmpty(EffectiveShareholding) ||
                         Country == null ||
                         DateOfIncorporation == null)

            {
                return false;
            }

            return true;
        }
    }
}
