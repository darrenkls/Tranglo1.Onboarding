using ActiveDirectoryManager.Configurations;
using ActiveDirectoryManager.Jobs;
using EntityFrameworkCore.SqlServer.TemporalTable.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ActiveDirectoryManager.Test")]

namespace ActiveDirectoryManager
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			var BUILD_VERSION = Environment.GetEnvironmentVariable("BUILD_VERSION");

			//Source: https://www.c-sharpcorner.com/article/serilog-in-dotnet-core/
			var configBuilder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{environment}.json", true, true);

			configBuilder.AddEnvironmentVariables();
			configBuilder.AddCommandLine(args);

			if (environment.StartsWith("Cloud-", StringComparison.OrdinalIgnoreCase))
			{
				configBuilder.AddSystemsManager(aws =>
				{
					aws.Path = $"/{environment}/{nameof(ActiveDirectoryManager)}";
					aws.ReloadAfter = TimeSpan.FromSeconds(30);
					aws.Optional = true;
				});
			}

			var config = configBuilder.Build();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.MinimumLevel.Verbose()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
				.CreateLogger();

			Log.Information($"DOTNET_ENVIRONMENT : {environment}");
			Log.Information($"BUILD_VERSION : {BUILD_VERSION}");

			var services = new ServiceCollection();
			ConfigureServices(services, config);

			var provider = services.BuildServiceProvider();

			using (var scope = provider.CreateScope())
			{
				try
				{
					LogContext.PushProperty("CorrelationId", Guid.NewGuid().ToString());

					Log.Information("Application started.");
					var _LdapAccountSynchronizer = scope.ServiceProvider.GetService<LdapAccountSynchronizer>();
					await _LdapAccountSynchronizer.ExecuteAsync();
					Log.Information("Application completed.");

				}
				catch (Exception ex)
				{
					Log.Fatal(ex, "The Application failed to start.");
				}
				finally
				{
					Log.CloseAndFlush();
				}
			}

		}

		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			var ldapOptions = configuration
				.GetSection(nameof(LdapConfigurations))
				.Get<LdapConfigurations>();

			//services.Configure<LdapConfigurations>(configuration.GetSection(nameof(LdapConfigurations)));
			services.AddSingleton<LdapConfigurations>(ldapOptions);

			services.AddLogging(configure =>
			{
				configure.ClearProviders();
				configure.AddSerilog(Log.Logger);
			});

			services.AddLdapServices();

			string _ConnectionString = configuration.GetConnectionString("IdentityServer");

			services.AddSqlServerUnitOfWork(_ConnectionString);

			services.AddLdapRepository((provider, options) =>
			{
				options.UseSqlServer(_ConnectionString);
				options.UseInternalServiceProvider(provider);
			});
			services.AddEntityFrameworkSqlServer();
			services.RegisterTemporalTablesForDatabase();

			services.AddScoped<LdapAccountSynchronizer>();
		}
	}
}
