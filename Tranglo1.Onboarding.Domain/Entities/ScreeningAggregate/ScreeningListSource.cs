using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate
{
    public class ScreeningListSource : Enumeration
    {
        public ScreeningListSource()
        {

        }
        public ScreeningListSource(int id, string name) : base(id, name)
        {

        }

        public static readonly ScreeningListSource WorldCompliance = new ScreeningListSource(1, "World Compliance");
        public static readonly ScreeningListSource Internal = new ScreeningListSource(2, "Internal");

        public string GetApiPathName()
        {
            if (this == WorldCompliance)
            {
                return "world-compliance";
            }
            else if (this == Internal)
            {
                return "internal-list";
            }

            return null;
        }
    }
}
