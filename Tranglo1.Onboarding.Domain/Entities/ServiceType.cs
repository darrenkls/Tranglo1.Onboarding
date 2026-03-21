using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ServiceType : Enumeration
    {
        public ServiceType() : base() { }

        public ServiceType(int id, string name) : base(id, name) { }

        public static readonly ServiceType Collection_Anyone = new ServiceType(1, "Collection(Anyone) and Payout");
        public static readonly ServiceType Collection_Ownself = new ServiceType(2, "Collection(Ownself) and Payout");
        public static readonly ServiceType Collection_Payout = new ServiceType(3, "Payout Only");
    }
}
