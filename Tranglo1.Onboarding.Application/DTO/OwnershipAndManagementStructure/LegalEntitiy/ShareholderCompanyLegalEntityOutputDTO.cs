using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.LegalEntitiy
{
    public class ShareholderCompanyLegalEntityOutputDTO
    {
        public long? LegalEntityCode { get; set; }
        public long? ShareholderCode { get; set; }
        public long ShareholderTypeCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string NameOfSharesAboveTenPercent { get; set; }
        public string EffectiveShareholding { get; set; }
        public string IDNumber { get; set; }
        public string CountryISO2 { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public long? ShareholderCompanyLegalEntityCode { get; set; }

    }
}

