using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AuthorisationLevel : Enumeration
    {
        public AuthorisationLevel() : base() { }

        public AuthorisationLevel(int id, string name) : base(id, name) { }
    }
}
