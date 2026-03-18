using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class PartnerOnboardingEmailEvent : DomainEvent
    {
        public long PartnerCode { get; private set; }
        public long? AgreementOnboardWorkflowStatusCode { get; private set; }
        public long PartnerSubsciptionCode { get; set; }

        public PartnerOnboardingEmailEvent(long partnerCode, long? agreementOnboardWorkflowStatus, long partnerSubscriptionCode)
        {
            this.PartnerCode = partnerCode;
            this.AgreementOnboardWorkflowStatusCode = agreementOnboardWorkflowStatus;
            this.PartnerSubsciptionCode = partnerSubscriptionCode;
        }

    }
}
