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
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.Onboarding.KYCEmailReminderScheduler.DependencyInjection;
using Tranglo1.Onboarding.KYCEmailReminderScheduler.Notification;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "DevelopmentP2";
            var BUILD_VERSION = Environment.GetEnvironmentVariable("BUILD_VERSION");

            var configBuilder = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", false, true)
              .AddJsonFile($"appsettings.{environment}.json", true, true);

            configBuilder.AddEnvironmentVariables();
            configBuilder.AddCommandLine(args);

            if (environment.StartsWith("Cloud-", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Running in container.");
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

            LogContext.PushProperty("CorrelationId", Guid.NewGuid().ToString());

            try
            {
                var host = Host.CreateDefaultBuilder(args)
                    .UseWindowsService()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.Sources.Clear();

                        config.AddJsonFile("appsettings.json", false, true)
                            .AddJsonFile($"appsettings.{environment}.json", true, true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args);
                    })
                    .UseSerilog()
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services, config);
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddSerilog(Log.Logger);
            });

            services.AddEntityFrameworkSqlServer();

            services.AddHttpClient();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddScoped<NotificationService>();

            bool kycReminderJobIsEnable = configuration.GetValue<bool>("KycReminderJobIsEnable");

            if (kycReminderJobIsEnable)
                services.AddScoped<ReminderService>();


            services.AddScoped<RejectKycPartnerService>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

            services.AddDbContext<BusinessProfileDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                                     o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                options.EnableDetailedErrors(true);
                //options.EnableSensitiveDataLogging(true);
                //options.LogTo(Console.WriteLine);
            });
            services.AddDbContext<PartnerDBContext>((provider, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                                     o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                options.EnableDetailedErrors(true);
                //options.EnableSensitiveDataLogging(true);
            });
            services.AddDbContext<ApplicationUserDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                                     o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                options.EnableDetailedErrors(true);
                //options.EnableSensitiveDataLogging(true);
                //options.LogTo(Console.WriteLine);
            }, ServiceLifetime.Transient);
            services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();

            services.AddBackendIdentityContext();
            services.AddNullEventDispatcher();
            services.AddSqlServerUnitOfWork(configuration.GetConnectionString("DefaultConnection"));

            services.SetupQuartz(configuration);
        }
    }
}
