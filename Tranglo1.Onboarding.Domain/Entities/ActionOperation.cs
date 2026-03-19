using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ActionOperation : Enumeration
    {
        public ActionOperation() : base() { }

        public ActionOperation(int id, string name) : base(id, name) { }
    }
}
