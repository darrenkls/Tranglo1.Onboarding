using CSharpFunctionalExtensions;
using System;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class PrimaryOfficer : Entity
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
        public Shareholder Shareholder { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public Title Title { get; set; }
        public string TitleOthers { get; set; }


        private PrimaryOfficer()
        {

        }

        public PrimaryOfficer(BusinessProfile businessProfile, string name, string positionTitle, DateTime? dateOfBirth, Gender gender,
                                 IDType idType, string idNumber, DateTime? idExpiryDate, CountryMeta countryMeta, CountryMeta countryOfResidence, string residentialAddress,
                                 string zipCodePostCode, Title titleCode, string titleOthers)
        {
            this.BusinessProfile = businessProfile;
            this.Name = name;
            this.PositionTitle = positionTitle;
            this.DateOfBirth = dateOfBirth;
            this.IDNumber = idNumber;
            this.IDExpiryDate = idExpiryDate;
            this.Gender = gender;
            this.IDType = idType;
            this.CountryOfResidence = countryOfResidence;
            this.Nationality = countryMeta;
            this.ResidentialAddress = residentialAddress;
            this.ZipCodePostCode = zipCodePostCode;
            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        //currently only TC got principal office and TB if other than normal corporate and individual will have primary officer.
        //TB and TC will have same validation
        public bool IsCompleted()
        {
            if (!DateOfBirth.HasValue ||
                       string.IsNullOrEmpty(Name) ||
                       string.IsNullOrEmpty(PositionTitle) ||
                       Nationality == null ||
                       Gender == null)
            {
                return false;
            }
            return true;
        }
    }
}
