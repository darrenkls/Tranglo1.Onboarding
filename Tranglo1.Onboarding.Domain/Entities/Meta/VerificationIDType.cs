using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class VerificationIDType : Enumeration
    {
        public VerificationIDType() : base() { }

        public VerificationIDType(int id, string name) : base(id, name) { }
    }
}
