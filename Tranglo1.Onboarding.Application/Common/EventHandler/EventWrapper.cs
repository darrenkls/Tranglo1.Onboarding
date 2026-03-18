using MediatR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.Common.EventHandlers
{
	class EventWrapper<TEvent> : INotification where TEvent : DomainEvent
	{
		public TEvent UnderlyingEvent { get; private set; }

		internal EventWrapper(TEvent e)
		{
			this.UnderlyingEvent = e;
		}

		//public static INotification Create<T>(T e) where T : DomainEvent
		//{
		//	return new EventWrapper<T>(e);
		//}

		public static INotification Create(DomainEvent e)
		{
			var factory = GetFactory(e);
			var n = factory(e);
			return n;
		}

		private static ConcurrentDictionary<Type, Func<DomainEvent, INotification>> _EventWrapperFactories =
			new ConcurrentDictionary<Type, Func<DomainEvent, INotification>>();

		private static Func<DomainEvent, INotification> GetFactory(DomainEvent e)
		{
			//EventWrapper<SenderRegisteredEvent> w1 =
			//	new EventWrapper<SenderRegisteredEvent>(e as SenderRegisteredEvent);


			//We have DomainEvent here, but we need to create instance of EventWrapper<TEvent>, 
			//with the generic type is the actual event type (ex: EventWrapper<SenderRegisteredEvent>)
			//Solution: use reflection, or Expression method


			/*
			 * Here is the non-generic way to create INotification instance to MediatR:
			 * 
			 * EventWrapper<SenderRegisteredEvent> w1 = 
			 *		new EventWrapper<SenderRegisteredEvent>(e as SenderRegisteredEvent);
			 * 
			 * We will create a lambda expression that will
			 *	1. cast received DomainEvent into event actual type using "as" operator.
			 *	2. instantiate instance of EventWrapper with the actual event type 
			 *	
			 *	
			 *	The lambda function we are going to create will have the following signature:
			 *			Func<DomainEvent, INotification>
			 *	
			 *	Where DomainEvent instance is the instance we receive in this function, and it will 
			 *	return an instance of EventWrapper<TEvent> (which is implement INotification interface)
			 *	
			 */


			//Ex: SenderRegisteredEvent
			Type eventType = e.GetType();

			return _EventWrapperFactories.GetOrAdd(eventType, et =>
			{
				//Ex: type for EventWrapper<SenderRegisteredEvent>
				Type wrapperType = typeof(EventWrapper<>).MakeGenericType(et);

				//this will get 
				//	private EventWrapper(TEvent e)
				ConstructorInfo ctor = wrapperType.GetConstructor(
					BindingFlags.Instance | BindingFlags.NonPublic,
					null,
					new Type[] { et },
					null);

				ParameterExpression eventParam = Expression.Parameter(typeof(DomainEvent));

				Expression<Func<DomainEvent, INotification>> expression =
					Expression.Lambda<Func<DomainEvent, INotification>>(
						Expression.Block(
							Expression.New(
								ctor, Expression.TypeAs(eventParam, et)
							)
						),
						eventParam
					);

				return expression.Compile();
			});
		}
	}

}
