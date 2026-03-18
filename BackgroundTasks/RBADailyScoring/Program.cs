using Masking.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading.Tasks;
using Tranglo1.RBADailyScoring.Jobs;

namespace Tranglo1.RBADailyScoring
{
    class Program
    {
        private static IConfiguration config;

        private static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            var BUILD_VERSION = Environment.GetEnvironmentVariable("BUILD_VERSION");

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);

            if (environment.StartsWith("Cloud-", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Running in container");

                configBuilder.AddSystemsManager(aws =>
                {
                    Console.WriteLine($"environment = {environment}");

                    aws.Path = $"/{environment}/{nameof(RBADailyScoring)}";
                    aws.ReloadAfter = TimeSpan.FromSeconds(30);
                    aws.Optional = true;
                });
            }

            config = configBuilder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .MinimumLevel.Verbose()
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
            services.AddLogging(configure =>
            {
                configure.AddSerilog(Log.Logger);
            });

            ConfigureServices(services, config);

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var _Job = scope.ServiceProvider.GetService<RBADailyScoringJob>();

                LogContext.PushProperty("CorrelationId", Guid.NewGuid().ToString());

                try
                {
                    Log.Information("Job Executing.");
                    await _Job.Execute();
                    Log.Information("Job Completed.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<RBADailyScoringJob>();
            services.AddSingleton<IConfiguration>(config);
        }
    }
}
