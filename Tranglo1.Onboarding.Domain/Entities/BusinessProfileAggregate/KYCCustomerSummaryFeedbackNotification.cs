using CSharpFunctionalExtensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCCustomerSummaryFeedbackNotification : Entity
    {
        public KYCCustomerSummaryFeedback KYCCustomerSummaryFeedback { get; set; }

        public BusinessProfile BusinessProfile { get; set; }

        [Required]
        public string Event { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public KYCCustomerSummaryFeedbackNotification()
        {

        }

        public KYCCustomerSummaryFeedbackNotification(long id, BusinessProfile businessProfile, KYCCustomerSummaryFeedback feedback)
        {
            if (id != 0)
                base.Id = id;

            BusinessProfile = businessProfile;
            KYCCustomerSummaryFeedback = feedback;
        }
    }
}
