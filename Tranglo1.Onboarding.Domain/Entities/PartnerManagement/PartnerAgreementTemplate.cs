using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class PartnerAgreementTemplate : Entity
    {
        public long PartnerCode { get; set; }
        public Guid TemplateId { get; set; }
        public bool IsRemoved { get; set; }
        public PartnerRegistration PartnerRegistration { get; set; }
        public long? SolutionCode { get; set; }
    }
}