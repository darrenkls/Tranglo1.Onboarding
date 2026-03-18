using System;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class IndividualShareholder : Shareholder
    {
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public CountryMeta Nationality { get; set; }

        public IDType IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public Gender Gender { get; set; }
        public CountryMeta CountryOfResidence { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public string PositionTitle { get; set; }
        public Title Title { get; set; }
        public string TitleOthers { get; set; }
        private IndividualShareholder()
        {

        }

        public IndividualShareholder(BusinessProfile businessProfile, string name, DateTime? dateOfBirth,
                                         IDType idType, string idNumber, DateTime? idExpiryDate,
                                          Gender gender, string percentageOfOwnership,
                                          CountryMeta nationality, CountryMeta countryOfResidence, BoardOfDirector boardOfDirector,
                                          PrimaryOfficer primaryOfficer, AuthorisedPerson authorisedPerson, string residentialAddress, string zipcodePostCode, string positionTitle,
                                          IndividualLegalEntity ultimateBeneficialOwner, Title titleCode, string titleOthers) :
            base(businessProfile, null, percentageOfOwnership, boardOfDirector, primaryOfficer, authorisedPerson, ultimateBeneficialOwner)
        {
            this.Name = name;
            this.DateOfBirth = dateOfBirth;
            this.IDType = idType;
            this.IDNumber = idNumber;
            this.IDExpiryDate = idExpiryDate;
            this.Gender = gender;
            this.Nationality = nationality;
            this.CountryOfResidence = countryOfResidence;
            this.ResidentialAddress = residentialAddress;
            this.ZipCodePostCode = zipcodePostCode;
            this.PositionTitle = positionTitle;
            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        //Currently for TB and TC Validation is same
        public bool IsTBCompleted()
        {
            if (
                            !DateOfBirth.HasValue ||
                            string.IsNullOrEmpty(Name) ||
                            Nationality == null ||
                            Gender == null ||
                            string.IsNullOrEmpty(EffectiveShareholding) ||
                            string.IsNullOrWhiteSpace(PositionTitle))
            {
                return false;
            }

            return true;
        }

        public bool IsTCCompleted(bool isTCRevampFeature)
        {
            var isValid = true;

            if (
                            !DateOfBirth.HasValue ||
                            string.IsNullOrEmpty(Name) ||
                            Nationality == null ||
                            Gender == null ||
                            string.IsNullOrEmpty(EffectiveShareholding)
                            )
            {
                isValid = false;
            }

            if (isTCRevampFeature && string.IsNullOrWhiteSpace(PositionTitle))
            {
                isValid = false;
            }

            return isValid;
        }
    }
}

