using System;
using System.Linq.Expressions;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;

namespace Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles
{
    public sealed class UnreadKYCSummaryFeedbackNotificationByBusinessProfileAndKYCCategory : Specification<KYCSummaryFeedbackNotification>
    {
        private readonly int _businessProfile;
        private readonly long _kycCategoryCode;

        public UnreadKYCSummaryFeedbackNotificationByBusinessProfileAndKYCCategory(int businessProfile, long kycCategoryCode)
        {
            _businessProfile = businessProfile;
            _kycCategoryCode = kycCategoryCode;
        }

        public override Expression<Func<KYCSummaryFeedbackNotification, bool>> ToExpression()
        {
            return x => x.BusinessProfile.Id == _businessProfile
                && x.KYCSummaryFeedback.KYCCategory.Id == _kycCategoryCode
                && !x.IsRead;
        }
    }
}
