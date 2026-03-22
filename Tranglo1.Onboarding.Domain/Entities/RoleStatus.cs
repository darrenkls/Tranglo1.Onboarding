using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class RoleStatus : Enumeration
    {
        public RoleStatus() : base() { }

        public RoleStatus(int id, string name) : base(id, name) { }

        public static readonly RoleStatus Active = new RoleStatus(1, "Active");
        public static readonly RoleStatus Inactive = new RoleStatus(2, "Inactive");
    }
}
