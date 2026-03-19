using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Title : Enumeration
    {
        public Title() : base() { }

        public Title(int id, string name) : base(id, name) { }
    }
}
