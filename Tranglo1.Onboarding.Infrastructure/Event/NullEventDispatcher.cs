using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Infrastructure.Event
{
	internal class NullEventDispatcher : IEventDispatcher
	{
		public Task DispatchAsync(DomainEvent @event)
		{
			return Task.CompletedTask;
		}
	}
}
