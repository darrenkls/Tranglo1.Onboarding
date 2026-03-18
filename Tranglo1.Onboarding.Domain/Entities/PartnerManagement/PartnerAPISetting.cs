using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class PartnerAPISetting : Entity
    {
        public long PartnerCode { get; set; }
        public string APIUserId { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; }
        public string APIStatusCallbackURL { get; set; }
        public int EnvironmentCode { get; set; }
        public bool IsConfigured { get; set; }
        public bool IsPartnerConfirmWhitelisted { get; set; }
        public bool IsSOAP { get; set; }
        public bool IsREST { get; set; }
        public PartnerRegistration PartnerRegistration { get; set; }
        public long? PartnerSubscriptionCode { get; set; }
    }
}
