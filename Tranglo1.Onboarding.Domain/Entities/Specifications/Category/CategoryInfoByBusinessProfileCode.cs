using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Specifications.Category
{
    public sealed class CategoryInfoByBusinessProfileCode : Specification<DocumentCategoryBP>
    {
        private readonly int _businessProfileCode;

        public CategoryInfoByBusinessProfileCode(int businessProfileCode)
        {
            _businessProfileCode = businessProfileCode;
        }

        public override Expression<Func<DocumentCategoryBP, bool>> ToExpression()
        {
            return c => c.BusinessProfileCode == _businessProfileCode;
        }
    }
}
