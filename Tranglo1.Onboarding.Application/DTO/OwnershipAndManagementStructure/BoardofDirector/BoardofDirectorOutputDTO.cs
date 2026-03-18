using System;

namespace Tranglo1.Onboarding.Application.DTO.BoardofDirector
{
    public class BoardofDirectorOutputDTO
    {
        public long? BoardOfDirectorCode { get; set; }
        public string Name { get; set; }
        public string PositionTitle { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? GenderCode { get; set; }
        public string NationalityISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public bool isCompleted { get; set; }
        public long? ShareholderCode { get; set; }
        public Guid? BoardOfDirectorConcurrencyToken { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }
    }
}
