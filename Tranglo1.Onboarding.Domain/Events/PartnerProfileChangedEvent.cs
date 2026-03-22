using System;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class PartnerProfileChangedEvent
    {
        public long EventId { get; set; }
        public string TrangloEntity { get; set; }
        public long SolutionCode { get; private set; }
        public long PartnerTypeCode { get; private set; }
        public long PartnerCode { get; private set; }
        public Guid? PartnerId { get; private set; }
        public long PartnerSubscriptionCode { get; private set; }
        public string SettlementCurrencyCode { get; set; }
        public string CustomerIncorporationCountry { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistrationNo { get; set; }
        public string CompanyRegisteredAddress { get; set; }
        public DateTime? IDExpiryDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public ContactNumber TelephoneNumber { get; set; }
        public Email Email { get; set; }
        public string CompanyRegisteredZipCodePostCode { get; set; }
        public long? BusinessNatureCode { get; set; }
        public string BusinessNatureDescription { get; set; }
        public long? EnvironmentCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public long? IncorporationCompanyTypeCode { get; set; }
        public long? CustomerTypeCode { get; set; }
        public long? CollectionTierCode { get; set; }
        public int? BusinessProfileCode { get; set; }
        internal PartnerProfileChangedEvent()
        {

        }

        public PartnerProfileChangedEvent(string trangloEntity, long solutionCode, long partnerTypeCode, long partnerCode, Guid? partnerId, long partnerSubscriptionCode, string settlementCurrencyCode, string customerIncorporationCountry,
            string companyName, string companyRegistrationNo, string companyRegisteredAddress, DateTime? idExpiryDate, DateTime? dateOfBirth, ContactNumber telephoneNumber, Email email, string companyRegisteredZipCodePostCode,
            long? businessNatureCode, string businessNatureDescription, long? environmentCode, DateTime? dateOfIncorporation, long? incorporationCompanyTypeCode, long? customerTypeCode, long? collectionTierCode, int? businessProfileCode)
        {
            this.TrangloEntity = trangloEntity;
            this.SolutionCode = solutionCode;
            this.PartnerTypeCode = partnerTypeCode;
            this.PartnerId = partnerId;
            this.PartnerCode = partnerCode;
            this.PartnerSubscriptionCode = partnerSubscriptionCode;
            this.SettlementCurrencyCode = settlementCurrencyCode;
            this.CustomerIncorporationCountry = customerIncorporationCountry;
            this.CompanyName = companyName;
            this.CompanyRegistrationNo = companyRegistrationNo;
            this.CompanyRegisteredAddress = companyRegisteredAddress;
            this.IDExpiryDate = idExpiryDate;
            this.DateOfBirth = dateOfBirth;
            this.TelephoneNumber = telephoneNumber;
            this.Email = email;
            this.CompanyRegisteredZipCodePostCode = companyRegisteredZipCodePostCode;
            this.BusinessNatureCode = businessNatureCode;
            this.BusinessNatureDescription = businessNatureDescription;
            this.EnvironmentCode = environmentCode;
            this.DateOfIncorporation = dateOfIncorporation;
            this.IncorporationCompanyTypeCode = incorporationCompanyTypeCode;
            this.CustomerTypeCode = customerTypeCode;
            this.CollectionTierCode = collectionTierCode;
            this.BusinessProfileCode = businessProfileCode;
        }


        public void MaterializeEvent()
        {
            this.TrangloEntity = this.TrangloEntity;
            this.SolutionCode = this.SolutionCode;
            this.PartnerTypeCode = this.PartnerTypeCode;
        }
    }
}
