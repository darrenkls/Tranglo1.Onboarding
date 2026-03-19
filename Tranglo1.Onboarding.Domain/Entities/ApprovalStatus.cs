using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ApprovalStatus : Enumeration
    {
        public ApprovalStatus() : base() { }

        public ApprovalStatus(int id, string name) : base(id, name) { }
    }
}
