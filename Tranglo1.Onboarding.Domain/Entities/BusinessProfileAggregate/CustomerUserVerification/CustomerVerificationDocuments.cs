using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Verification;
using Tranglo1.Onboarding.Domain.Entities.Meta;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification
{
    public class CustomerVerificationDocuments : Entity
    {
        public CustomerVerification CustomerVerification { get; set; }
        public Guid? RawDocumentID { get; set; }
        public string RawDocumentName { get; set; }
        public Guid? WatermarkDocumentID { get; set; }
        public string WatermarkDocumentName { get; set; }
        public VerificationIDTypeSection VerificationIDTypeSection { get; set; }
        public SubmissionResult SubmissionResult { get; set; }

        private CustomerVerificationDocuments()
        {

        }

        public CustomerVerificationDocuments(CustomerVerification customerVerification, Guid? rawDocumentID,string rawDocumentName,
            Guid? watermarkDocumentID,string watermarkDocumentName,VerificationIDTypeSection verificationIDTypeSection, SubmissionResult submissionResult)
        {
            CustomerVerification = customerVerification;
            RawDocumentID = rawDocumentID;
            RawDocumentName = rawDocumentName;
            WatermarkDocumentID = watermarkDocumentID;
            WatermarkDocumentName = watermarkDocumentName;
            VerificationIDTypeSection = verificationIDTypeSection;
            SubmissionResult = submissionResult;

        }

    }
}
