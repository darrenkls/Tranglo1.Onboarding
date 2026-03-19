using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloEntity : Enumeration
    {
        public TrangloEntity() : base() { }

        public TrangloEntity(int id, string name) : base(id, name) { }
    }
}
