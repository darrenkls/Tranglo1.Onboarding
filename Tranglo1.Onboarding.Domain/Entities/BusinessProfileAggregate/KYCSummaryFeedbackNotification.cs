using CSharpFunctionalExtensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCSummaryFeedbackNotification : Entity
    {
        public KYCSummaryFeedback KYCSummaryFeedback { get; set; }
        public BusinessProfile BusinessProfile { get; set; }
        [Required]
        public string Event { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public KYCSummaryFeedbackNotification()
        {

        }

        public KYCSummaryFeedbackNotification(long id, BusinessProfile businessProfile, KYCSummaryFeedback feedback)
        {
            if (id != 0)
                base.Id = id;

            BusinessProfile = businessProfile;
            KYCSummaryFeedback = feedback;
        }

    }
}
