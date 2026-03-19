using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class IncorporationCompanyType : Enumeration
    {
        public IncorporationCompanyType() : base() { }

        public IncorporationCompanyType(int id, string name) : base(id, name) { }
    }
}
