using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class UserType : Enumeration
    {
        public UserType() : base() { }

        public UserType(int id, string name) : base(id, name) { }
    }
}
