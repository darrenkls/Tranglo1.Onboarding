using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class UnitOfWorkExtensions
	{
		/// <summary>
		/// All <seealso cref="Tranglo1.Onboarding.Infrastructure.Persistence.BaseDbContext"/> that are 
		/// using the same connection string will share the same DbTransaction.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static IServiceCollection AddSqlServerUnitOfWork(this IServiceCollection services, 
			string connectionString)
		{
			if (services != null)
			{
				//Use the factory version, so that the real instance of IUnitOfWork will 
				//be created when there is a DbContext initiated in current scope too.
				//means : no sql connection will be created if there is no DbContext is used in current scope
				services.AddScoped<IUnitOfWork>(provider =>
				{
					return new SqlServerUnitOfWork(connectionString);
				});
			}

			return services;
		}
	}
}
