using CSharpFunctionalExtensions;
using System;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement
{
    public class AuthorisedPerson : Entity
    {
        public string FullName { get; set; }
        public AuthorisationLevel AuthorisationLevel { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public CountryMeta Nationality { get; set; }
        public IDType IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public CountryMeta CountryOfResidence { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public Shareholder Shareholder { get; set; }
        public bool IsDefault { get; set; }
        public string PositionTitle { get; set; }
        public ContactNumber TelephoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public Title Title { get; set; }
        public string TitleOthers { get; set; }

        private AuthorisedPerson()
        {

        }

        public AuthorisedPerson(BusinessProfile businessProfile, string fullName, AuthorisationLevel authorisationLevel, DateTime? dateOfBirth,
                            CountryMeta nationality, IDType idType, string idNumber, DateTime? idExpiryDate, string residentialAddress,
                            string zipCodePostCode, CountryMeta countryOfResidence, string positionTitle, string emailAddress, Title titleCode, string titleOthers, bool isDefault = false)
        {
            this.BusinessProfile = businessProfile;
            this.FullName = fullName;
            this.AuthorisationLevel = authorisationLevel;
            this.DateOfBirth = dateOfBirth;
            this.Nationality = nationality;
            this.IDType = idType;
            this.IDNumber = idNumber;
            this.IDExpiryDate = idExpiryDate;
            this.ResidentialAddress = residentialAddress;
            this.ZipCodePostCode = zipCodePostCode;
            this.CountryOfResidence = countryOfResidence;
            this.IsDefault = isDefault;
            this.PositionTitle = positionTitle;
            this.EmailAddress = emailAddress;
            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        public bool IsTBCompleted()
        {
            if (
                       !DateOfBirth.HasValue ||
                       string.IsNullOrEmpty(FullName) ||
                       Nationality == null ||
                       PositionTitle == null ||
                       TelephoneNumber == null ||
                       ResidentialAddress == null ||
                       IDNumber == null)
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
                      string.IsNullOrEmpty(FullName) ||
                      Nationality == null)
            {
                isValid = false;
            }

            return isValid;
        }
    }


}
