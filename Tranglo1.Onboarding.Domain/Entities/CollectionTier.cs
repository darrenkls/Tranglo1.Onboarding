using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CollectionTier : Enumeration
    {
        public CollectionTier() : base() { }

        public CollectionTier(int id, string name) : base(id, name) { }

        public static readonly CollectionTier Tier_1 = new CollectionTier(1, "Tier 1");
        public static readonly CollectionTier Tier_2 = new CollectionTier(2, "Tier 2");
        public static readonly CollectionTier Tier_3 = new CollectionTier(3, "Tier 3");
    }
}
