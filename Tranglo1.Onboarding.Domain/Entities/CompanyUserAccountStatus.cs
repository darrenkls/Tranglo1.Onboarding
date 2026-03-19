using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CompanyUserAccountStatus : Enumeration
    {
        public CompanyUserAccountStatus() : base() { }

        public CompanyUserAccountStatus(int id, string name) : base(id, name) { }
    }
}
