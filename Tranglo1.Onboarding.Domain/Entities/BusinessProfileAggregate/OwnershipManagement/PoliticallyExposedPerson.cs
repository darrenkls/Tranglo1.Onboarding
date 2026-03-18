using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class PoliticallyExposedPerson : Entity
    {
        public string Name { get; set; }
        public string PositionTitle { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public CountryMeta Nationality { get; set; }
        public IDType IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public CountryMeta CountryOfResidence { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }


        private PoliticallyExposedPerson()
        {


        }
   

        public PoliticallyExposedPerson(BusinessProfile businessProfile, string name, string positionTitle, DateTime? dateOfBirth,
                                         Gender gender,  IDType idType, string idNumber, DateTime? idExpiryDate, CountryMeta nationality,
                                            CountryMeta countryOfResidence)
        {
            this.BusinessProfile = businessProfile;
            this.Name = name;
            this.PositionTitle = positionTitle;
            this.DateOfBirth = dateOfBirth;
            this.Gender = gender;
            this.IDType = idType;
            this.IDNumber = idNumber;
            this.IDExpiryDate = idExpiryDate;
            this.CountryOfResidence = countryOfResidence;
            this.Nationality = nationality;

        }

    }
}
