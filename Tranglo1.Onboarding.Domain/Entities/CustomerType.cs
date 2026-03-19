using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerType : Enumeration
    {
        public CustomerType() : base() { }

        public CustomerType(int id, string name) : base(id, name) { }
    }
}
