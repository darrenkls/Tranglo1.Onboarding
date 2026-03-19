using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AuthorityLevel : Enumeration
    {
        public AuthorityLevel() : base() { }

        public AuthorityLevel(int id, string name) : base(id, name) { }
    }
}
