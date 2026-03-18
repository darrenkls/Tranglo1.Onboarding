using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class PartnerSubscription : Entity
    {
        public string TrangloEntity { get; set; }
        public bool? IsOnboardComplete { get; set; }
        public long? APIIntegrationOnboardWorkflowStatusCode { get; set; }
        public long? RspStagingId { get; set; }
        public long? SupplierPartnerStagingId { get; set; }
        public string SettlementCurrencyCode { get; set; }
        public bool? IsCurrencyCodeAssigned { get; set; }
        public bool? IsPricePackageAssigned { get; set; }

        #region Foreign Keys
        [ForeignKey(nameof(PartnerRegistration))]
        public long PartnerCode { get; set; }

        [ForeignKey(nameof(Solution))]
        public long? SolutionCode { get; set; }

        [ForeignKey(nameof(PartnerType))]
        public long? PartnerTypeCode { get; set; }

        [ForeignKey(nameof(Environment))]
        public long? EnvironmentCode { get; set; }

        [ForeignKey(nameof(PartnerAccountStatusType))]
        public long? PartnerAccountStatusTypeCode { get; set; }

        [ForeignKey(nameof(KYCReminderSubscription))]
        public long? KYCReminderSubscriptionCode { get; set; }
        #endregion Foreign Keys

        #region Navigation Properties
        public PartnerRegistration PartnerRegistration { get; set; }
        public Solution Solution { get; set; }
        public PartnerType PartnerType { get; set; }
        public Environment Environment { get; set; }
        public PartnerAccountStatusType PartnerAccountStatusType { get; set; }
        public KYCReminderSubscription KYCReminderSubscription { get; set; }
        #endregion Navigation Properties

        private PartnerSubscription()
        {

        }

        public PartnerSubscription(long partnerCode, PartnerType partnerType, Solution solution, 
            string trangloEntity, string settlementCurrencyCode, long? rspStagingId, long? supplierPartnerStagingId, bool? isOnboardComplete, bool? productionPartnerSub)
        {
            this.PartnerCode = partnerCode;
            this.PartnerType = partnerType;
            this.Solution = solution;
            this.TrangloEntity = trangloEntity;
            this.Environment = Environment.Staging; //set Environment = Staging for new subcriptions
            this.SettlementCurrencyCode = settlementCurrencyCode;
            this.RspStagingId = rspStagingId;
            this.SupplierPartnerStagingId = supplierPartnerStagingId;
            this.SettlementCurrencyCode = settlementCurrencyCode;
            this.PartnerAccountStatusType = PartnerAccountStatusType.Active;
            this.IsOnboardComplete = isOnboardComplete;

            if (this.SettlementCurrencyCode != null)
            {
                this.IsCurrencyCodeAssigned = true;
            }

            if(this.APIIntegrationOnboardWorkflowStatusCode == 3)
            {
                SetAPIIntegrationOnboardWorkflowStatus();
            }

            if (productionPartnerSub == true)
            {
                this.Environment = Environment.Production;
            }
        }

        public void SetAPIIntegrationOnboardWorkflowStatus()
        {
            this.APIIntegrationOnboardWorkflowStatusCode = OnboardWorkflowStatus.Approve_Complete.Id;
            this.SetOverallOnboardComplete(Id);
        }

        public void SetOverallOnboardComplete(long? partnerSubscriptionCode)
        {
            if (this.IsOnboardComplete is false || this.IsOnboardComplete is null)
            {
                this.IsOnboardComplete = true;
            }
        }

        public void SetOverallOnboardInComplete()
        {
            if (this.IsOnboardComplete is true || this.IsOnboardComplete is null)
            {
                this.IsOnboardComplete = false;
            }
        }

        public void SetBusinessEnvironment()
        {
            this.Environment = Environment.Production;
        }

        public void SetConnectEnvironment()
        {
            this.Environment = Environment.Production;
        }
    }
}
