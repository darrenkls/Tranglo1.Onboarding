using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class EntityType : Enumeration
    {
        public EntityType() : base() { }

        public EntityType(int id, string name) : base(id, name) { }

        public static readonly EntityType Licensed_Financial_Institution_Bank = new EntityType(1, "Licensed Financial Institution (Bank)");
        public static readonly EntityType Licensed_Money_Services = new EntityType(2, "Licensed Money Services");
    }
}
