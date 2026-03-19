using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.SignUpCodes
{
    public class LeadsOrigin : Enumeration
    {
        public LeadsOrigin() : base() { }

        public LeadsOrigin(int id, string name) : base(id, name) { }
    }
}
