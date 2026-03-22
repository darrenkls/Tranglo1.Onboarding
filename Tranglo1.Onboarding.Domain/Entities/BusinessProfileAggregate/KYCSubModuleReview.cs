using System;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class KYCSubModuleReview
    {
        public KYCCategory KYCCategory { get; set; }
        public ReviewResult ReviewResult { get; set; }
        public DateTime? UserUpdateDate { get; private set; }
        public DateTime? LastReviewedDate { get; set; }
        public BusinessProfile BusinessProfile { get; set; }

        public int BusinessProfileCode { get; private set; }
        public long KYCCategoryCode { get; set; }

        private KYCSubModuleReview() { }

        public KYCSubModuleReview(BusinessProfile businessProfile, KYCCategory kYCCategory, ReviewResult reviewResult)
        {
            BusinessProfile = businessProfile;
            KYCCategory = kYCCategory;
            ReviewResult = reviewResult;
        }

        public void UpdateUserUpdatedDate()
        {
            UserUpdateDate = DateTime.UtcNow;
        }

        public void UpdateLastReviewDate()
        {
            LastReviewedDate = DateTime.UtcNow;
        }

        public void AssignReviewResult(ReviewResult reviewResult)
        {
            if (ReviewResult != reviewResult)
            {
                ReviewResult = reviewResult;
            }
        }
    }
}
