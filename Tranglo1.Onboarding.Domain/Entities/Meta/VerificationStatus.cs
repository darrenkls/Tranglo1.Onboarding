using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class VerificationStatus : Enumeration
    {
        public VerificationStatus() : base() { }

        public VerificationStatus(int id, string name) : base(id, name) { }

		public static readonly VerificationStatus Pending = new VerificationStatus(1, "Pending");
		public static readonly VerificationStatus Pending_Review = new VerificationStatus(2, "Pending Review");
		public static readonly VerificationStatus Passed_By_System = new VerificationStatus(3, "Passed(by system)");
		public static readonly VerificationStatus Rejected_By_System = new VerificationStatus(4, "Rejected(by system)");
		public static readonly VerificationStatus Passed = new VerificationStatus(5, "Passed");
		public static readonly VerificationStatus Rejected = new VerificationStatus(6, "Rejected");
	}
}
