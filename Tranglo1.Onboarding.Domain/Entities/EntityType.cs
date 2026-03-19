using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class EntityType : Enumeration
    {
        public EntityType() : base() { }

        public EntityType(int id, string name) : base(id, name) { }
    }
}
