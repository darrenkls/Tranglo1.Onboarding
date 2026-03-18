using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary
{
    public class AffiliateAndSubsidiaryOutputDTO
    {
        public long? AffliateSubsidiaryCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string CountryISO2 { get; set; }
        public bool isCompleted { get; set; }
        public Guid? AffiliatesAndSubsidiariesConcurrencyToken { get; set; }
        public string Relationship {  get; set; }

    }
}
