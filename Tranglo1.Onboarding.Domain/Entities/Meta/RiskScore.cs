using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class RiskScore : Enumeration
    {
        public decimal LowRange { get; set; }
        public decimal HighRange { get; set; }

        public RiskScore() : base() { }

        public RiskScore(int id, string name, decimal lowRange, decimal highRange)
            : base(id, name)
        {
            LowRange = lowRange;
            HighRange = highRange;
        }

        public static readonly RiskScore Low_Risk = new RiskScore(1, "Low Risk", 0, 39);
        public static readonly RiskScore Medium_Risk = new RiskScore(2, "Medium Risk", 40, 69);
        public static readonly RiskScore High_Risk = new RiskScore(3, "High Risk", 70, 100);
    }
}
