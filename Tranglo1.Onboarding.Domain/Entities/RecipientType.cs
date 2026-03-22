using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class RecipientType : Enumeration
    {
        public RecipientType() : base() { }

        public RecipientType(int id, string name) : base(id, name) { }

        public static readonly RecipientType TO = new RecipientType(1, "TO");
        public static readonly RecipientType CC = new RecipientType(2, "CC");
        public static readonly RecipientType BCC = new RecipientType(3, "BCC");
    }
}
