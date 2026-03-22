using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class SystemEnvironment : Enumeration
    {
        public SystemEnvironment() : base()
        {
        }

        public SystemEnvironment(int id, string name)
            : base(id, name)
        {
        }

        public static readonly SystemEnvironment Staging = new SystemEnvironment(1, "Staging");
        public static readonly SystemEnvironment Production = new SystemEnvironment(2, "Production");
    }
}
