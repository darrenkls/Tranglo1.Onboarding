using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.Common.EventHandlers
{
	abstract class BaseEventHandler<TEvent> : INotificationHandler<EventWrapper<TEvent>> where TEvent : DomainEvent
	{
		Task INotificationHandler<EventWrapper<TEvent>>.Handle(
			EventWrapper<TEvent> notification, CancellationToken cancellationToken)
		{
			return this.HandleAsync(notification.UnderlyingEvent, cancellationToken);
		}

		protected abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
	}
}
