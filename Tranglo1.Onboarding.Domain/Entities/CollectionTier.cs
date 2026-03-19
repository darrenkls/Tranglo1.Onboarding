using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CollectionTier : Enumeration
    {
        public CollectionTier() : base() { }

        public CollectionTier(int id, string name) : base(id, name) { }
    }
}
