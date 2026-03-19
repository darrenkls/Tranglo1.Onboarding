using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class RiskType : Enumeration
    {
        public RiskType() : base() { }

        public RiskType(int id, string name) : base(id, name) { }
    }
}
