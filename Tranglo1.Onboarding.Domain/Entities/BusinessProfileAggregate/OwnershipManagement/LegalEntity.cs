using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class LegalEntity : Entity
    {
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string NameOfSharesAboveTenPercent { get; set; }
        public string EffectiveShareholding { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
      

        private protected LegalEntity()
        {

        }

        public LegalEntity(BusinessProfile businessProfile, string companyName, string companyRegNo,
                            string nameOfSharesAboveTenPercent, string effectiveShareholding)
        {
            this.BusinessProfile = businessProfile;
            this.CompanyName = companyName;
            this.CompanyRegNo = companyRegNo;
            this.NameOfSharesAboveTenPercent = nameOfSharesAboveTenPercent;
            this.EffectiveShareholding = effectiveShareholding;
          
        }

    }
}
