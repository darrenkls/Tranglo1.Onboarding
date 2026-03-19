using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class RecipientType : Enumeration
    {
        public RecipientType() : base() { }

        public RecipientType(int id, string name) : base(id, name) { }
    }
}
