using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class ActiveInactivePartnerAccountStatusEvent : DomainEvent
    {
		public long PartnerCode { get; private set; }
		public PartnerAccountStatusType PartnerAccountStatusType { get; private set; }
		public ChangeType ChangeType { get; private set; }
		public string Description { get; private set; }
		public long PartnerSubsciptionCode { get; set; }


		public ActiveInactivePartnerAccountStatusEvent(long partnerCode, ChangeType changeType, string description, PartnerAccountStatusType partnerAccountStatusType, long partnerSubsciptionCode)
		{
			this.PartnerCode = partnerCode;
			this.ChangeType = changeType;
			this.PartnerAccountStatusType = partnerAccountStatusType;
			this.Description = description;
			this.PartnerSubsciptionCode = partnerSubsciptionCode;
			
		}
	}
}
