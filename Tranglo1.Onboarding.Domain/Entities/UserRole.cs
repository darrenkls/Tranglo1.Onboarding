using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class UserRole : Enumeration
    {
        public UserRole() : base() { }

        public UserRole(int id, string name) : base(id, name) { }
    }
}
