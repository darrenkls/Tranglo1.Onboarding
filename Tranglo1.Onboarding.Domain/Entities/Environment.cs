using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Environment : Enumeration
    {
        public Environment() : base() { }

        public Environment(int id, string name) : base(id, name) { }
    }
}
