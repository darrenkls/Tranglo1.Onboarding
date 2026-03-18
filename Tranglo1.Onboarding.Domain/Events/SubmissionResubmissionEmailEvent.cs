using System;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class SubmissionResubmissionEmailEvent : DomainEvent
    {
		public string CompanyName { get; private set; }
		public KYCSubmissionStatus KYCSubmissionStatus { get; private set; }
		public DateTime? KYCSubmissionDate { get; private set; }
		public long? solutionCode { get; set; }
		public CollectionTier CollectionTier { get; set; }
		public long KYCSubmissionStatusCode { get; set; }
        public string PICName { get; set; }
        public string CustomerSolution { get; set; }
        public int BusinessProfileCode { get; set; }
        public int UserId { get; set; }

        public SubmissionResubmissionEmailEvent(string companyName, KYCSubmissionStatus kycSubmissionStatus, DateTime? kycSubmissionDate, long? solution, CollectionTier collectionTier, long kycSubmissionStatusCode,
                                                string PICName, string CustomerSolution, int businessProfileCode, int userId)
		{
			this.CompanyName = companyName;
			this.KYCSubmissionStatus = kycSubmissionStatus;
			this.KYCSubmissionDate = kycSubmissionDate;
			this.solutionCode = solution;
			this.CollectionTier = collectionTier;
			this.KYCSubmissionStatusCode = kycSubmissionStatusCode;
			this.PICName = PICName;
			this.CustomerSolution = CustomerSolution;
			this.BusinessProfileCode = businessProfileCode;
			this.UserId = userId;
		}
    }
}
