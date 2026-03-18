using System;

namespace Tranglo1.RBADailyScoring.DTOs
{
    public class BusinessProfileDTO
    {
        public int BusinessProfileCode { get; set; }
        public long? ServiceTypeCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public long? CompanyRegisteredCountryCode { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
        public long? BusinessNatureCode { get; set; }
        public long? CollectionTierCode { get; set; }
        public long? NationalityMetaCode { get; set; }
    }
}