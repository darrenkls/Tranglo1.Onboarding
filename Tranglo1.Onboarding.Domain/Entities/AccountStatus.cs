using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AccountStatus : Enumeration
    {
        public AccountStatus() : base() { }

        public AccountStatus(int id, string name) : base(id, name) { }
    }
}
