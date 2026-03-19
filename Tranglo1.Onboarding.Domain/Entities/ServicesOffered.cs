using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ServicesOffered : Enumeration
    {
        public ServicesOffered() : base() { }

        public ServicesOffered(int id, string name) : base(id, name) { }
    }
}
