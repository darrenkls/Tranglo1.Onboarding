using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CompanyUserBlockStatus : Enumeration
    {
        public CompanyUserBlockStatus() : base() { }

        public CompanyUserBlockStatus(int id, string name) : base(id, name) { }
    }
}
