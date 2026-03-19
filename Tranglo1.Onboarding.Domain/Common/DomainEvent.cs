using System;

namespace Tranglo1.Onboarding.Domain.Common
{
    public abstract class DomainEvent
    {
        protected DomainEvent()
        {
            DateOccurred = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset DateOccurred { get; protected set; } = DateTime.UtcNow;
    }
}
