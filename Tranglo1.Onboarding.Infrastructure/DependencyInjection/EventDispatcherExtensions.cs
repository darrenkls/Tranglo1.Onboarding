using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Infrastructure.Event;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class EventDispatcherExtensions
	{
		public static IServiceCollection AddNullEventDispatcher(this IServiceCollection services)
		{
			services.AddScoped<IEventDispatcher, NullEventDispatcher>();
			return services;
		}
	}
}
