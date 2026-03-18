using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.CustomerIdentity.Domain.DomainServices;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddLdapServices(this IServiceCollection services)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			services.AddScoped<LdapAccountManager>();

			return services;
		}
	}
}
