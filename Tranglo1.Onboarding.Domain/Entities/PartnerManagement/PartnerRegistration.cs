using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Events;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class PartnerRegistration : AggregateRoot<long>
    {
        public Email Email { get; set; }
        public string IMID { get; set; }
        public string TrangloEntity { get; set; }
        public PartnerType PartnerType { get; set; }
        public string? AgentLoginId { get; set; }
        public string TimeZone { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int? AgreementStatus { get; set; }
        public PartnerAccountStatusType PartnerAccountStatusType { get; private set; }
        public string ProductLoginId { get; set; }
        public string SalesOperationLoginId { get; set; }
        public Environment Environment { get; set; }

        public OnboardWorkflowStatus AgreementOnboardWorkflowStatus { get; set; }
        public long? AgreementOnboardWorkflowStatusCode { get; set; }
        public OnboardWorkflowStatus APIIntegrationOnboardWorkflowStatus { get; set; }
        public long? APIIntegrationOnboardWorkflowStatusCode { get; set; }
        public long? PartnerTypeCode { get; set; }

        public bool IsOnboardComplete { get; set; }
        public long? RspStagingId { get; set; }
        public long? SupplierPartnerStagingId { get; set; }

        //#39856
        public bool IsPricePackageAssigned { get; set; }
        public bool IsCurrencyCodeAssigned { get; set; }
        public Guid? PartnerId { get; set; }

        //Phase 3 Changes
        public CustomerType CustomerType { get; set; }
        public long? CustomerTypeCode { get; set; }

        //#51153 Terms and conditions 
        public DateTime? TermsAcceptanceDate { get; set; }

        public PartnerRegistrationLeadsOrigin PartnerRegistrationLeadsOrigin { get; set; }
        public string OtherPartnerRegistrationLeadsOrigin { get; set; }

        #region Foreign Keys
        [ForeignKey(nameof(BusinessProfile))]
        public int BusinessProfileCode { get; set; }
        #endregion Foreign Keys

        #region Navigation Properties
        public BusinessProfile BusinessProfile { get; set; }
        public ICollection<PartnerSubscription> PartnerSubscriptions { get; set; } = new List<PartnerSubscription>();
        #endregion Navigation Properties



        private List<PartnerAccountStatus> partnerAccountStatuses = new List<PartnerAccountStatus>();
        public IReadOnlyList<PartnerAccountStatus> PartnerAccountStatuses => this.partnerAccountStatuses.AsReadOnly();


        private PartnerRegistration()
        {

        }

        public PartnerRegistration(BusinessProfile businessProfile, Email email, long? customerTypeCode, string iMID,
            string? agentLoginId, DateTime? termsAcceptanceDate = null, PartnerRegistrationLeadsOrigin? leadsOrigin = null, string? otherLeadsOrigin = null)
        {
            this.BusinessProfileCode = businessProfile.Id;
            this.Email = email;
            this.CustomerTypeCode = customerTypeCode;
            this.IMID = iMID;
            this.AgentLoginId = agentLoginId;
            this.IsOnboardComplete = false;
            this.PartnerId = Guid.NewGuid();
            this.TermsAcceptanceDate = termsAcceptanceDate; // DateTime.UtcNow.Date;
            this.PartnerRegistrationLeadsOrigin = leadsOrigin;
            this.OtherPartnerRegistrationLeadsOrigin = otherLeadsOrigin;
        }

        public void SetOverallOnboardComplete(long partnerSubsciptionCode)
        {
            if (!this.IsOnboardComplete)
            {
                this.IsOnboardComplete = true;

                this.AgreementOnboardWorkflowStatusCode = OnboardWorkflowStatus.Approve_Complete.Id;
                //this.APIIntegrationOnboardWorkflowStatusCode = OnboardWorkflowStatus.Approve_Complete.Id;

                this.AddDomainEvent(new PartnerOnboardingEmailEvent //check
              (this.Id, this.AgreementOnboardWorkflowStatusCode, partnerSubsciptionCode));

            }
        }

        public void SetOverallOnboardInComplete()
        {
            if (this.IsOnboardComplete)
            {
                this.IsOnboardComplete = false;
            }
        }

        public void ChangePartnerAccountStatus(ChangeType changeType, string description,
            PartnerAccountStatusType partnerAccountStatusType, long partnerSubscriptionCode)
        {
            //this.SetPartnerStatus(partnerAccountStatusType);

            PartnerAccountStatus partnerAccountStatus = new PartnerAccountStatus(changeType, description, partnerAccountStatusType, partnerSubscriptionCode);
            partnerAccountStatuses.Add(partnerAccountStatus);

            this.AddDomainEvent(new ActiveInactivePartnerAccountStatusEvent(this.Id, changeType, description, partnerAccountStatusType, partnerSubscriptionCode));
        }
    }
}
