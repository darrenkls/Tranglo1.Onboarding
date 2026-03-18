using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate
{
    public class EnforcementActions : Enumeration
    {
        public EnforcementActions()
        {
        }

        public EnforcementActions(int id, string name) : base(id, name)
        {
        }
        public static readonly EnforcementActions Yes = new EnforcementActions(1, "Yes");
        public static readonly EnforcementActions No = new EnforcementActions(2, "No");
        public static readonly EnforcementActions NoBySystem = new EnforcementActions(3, "No (by System)");
        public static readonly EnforcementActions NA = new EnforcementActions(4, "N/A");
    }
}
