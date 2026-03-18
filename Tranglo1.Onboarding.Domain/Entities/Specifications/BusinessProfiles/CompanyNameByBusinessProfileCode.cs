using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles
{
    public sealed class CompanyNameByBusinessProfileCode : Specification<BusinessProfile>
    {
        private readonly int _businessProfileCode;

        public CompanyNameByBusinessProfileCode(int businessProfileCode)
        {
            _businessProfileCode = businessProfileCode;
        }

        public override Expression<Func<BusinessProfile, bool>> ToExpression()
        {
            return c => c.Id == _businessProfileCode;
        }
    }
}
