using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate
{
    public class ScreeningDetailsCategory : Enumeration
    {
        public ScreeningDetailsCategory(int id, string name) : base(id, name)
        {
        }

        public ScreeningDetailsCategory()
        {
        }

        public static readonly ScreeningDetailsCategory PEP = new ScreeningDetailsCategory(1, "PEP");
        public static readonly ScreeningDetailsCategory Sanctions = new ScreeningDetailsCategory(2, "Sanctions");
        public static readonly ScreeningDetailsCategory SOE = new ScreeningDetailsCategory(3, "SOE");
        public static readonly ScreeningDetailsCategory Adverse_Media = new ScreeningDetailsCategory(4, "Adverse Media");
        public static readonly ScreeningDetailsCategory Enforcement = new ScreeningDetailsCategory(5, "Enforcement");
    }
}
