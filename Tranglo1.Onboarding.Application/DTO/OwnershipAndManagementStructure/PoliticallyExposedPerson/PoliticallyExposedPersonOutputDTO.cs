using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.PoliticallyExposedPerson
{
    public class PoliticallyExposedPersonOutputDTO
    {
        public long? PoliticallyExposedPersonCode { get; set; }
        public string Name { get; set; }
        public string PositionTitle { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? GenderCode { get; set; }
        public string NationalityISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public Guid? PoliticalExposedPersonsConcurrencyToken { get; set; }


    }
}
