using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Infrastructure.Event
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(DomainEvent @event);

    }
}
