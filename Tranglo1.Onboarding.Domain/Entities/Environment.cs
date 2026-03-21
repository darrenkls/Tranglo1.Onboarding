using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Environment : Enumeration
    {
        public Environment() : base() { }

        public Environment(int id, string name) : base(id, name) { }

        public static readonly Environment Staging = new Environment(1, "Staging");
        public static readonly Environment Production = new Environment(2, "Production");
    }
}
