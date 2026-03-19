using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AccountStatus : Enumeration
    {
        public AccountStatus() : base() { }

        public AccountStatus(int id, string name) : base(id, name) { }

        public static readonly AccountStatus Active = new AccountStatus(1, "Active");
        public static readonly AccountStatus Inactive = new AccountStatus(2, "Inactive");
        public static readonly AccountStatus PendingApproval = new AccountStatus(3, "Pending Approval");
        public static readonly AccountStatus Suspended = new AccountStatus(4, "Suspended");
        public static readonly AccountStatus PendingActivation = new AccountStatus(5, "Pending Activation");
        public static readonly AccountStatus Blocked = new AccountStatus(6, "Blocked");
    }
}
