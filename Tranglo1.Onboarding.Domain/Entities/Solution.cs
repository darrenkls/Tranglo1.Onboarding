using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Solution : Enumeration
    {
        public Solution() : base() { }

        public Solution(int id, string name) : base(id, name) { }

        public static readonly Solution Undefined = new Solution(-1, "Undefined");
        public static readonly Solution Connect = new Solution(1, "Tranglo Connect");
        public static readonly Solution Business = new Solution(2, "Tranglo Business");
        public static readonly Solution Recharge = new Solution(3, "Tranglo Recharge");
        public static readonly Solution Personal = new Solution(4, "Tranglo Personal");

        public string GetShortName()
        {
            var solutionShortNames = new Dictionary<Solution, string>
            {
                { Connect, "TC" },
                { Business, "TB" }
            };

            return solutionShortNames.TryGetValue(this, out var shortName) ? shortName : Name;
        }
    }
}
