using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ExternalUserRoleStatus : Enumeration
    {
        public ExternalUserRoleStatus() : base() { }

        public ExternalUserRoleStatus(int id, string name) : base(id, name) { }

        public static readonly ExternalUserRoleStatus Active = new ExternalUserRoleStatus(1, "Active");
        public static readonly ExternalUserRoleStatus Inactive = new ExternalUserRoleStatus(2, "Inactive");
    }
}
