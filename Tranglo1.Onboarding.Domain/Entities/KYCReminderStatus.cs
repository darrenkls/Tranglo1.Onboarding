using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class KYCReminderStatus : Enumeration
    {
        public KYCReminderStatus() : base() { }

        public KYCReminderStatus(int id, string name) : base(id, name) { }

        public static readonly KYCReminderStatus Unsubscribed = new KYCReminderStatus(1, "Unsubscribed");
        public static readonly KYCReminderStatus Expired = new KYCReminderStatus(2, "Expired");
    }
}
