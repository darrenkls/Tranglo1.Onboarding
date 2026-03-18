using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Specifications.CustomerUserBusinessProfileRoles
{
    public sealed class RoleByCustomerUserBusinessProfileCode : Specification<CustomerUserBusinessProfileRole>
    {
        private readonly long _customerUserBusinessProfileCode;

        public RoleByCustomerUserBusinessProfileCode(long customerUserBusinessProfileCode)
        {
            _customerUserBusinessProfileCode = customerUserBusinessProfileCode;
        }

        public override Expression<Func<CustomerUserBusinessProfileRole, bool>> ToExpression()
        {
            return c => c.CustomerUserBusinessProfileCode == _customerUserBusinessProfileCode;
        }
    }
}
