using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Events
{
   public class PartnerOnboardingEmailLiveEmailEvent : DomainEvent
    {
        public long PartnerCode { get; private set; }
        public long? AgreementOnboardWorkflowStatusCode { get; private set; }
        public long? APIIntegrationOnboardWorkflowStatusCode { get; private set; }
        public long PartnerSubscriptionCode { get; set; }

        public PartnerOnboardingEmailLiveEmailEvent(long partnerCode, long? agreementOnboardWorkflowStatus,
            long? apiIntegrationOnboardWorkFlowStatus, long partnerSubscriptionCode)
        {
            this.PartnerCode = partnerCode;
            this.AgreementOnboardWorkflowStatusCode = agreementOnboardWorkflowStatus;
            this.APIIntegrationOnboardWorkflowStatusCode = apiIntegrationOnboardWorkFlowStatus;
            this.PartnerSubscriptionCode = partnerSubscriptionCode;
        }
    }
}
