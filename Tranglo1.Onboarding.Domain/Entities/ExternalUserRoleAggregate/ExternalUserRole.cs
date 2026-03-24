using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate
{
    public class ExternalUserRole : Entity
    {
        public string RoleCode { get; set; }
		public string ExternalUserRoleName { get; set; }
		public Solution Solution { get; set; }
        public ExternalUserRoleStatus Status { get; set; }
    }
}
