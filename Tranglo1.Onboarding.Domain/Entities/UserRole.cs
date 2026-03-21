using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class UserRole : Enumeration
    {
        public UserRole() : base() { }

        public UserRole(int id, string name) : base(id, name) { }

        public static readonly UserRole MasterTeller = new UserRole(1, "Master Teller");
        public static readonly UserRole SystemAdmin = new UserRole(2, "System Admin");
        public static readonly UserRole Compliance = new UserRole(3, "Compliance");
        public static readonly UserRole MasterCompliance = new UserRole(4, "Master Compliance");
        public static readonly UserRole Teller = new UserRole(5, "Teller");
        public static readonly UserRole Finance = new UserRole(6, "Finance");
        public static readonly UserRole CustomerSupport = new UserRole(7, "Customer Support");
    }
}
