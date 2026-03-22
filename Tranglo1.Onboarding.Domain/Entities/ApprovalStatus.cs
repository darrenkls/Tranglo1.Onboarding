using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ApprovalStatus : Enumeration
    {
        public ApprovalStatus() : base() { }

        public ApprovalStatus(int id, string name) : base(id, name) { }

        public static readonly ApprovalStatus Pending_L1_Approval = new ApprovalStatus(1, "Pending L1 Approval");
        public static readonly ApprovalStatus Pending_L2_Approval = new ApprovalStatus(2, "Pending L2 Approval");
        public static readonly ApprovalStatus Approved = new ApprovalStatus(3, "Approved");
        public static readonly ApprovalStatus Rejected = new ApprovalStatus(4, "Rejected");
    }
}
