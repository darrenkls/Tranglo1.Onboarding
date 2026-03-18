using System;
using System.Linq.Expressions;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;

namespace Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles
{
    public sealed class UnreadKYCCustomerSummaryFeedbackNotificationByBusinessProfile : Specification<KYCCustomerSummaryFeedbackNotification>
    {
        private readonly int _businessProfile;
        private readonly int _solutionCode;

        public UnreadKYCCustomerSummaryFeedbackNotificationByBusinessProfile(int businessProfile, int solutionCode)
        {
            _businessProfile = businessProfile;
            _solutionCode = solutionCode;
        }

        public override Expression<Func<KYCCustomerSummaryFeedbackNotification, bool>> ToExpression()
        {
            return x => x.BusinessProfile.Id == _businessProfile
                && x.KYCCustomerSummaryFeedback.SolutionCode == _solutionCode
                && !x.IsRead;
        }
    }
}
