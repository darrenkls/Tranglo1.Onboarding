using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification
{
    public class JumioGetWorkflowDetail : Entity
    {
        public long CustomerVerificationDocumentCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Workflow_Status { get; set; }
        public string AccountId { get; set; }
        public string WorkflowExecutionId { get; set; }
        public double DecisionRiskScore { get; set; }
        public string DecisionType { get; set; }
        public string DecisionDetailsLabel { get; set; }

        private JumioGetWorkflowDetail() { }
    }
}