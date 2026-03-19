using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class KYCReminderSubscription : Enumeration
    {
        public KYCReminderSubscription() : base() { }

        public KYCReminderSubscription(int id, string name) : base(id, name) { }
    }
}
