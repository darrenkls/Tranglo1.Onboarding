using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class WhitelistIP : Entity
    {
        public long PartnerCode { get; set; }
        public string IPAddressStart { get; set; }
        public string IPAddressEnd { get; set; }
        public bool IsRangeIP { get; set; }
        public bool IsWhitelisted { get; set; }
        public int Environment { get; set; }
        public PartnerRegistration PartnerRegistration { get; set; }
        public long? PartnerSubscriptionCode { get; set; }
    }
}
