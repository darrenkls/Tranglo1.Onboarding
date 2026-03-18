using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification
{
    public class JumioVerification : Entity
    {
        public long CustomerVerificationDocumentCode { get; set; }
        public Guid? DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string CustomerInternalReference { get; set; }

        private JumioVerification() { }
    }
}