using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class BusinessProfileContactPersonNameUpdatedEvent : DomainEvent
    {
        public BusinessProfile BusinessProfile { get; set; }
        public string ContactPersonName { get; set; }

        public BusinessProfileContactPersonNameUpdatedEvent(BusinessProfile businessProfile, string contactPersonName )
        {
            this.BusinessProfile = businessProfile;
            this.ContactPersonName = contactPersonName;
        }
    }
}
