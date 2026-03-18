using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement
{
    public class ShareholderCompanyLegalEntity : LegalEntity
    {
 
        public Shareholder Shareholder { get; set; }
        public CountryMeta Country { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public long? ShareholderCompanyLegalEntityCode { get; set; }


        private ShareholderCompanyLegalEntity()
        {

        }

       

        public ShareholderCompanyLegalEntity(BusinessProfile businessProfile, string companyName, string companyRegNo,
                                    string nameOfShareAboveTenPercent, string effectiveShareholding, DateTime? dateOfIncorporation, CountryMeta country,Shareholder shareholder,long? shareholderCompanyLegalEntityCode) :
            base(businessProfile, companyName, companyRegNo, nameOfShareAboveTenPercent, effectiveShareholding)
        {
            this.DateOfIncorporation = dateOfIncorporation;
            this.Country = country;
            this.Shareholder = shareholder;
            this.ShareholderCompanyLegalEntityCode = shareholderCompanyLegalEntityCode;
        }

        // TB and TC has the same mandatory fields
        public bool IsCompleted()
        {
            return !string.IsNullOrWhiteSpace(CompanyName) &&
                !string.IsNullOrWhiteSpace(CompanyRegNo) &&
                !string.IsNullOrWhiteSpace(NameOfSharesAboveTenPercent) &&
                Country != null &&
                DateOfIncorporation != null &&
                !string.IsNullOrWhiteSpace(EffectiveShareholding);

        }
    }
}
