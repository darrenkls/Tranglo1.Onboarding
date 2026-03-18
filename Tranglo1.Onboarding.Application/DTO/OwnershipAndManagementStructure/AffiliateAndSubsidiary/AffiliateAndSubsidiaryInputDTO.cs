using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary
{
    public class AffiliateAndSubsidiaryInputDTO
    {
        public long? AffliateSubsidiaryCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string CountryISO2 { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string Relationship { get; set; }

    }
}
