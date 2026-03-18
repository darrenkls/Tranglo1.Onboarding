using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ParentHoldingCompany : Entity
    {
        public string NameOfListedParentHoldingCompany { get; set; }
        public CountryMeta Country { get; set; }
        public string NameOfStockExchange { get; set; }
        public string StockCode { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public DateTime? DateOfIncorporation { get; set; }


        private ParentHoldingCompany()
        {
          

        }
/*        public void SetCountry(CountryMeta country)
        {
            if (this.Country != country)
            {
                this.Country = country;
            }
        }*/

        public ParentHoldingCompany(BusinessProfile businessProfile, string nameOfListedParentHoldingCompany, CountryMeta country,
                                     string nameOfStockExchange, string stockCode, DateTime? dateOfIncorporation)
        {
            this.BusinessProfile = businessProfile;
            this.NameOfListedParentHoldingCompany = nameOfListedParentHoldingCompany;
            this.Country = country;
            this.NameOfStockExchange = nameOfStockExchange;
            this.StockCode = stockCode;
            this.DateOfIncorporation = dateOfIncorporation;
        }
    }
}
