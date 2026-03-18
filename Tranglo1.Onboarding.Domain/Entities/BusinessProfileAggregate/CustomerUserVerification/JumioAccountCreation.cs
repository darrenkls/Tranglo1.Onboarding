using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification
{
    public class JumioAccountCreation : Entity
    {
        public long CustomerVerificationDocumentCode { get; set; }
        public string CustomerInternalReference { get; set; }
        public string Timestamp { get; set; }
        public string AccountId { get; set; }
        public string WorkflowExecutionId { get; set; }
        public string WorkflowExecutionCredentialsId { get; set; }

        private JumioAccountCreation() { }
    }    
}