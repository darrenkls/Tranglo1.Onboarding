using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class SignedPartnerAgreement : Entity
    {
        public long PartnerCode { get; set; }
        public Guid SignedDocumentId { get; set; }
        public bool IsRemoved { get; set; } = false;
        public PartnerRegistration PartnerRegistration { get; set; }
        public bool IsDisplay { get; set; } = true;
        public int? RemovedBy { get; set; }
        public DateTime? RemovedDate { get; set; }
        public long? SolutionCode { get; set; }
    }
}
