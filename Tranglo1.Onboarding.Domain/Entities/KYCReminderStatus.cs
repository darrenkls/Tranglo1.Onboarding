using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class KYCReminderStatus : Enumeration
    {
        public KYCReminderStatus() : base() { }

        public KYCReminderStatus(int id, string name) : base(id, name) { }
    }
}
