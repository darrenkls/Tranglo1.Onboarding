using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ExternalUserRoleStatus : Enumeration
    {
        public ExternalUserRoleStatus() : base() { }

        public ExternalUserRoleStatus(int id, string name) : base(id, name) { }
    }
}
