using System;

namespace Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.LegalEntitiy
{
    public class ShareholderIndividualLegalEntityOutputDTO
    {
        public long? LegalEntityCode { get; set; }
        public long? ShareholderCode { get; set; }
        public long ShareholderTypeCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string NameOfSharesAboveTenPercent { get; set; }
        public string EffectiveShareholding { get; set; }
        public string NationalityISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IDNumber { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public long? GenderCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public long? ShareholderIndividualLegalEntityCode { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }
    }
}
