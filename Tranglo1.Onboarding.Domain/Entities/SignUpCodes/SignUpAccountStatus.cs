using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.SignUpCodes
{
    public class SignUpAccountStatus : Enumeration
    {
        public SignUpAccountStatus() : base() { }

        public SignUpAccountStatus(int id, string name) : base(id, name) { }
    }
}
