using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class RiskScore : Enumeration
    {
        public double LowRange { get; set; }
        public double HighRange { get; set; }

        public RiskScore() : base() { }

        public RiskScore(int id, string name, double lowRange, double highRange)
            : base(id, name)
        {
            LowRange = lowRange;
            HighRange = highRange;
        }

		public static readonly RiskScore Low_Risk = new RiskScore(1, "Low Risk", 0, 29.99);
		public static readonly RiskScore Medium_Risk = new RiskScore(2, "Medium Risk", 30, 70);
		public static readonly RiskScore High_Risk = new RiskScore(3, "High Risk", 70.1, 100);
	}
}
