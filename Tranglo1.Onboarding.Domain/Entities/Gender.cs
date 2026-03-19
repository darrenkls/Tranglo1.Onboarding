using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Gender : Enumeration
    {
        public Gender() : base() { }

        public Gender(int id, string name) : base(id, name) { }
    }
}
