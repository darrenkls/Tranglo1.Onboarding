using System;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class IndividualLegalEntity : LegalEntity
    {
        public string Name { get; set; }
        public CountryMeta Nationality { get; set; }
        public IDType IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public CountryMeta CountryOfResidence { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public Shareholder Shareholder { get; set; }
        public string PositionTitle { get; set; }
        public Title Title { get; set; }
        public string TitleOthers { get; set; }

        /*        public void SetCountry(CountryMeta nationality)
                        {
                            if (this.Nationality != nationality)
                            {
                                this.Nationality = nationality;
                            }
                        }
                        public void SetResidence(CountryMeta residence)
                        {
                            if (this.CountryOfResidence != residence)
                            {
                                this.CountryOfResidence = residence;
                            }
                        }*/
        private IndividualLegalEntity()
        {

        }

        public IndividualLegalEntity(BusinessProfile businessProfile, string companyName,
                                    string effectiveOfShareholding, string residentialAddress, string zipCodePostCode,
                                     IDType idType, string idNumber, CountryMeta nationality, Gender gender, DateTime? dateOfBirth, DateTime? idExpiryDate,
                                     CountryMeta countryOfResidence, string positionTitle, Title titleCode, string titleOthers, string nameOfShareAboveTenPercent = null, string companyRegNo = null) :
            base(businessProfile, companyName, companyRegNo = null, nameOfShareAboveTenPercent = null, effectiveOfShareholding)
        {

            this.IDType = idType;
            this.IDNumber = idNumber;
            this.Nationality = nationality;
            this.Gender = gender;
            this.DateOfBirth = dateOfBirth;
            this.IDExpiryDate = idExpiryDate;
            this.CountryOfResidence = countryOfResidence;
            this.ResidentialAddress = residentialAddress;
            this.ZipCodePostCode = zipCodePostCode;
            this.PositionTitle = positionTitle;
            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        //TC and TB have different mandatory field

        public bool IsTCCompleted(bool isTCRevampFeature)
        {
            var isValid = true;

            if (string.IsNullOrEmpty(EffectiveShareholding) ||
                         Nationality == null ||
                        Gender == null ||
                        DateOfBirth == null
                        )
            {
                isValid = false;
            }

            if (isTCRevampFeature && string.IsNullOrEmpty(PositionTitle))
            {
                isValid = false;
            }

            return isValid;
        }

        public bool IsTBCompleted()
        {
            if (string.IsNullOrEmpty(EffectiveShareholding) ||
                        string.IsNullOrEmpty(CompanyName) ||
                         Nationality == null ||
                         Gender == null ||
                         DateOfBirth == null ||
                         ResidentialAddress == null ||
                         IDNumber == null ||
                         PositionTitle == null)
            {
                return false;
            }

            return true;
        }
    }
}
