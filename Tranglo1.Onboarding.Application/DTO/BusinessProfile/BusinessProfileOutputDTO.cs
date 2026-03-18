using System;

namespace Tranglo1.Onboarding.Application.DTO.BusinessProfile
{
    public class BusinessProfileOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public long WorkFlowStatusCode { get; set; }
        public long? KYCStatusCode { get; set; }
        public DateTime RegistrationDate { get; set; }
        public long? SolutionCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistrationName { get; set; }
        public string TradeName { get; set; }
        public string CompanyRegisteredAddress { get; set; }
        public string CompanyRegisteredZipCodePostCode { get; set; }
        public string CompanyRegisteredCountryISO2 { get; set; }
        //public string CompanyRegisteredCountry { get; set; }
        public string MailingAddress { get; set; }
        public string MailingZipCodePostCode { get; set; }
        public string MailingCountryISO2 { get; set; }
        //public string MailingCountry { get; set; }
        public long? BusinessNatureCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        /*        public string CorporationType { get; set; }
        */
        public string CompanyRegistrationNo { get; set; }
        public int? NumberOfBranches { get; set; }
        public string ContactNumber { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string Website { get; set; }
        public bool? IsCompanyListed { get; set; }
        public string StockExchangeName { get; set; }
        public string StockCode { get; set; }
        public bool? IsMoneyTransferRemittance { get; set; }
        public bool? IsForeignCurrencyExchange { get; set; }
        public bool? IsRetailCommercialBankingServices { get; set; }
        public bool? IsForexTrading { get; set; }
        public bool? IsEMoneyEwallet { get; set; }
        public bool? IsIntermediataryRemittance { get; set; }
        public bool? IsCryptocurrency { get; set; }
        public bool? IsOther { get; set; }
        public string OtherReason { get; set; }
        public long? KYCSubmissionStatusCode { get; set; }


        public string FormerRegisteredCompanyName { get; set; }
        public string ForOthers { get; set; }
        public long? EntityTypeCode { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
        public string TaxIdentificationNo { get; set; }
        public string FacsimileDialCode { get; set; }
        public string FacsimileNumber { get; set; }
        public string FacsimileNumberCountryISO2 { get; set; }

        public string TelephoneDialCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string TelephoneNumberCountryISO2 { get; set; }

        public string ContactPersonName { get; set; }

        public string ContactEmailAddress { get; set; }

        //Phase 3 Changes
        public long? CustomerTypeCode { get; set; }
        public string CustomerTypeDescription { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? BusinessProfileIDTypeCode { get; set; }
        public string IDDescription { get; set; }
        public string IDNumber { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public long? ServiceTypeCode { get; set; }
        public string ServiceTypeDescription { get; set; }
        public long? CollectionTierCode { get; set; }
        public string CollectionTierDescription { get; set; }
        public string AliasName { get; set; }
        public long? NationalityCode { get; set; }
        public string NationalityISO2 { get; set; }
        public bool? IsMicroEnterprise { get; set; }
        public string FullName { get; set; }
        public Guid? BusinessProfileConcurrencyToken { get; set; }

        //Ticket 55839
        public string SSTRegistrationNumber { get; set; }
        public string SenderCity { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }
    }
}
