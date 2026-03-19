using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ServiceType : Enumeration
    {
        public ServiceType() : base() { }

        public ServiceType(int id, string name) : base(id, name) { }
    }
}
