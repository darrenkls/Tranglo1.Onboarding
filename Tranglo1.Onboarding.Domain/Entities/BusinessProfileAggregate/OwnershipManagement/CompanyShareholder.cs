using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CompanyShareholder : Shareholder
    {
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public CountryMeta Country { get; set; }

        private CompanyShareholder()
        {

        }
/*        public void SetCountry(CountryMeta country)
        {
            if (this.Country != country)
            {
                this.Country = country;
            }
        }*/

        public CompanyShareholder(BusinessProfile businessProfile, DateTime? dateOfIncorporation,string effectiveOfShareholding,
                                   string companyName, string companyRegNo, CountryMeta country,BoardOfDirector boardOfDirector,PrimaryOfficer primaryOfficer,
                                   AuthorisedPerson authorisedPerson):
            base(businessProfile, dateOfIncorporation, effectiveOfShareholding, boardOfDirector,primaryOfficer,authorisedPerson,null)
        {
                this.CompanyName = companyName;
                this.CompanyRegNo = companyRegNo;
                this.Country = country;
        }

        /// <summary>
        /// Currently for TB and TC Validation is same (for both TB and TC usage)
        /// </summary>
        /// <returns></returns>
        public bool IsCompleted()
        {
            if (string.IsNullOrEmpty(CompanyName) ||
                        !DateOfIncorporation.HasValue ||
                        Country == null ||
                        string.IsNullOrEmpty(CompanyRegNo) ||
                        string.IsNullOrEmpty(EffectiveShareholding))

            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if Ultiamte Shareholder (Individual/Company) is completed for both TB and TC usage
        /// </summary>
        public bool IsUltimateShareholderCompleted(
            Solution solution,
            IReadOnlyList<ShareholderIndividualLegalEntity> individualUltimateShareholders,
            IReadOnlyList<ShareholderCompanyLegalEntity> companyUltimateShareholders)
        {
            // Filter ultimate shareholders for this company shareholder
            individualUltimateShareholders = individualUltimateShareholders.Where(x => x.Shareholder.Id == Id).ToList();
            companyUltimateShareholders = companyUltimateShareholders.Where(x => x.Shareholder.Id == Id).ToList();

            // Determine if ultimate shareholders are required based on solution and shareholding percentage
            bool requiresUltimateShareholders = solution == Solution.Business
                ? IsAtLeast25PercentEffectiveShareholdingAndQuaterlyOwned()
                : IsAtLeast25PercentEffectiveShareholding();

            if (!requiresUltimateShareholders)
            {
                // No ultimate shareholders required for this company shareholder
                return true;
            }

            // Ultimate shareholders are required - validate they exist and are completed
            bool hasIndividualUltimateShareholders = individualUltimateShareholders.Any();
            bool hasCompanyUltimateShareholders = companyUltimateShareholders.Any();

            // Must have at least one type of ultimate shareholder
            if (!hasIndividualUltimateShareholders && !hasCompanyUltimateShareholders)
            {
                return false;
            }

            // Validate all individual ultimate shareholders are completed
            if (hasIndividualUltimateShareholders)
            {
                bool allIndividualCompleted = individualUltimateShareholders.All(s => s.IsCompleted());
                if (!allIndividualCompleted)
                {
                    return false;
                }
            }

            // Validate all company ultimate shareholders are completed
            if (hasCompanyUltimateShareholders)
            {
                bool allCompanyCompleted = companyUltimateShareholders.All(s => s.IsCompleted());
                if (!allCompanyCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// For TB usage only because Quaterly Owned field is not applicable for TC
        /// </summary>
        /// <returns></returns>
        public bool IsAtLeast25PercentEffectiveShareholdingAndQuaterlyOwned()
        {
            return EffectiveShareholding != null && 
                Convert.ToDecimal(EffectiveShareholding) >= 25 &&
                IsQuarterlyOwned;
        }

        /// <summary>
        /// For TC usage only
        /// </summary>
        /// <returns></returns>
        public bool IsAtLeast25PercentEffectiveShareholding()
        {
            return EffectiveShareholding != null &&
                Convert.ToDecimal(EffectiveShareholding) >= 25;
        }
    }
}
