using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Specifications.CustomerUserBusinessProfiles
{
    public class CustomerUserBusinessProfileByBusinessProfile : Specification<CustomerUserBusinessProfile>
    {
        private readonly long _businessProfileCode;

        public CustomerUserBusinessProfileByBusinessProfile(long businessProfileCode)
        {
            _businessProfileCode = businessProfileCode;
        }

        public override Expression<Func<CustomerUserBusinessProfile, bool>> ToExpression()
        {
            return c => c.BusinessProfileCode == _businessProfileCode;
        }
    }
}
