using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CompanyUserAccountStatus : Enumeration
    {
        public CompanyUserAccountStatus() : base() { }

        public CompanyUserAccountStatus(int id, string name) : base(id, name) { }

        public static readonly CompanyUserAccountStatus Active = new CompanyUserAccountStatus(1, "Active");
        public static readonly CompanyUserAccountStatus Inactive = new CompanyUserAccountStatus(2, "Inactive");
        public static readonly CompanyUserAccountStatus PendingActivation = new CompanyUserAccountStatus(3, "Pending Activation");
    }
}
