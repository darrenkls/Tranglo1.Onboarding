using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class HelloSignDocument : Entity
    {
        public long PartnerCode { get; set; }
        public string DocumentName { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime? SentDate { get; set; }
        public PartnerRegistration PartnerRegistration { get; set; }
        public long? SolutionCode { get; set; }
    }
}
