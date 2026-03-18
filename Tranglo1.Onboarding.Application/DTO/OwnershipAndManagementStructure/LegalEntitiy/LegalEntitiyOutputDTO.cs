using System;

namespace Tranglo1.Onboarding.Application.DTO.LegalEntitiy
{
    public class LegalEntitiyOutputDTO
    {
        public long? LegalEntityCode { get; set; }
        public long ShareholderTypeCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string NameOfSharesAboveTenPercent { get; set; }
        public string EffectiveShareholding { get; set; }
        //public string PercentageOfOwnership { get; set; }
        // public string Name { get; set; }
        public string NationalityISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IDNumber { get; set; }
        public string CountryISO2 { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public long? GenderCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public bool isCompleted { get; set; }
        public Guid? LegalEntityConcurrencyToken { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public long? ShareholderCode { get; set; }
        public string PositionTitle { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }



    }
}
