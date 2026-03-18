using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.LegalEntitiy;

namespace Tranglo1.Onboarding.Application.DTO.Shareholder
{
    public class ShareholderOutputDTO
    {
        public long? ShareholderCode { get; set; }
        public long? ShareholderTypeCode { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string EffectiveShareholding { get; set; }
        //public string PercentageOfOwnership { get; set; }
        public string NationalityISO2 { get; set; }
        public string CountryISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public long? GenderCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsQuarterlyOwned { get; set; }

        public long? BoardOfDirectorCode { get; set; }
        public long? PrimaryOfficersCode { get; set; }
        public long? AuthorisedPersonCode { get; set; }
        public long? UltimateBeneficialOwnerCode { get; set; }

        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public string PositionTitle { get; set; }
        public Guid? ShareholderConcurrencyToken { get; set; }

        public List<ShareholderIndividualLegalEntityOutputDTO> ShareholderIndividualLegalEntityOutputDTOs { get; set; }
        public List<ShareholderCompanyLegalEntityOutputDTO> ShareholderCompanyLegalEntityOutputDTOs { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }
    }
}