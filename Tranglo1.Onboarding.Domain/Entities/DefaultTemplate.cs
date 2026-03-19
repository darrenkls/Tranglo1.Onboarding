using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DefaultTemplate : Enumeration
    {
        public DefaultTemplate() : base() { }

        public DefaultTemplate(int id, string name) : base(id, name) { }
    }
}
