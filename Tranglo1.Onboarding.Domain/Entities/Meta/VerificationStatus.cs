using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class VerificationStatus : Enumeration
    {
        public VerificationStatus() : base() { }

        public VerificationStatus(int id, string name) : base(id, name) { }
    }
}
