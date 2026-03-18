using System;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement
{
    public class ShareholderIndividualLegalEntity : LegalEntity
    {
        public string Name { get; set; }
        public CountryMeta Nationality { get; set; }
        public IDType IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public CountryMeta CountryOfResidence { get; set; }
        public Shareholder Shareholder { get; set; }
        public long? ShareholderIndividualLegalEntityCode { get; set; }
        public Title Title { get; set; }
        public string TitleOthers { get; set; }


        private ShareholderIndividualLegalEntity()
        {

        }

        public ShareholderIndividualLegalEntity(BusinessProfile businessProfile, string companyName,
                                    string effectiveShareholding,
                                     IDType idType, string idNumber, CountryMeta nationality, Gender gender, DateTime? dateOfBirth, DateTime? idExpiryDate,
                                     CountryMeta countryOfResidence, Shareholder shareholder, long? shareholderIndividualLegalEntityCode,
                                     Title titleCode, string titleOthers, string nameOfShareAboveTenPercent = null, string companyRegNo = null) :
            base(businessProfile, companyName, companyRegNo = null, nameOfShareAboveTenPercent = null, effectiveShareholding)
        {

            this.IDType = idType;
            this.IDNumber = idNumber;
            this.Nationality = nationality;
            this.Gender = gender;
            this.DateOfBirth = dateOfBirth;
            this.IDExpiryDate = idExpiryDate;
            this.CountryOfResidence = countryOfResidence;
            this.Shareholder = shareholder;
            this.ShareholderIndividualLegalEntityCode = shareholderIndividualLegalEntityCode;
            this.Title = titleCode;
            this.TitleOthers = titleOthers;
        }

        // TB and TC has the same mandatory fields
        public bool IsCompleted()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(EffectiveShareholding) &&
                Nationality != null &&
                DateOfBirth != null &&
                Gender != null;
        }
    }
}
