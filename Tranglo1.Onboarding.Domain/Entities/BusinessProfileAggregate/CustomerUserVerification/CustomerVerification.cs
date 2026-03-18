using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.Meta;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Verification
{
    public class CustomerVerification : Entity
    {
        public BusinessProfile BusinessProfile { get; private set; }
        public VerificationStatus EKYCVerificationStatus { get; set; }
        public VerificationStatus F2FVerificationStatus { get; set; }
        public VerificationIDType VerificationIDType { get; set; }
        public long? SubmissionCount { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string JustificationRemark { get; set; }
        public RiskScore RiskScore { get; set; }
        public RiskType RiskType { get; set; }
        public Guid? TemplateID { get; set; }

        // Concurrency Token
        public Guid? CustomerVerificationConcurrencyToken { get; set; }

        private CustomerVerification()
        {

        }
        public CustomerVerification(BusinessProfile businessProfile, VerificationStatus eKYCVerificationStatus,VerificationStatus f2fVerificationStatus,
            VerificationIDType verificationIDType,long? submissionCount, DateTime? submissionDate, 
            string justificationRemark, RiskScore riskScore, RiskType riskType,Guid? templateID, Guid? customerVerificationConcurrencyToken)
        {
            BusinessProfile = businessProfile;
            EKYCVerificationStatus = eKYCVerificationStatus;
            F2FVerificationStatus = f2fVerificationStatus;
            VerificationIDType = verificationIDType;
            SubmissionCount = submissionCount;
            SubmissionDate = submissionDate;
            JustificationRemark = justificationRemark;
            RiskScore = riskScore;
            RiskType = riskType;
            TemplateID = templateID;
            CustomerVerificationConcurrencyToken = customerVerificationConcurrencyToken;
        }

    }
}
