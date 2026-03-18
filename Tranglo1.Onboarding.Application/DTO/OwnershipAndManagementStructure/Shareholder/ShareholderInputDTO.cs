using System;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.Shareholder
{
    public class ShareholderInputDTO
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
        public bool IsDeleted { get; set; } = false;
        public bool isBoardOfDirector { get; set; } = false;
        public bool isPrimaryOfficers { get; set; } = false;
        public bool isAuthorisedPerson { get; set; } = false;
        public bool isUltimateBeneficialOwners { get; set; } = false;
        public bool IsQuarterlyOwned { get; set; }
        public string ResidentialAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public string PositionTitle { get; set; }

        public List<ShareholderLegalEntity>? ShareholderLegalEntities { get; set; }
        public string EmailAddress { get; set; }
        public ShareholderInputDTO()
        {
            ShareholderLegalEntities = new List<ShareholderLegalEntity>();
        }

        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }
    }

    public class ShareholderLegalEntity
    {
        public long? ShareholderTypeCode { get; set; }
        public long LegalEntityCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string CountryISO2 { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string NameOfSharesAboveTenPercent { get; set; }
        public string EffectiveShareholding { get; set; }

        public string NationalityISO2 { get; set; }
        public long? IdTypeCode { get; set; }
        public string IDNumber { get; set; }
        public long? GenderCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public string CountryOfResidenceISO2 { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }
    }

}

