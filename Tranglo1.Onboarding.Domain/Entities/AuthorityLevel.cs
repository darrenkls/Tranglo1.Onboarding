using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AuthorityLevel : Enumeration
    {
        public AuthorityLevel() : base() { }

        public AuthorityLevel(int id, string name) : base(id, name) { }

        public static readonly AuthorityLevel Level1 = new AuthorityLevel(1, "Level 1");
        public static readonly AuthorityLevel Level2 = new AuthorityLevel(2, "Level 2");
    }
}
