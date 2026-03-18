using CMSIntegrationTask.Services;
using Masking.Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Infrastructure.Repositories;

namespace CMSIntegrationTask
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			var BUILD_VERSION = Environment.GetEnvironmentVariable("BUILD_VERSION");

			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{environment}.json", true, true);

			configuration.AddEnvironmentVariables();
			configuration.AddCommandLine(args);

			if (environment.StartsWith("Cloud-", StringComparison.OrdinalIgnoreCase))
			{
				configuration.AddSystemsManager(aws =>
				{
					aws.Path = $"/{environment}/{nameof(CMSIntegrationTask)}";
					aws.ReloadAfter = TimeSpan.FromSeconds(30);
					aws.Optional = true;
				});
			}

			var config = configuration.Build();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.Destructure.ByMaskingProperties(x =>
				{
					x.PropertyNames.Add("Password");
					x.PropertyNames.Add("Token");
					x.PropertyNames.Add("Cookie");
					x.PropertyNames.Add("ApiKey");
					x.Mask = "******";
				})
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level} {CorrelationId}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
				.CreateLogger();

			Log.Information($"DOTNET_ENVIRONMENT : {environment}");
			Log.Information($"BUILD_VERSION : {BUILD_VERSION}");

			var services = new ServiceCollection();
			services.AddLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddSerilog(Log.Logger);
			});

			ConfigureServices(services, config);

			var provider = services.BuildServiceProvider();

			using (var scope = provider.CreateScope())
			{
				var WalletManager = scope.ServiceProvider.GetRequiredService<WalletManager>();
				var Logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
				var CorrelationIdProvider = scope.ServiceProvider.GetRequiredService<ICorrelationIdProvider>();

				LogContext.PushProperty("CorrelationId", CorrelationIdProvider.GetCorrelationId().ToString());

				try
				{
					Logger.LogInformation("Executing CMS integration task.");
					await WalletManager.ProcessWalletsAsync();
					Logger.LogInformation("CMS integration task completed.");
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, "Unexpected error when executing CMS integration task.");
				}
			}
		}

		public static void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
		{
			string _ConnectionString = Configuration.GetConnectionString("DefaultConnection");

			services.AddScoped<NotificationService>();
			services.AddScoped<WalletManager>();
			services.AddScoped<ICorrelationIdProvider>(provider =>
			{
				return new ScopedCorrelationIdProvider(Guid.NewGuid());
			});

			services.AddDbContext<PartnerDBContext>((provider, options) =>
			{
				options.UseSqlServer(_ConnectionString,
									 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
				options.EnableDetailedErrors(true);
				options.EnableSensitiveDataLogging(true);
			});

			services.AddDbContext<BusinessProfileDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
									 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
				options.EnableDetailedErrors(true);
				options.EnableSensitiveDataLogging(true);
				//options.LogTo(Console.WriteLine);
			});

			services.AddDbContext<SignUpCodeDBContext>((provider, options) =>
			{
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
									 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
				options.EnableDetailedErrors(true);
				options.EnableSensitiveDataLogging(true);
			});
			services.AddDbContext<ApplicationUserDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
									 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
				options.EnableDetailedErrors(true);
				options.EnableSensitiveDataLogging(true);
				//options.LogTo(Console.WriteLine);
			}, ServiceLifetime.Transient);

			services.AddDbContext<ExternalUserRoleDbContext>((provider, options) =>
			{
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
									 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
				options.EnableDetailedErrors(true);
				options.EnableSensitiveDataLogging(true);
			});

			services.AddScoped<IPartnerRepository, PartnerRepository>();
			services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
			//services.AddScoped<ICompanyRepository, CompanyRepository>();
			services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
			services.AddScoped<ISignUpCodeRepository, SignUpCodeRepository>();
			services.AddScoped<IExternalUserRoleRepository, ExternalUserRoleRepository>();

			services.AddScoped<BusinessProfileService>();
			services.AddBackendIdentityContext();
			services.AddNullEventDispatcher();

			services.AddSingleton<IConfiguration>(Configuration);
			services.AddSqlServerUnitOfWork(_ConnectionString);

			services.AddHttpClient<WalletManager>()
				.ConfigureHttpClient((provider, httpClient) =>
			{
				var CMSApiUri = Configuration["CMSApiUri"];
				var CMSApiKey = Configuration["CMSApiKey"];

				var _CorrelationIdProvider = provider.GetService<ICorrelationIdProvider>();
				httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", _CorrelationIdProvider.GetCorrelationId().ToString());

				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("api", CMSApiKey);
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.BaseAddress = new Uri(CMSApiUri);
			});

			Log.Verbose($"CMS API Endpoint: {Configuration["CMSApiUri"]}");
		}
	}
}
