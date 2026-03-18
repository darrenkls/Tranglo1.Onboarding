using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Quartz;
using Quartz.Spi;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance;
using Tranglo1.Onboarding.Domain.ExternalServices.Watchlist;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Commons;
using Tranglo1.Onboarding.Infrastructure.ExternalServices;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Infrastructure.Repositories;

namespace ProfileScreening
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal(ex, "The Application failed to start.");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        #region Private Helper Methods
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var _RunInContainer = System.Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            var _HostBuilder = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(c =>
                {
                    c.ClearProviders();
                    c.AddConsole();
                })
                .UseNLog()
                .ConfigureAppConfiguration((host, configuration) =>
                {
                    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables()
                          .AddCommandLine(args);
                })
                 .ConfigureServices((hostContext, services) =>
                 {
                     ConfigureServices(services, hostContext.Configuration, hostContext.HostingEnvironment);
                 });

            return _HostBuilder;
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDbContext<BusinessProfileDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            services.AddSqlServerUnitOfWork(configuration.GetConnectionString("DefaultConnection"));
            services.AddNullEventDispatcher();
            services.AddBackendIdentityContext();
            services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
            services.AddTransient<LoggingHandler<ComplianceExternalService>>();
            services.AddHttpClient<IComplianceExternalService, ComplianceExternalService>()
                .ConfigureHttpClient(e =>
                {
                    string baseUrl = configuration.GetValue<string>("ComplianceScreeningAPI");
                    string apiKey = configuration.GetValue<string>("ComplianceScreeningAPIKey");

                    e.BaseAddress = new Uri(baseUrl);
                    e.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TIKAPI", apiKey);

                    e.DefaultRequestHeaders.UserAgent.Clear();
                    e.DefaultRequestHeaders.UserAgent.ParseAdd($"ComplianceExternalService-{hostEnvironment.EnvironmentName}/1.0");

                    // The following settings are available in .NET 5.0 or greater
                    #if NET5_0_OR_GREATER
                        e.DefaultRequestVersion = new Version(2, 0);
                        e.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                    #endif
                })
                .AddHttpMessageHandler<LoggingHandler<ComplianceExternalService>>()
                .AddPolicyHandler(GetRetryPolicy());

            services.AddTransient<LoggingHandler<WatchlistNotificationExternalService>>();
            services.AddHttpClient<IWatchlistNotificationExternalService, WatchlistNotificationExternalService>()
                .ConfigureHttpClient(e =>
                {
                    string baseUrl = configuration.GetValue<string>("IdentityServerUri");
                    string apiKey = configuration.GetValue<string>("ApiKeyValue");

                    e.BaseAddress = new Uri(baseUrl);
                    e.DefaultRequestHeaders.Add("X-T1-API-Key", apiKey);

                    e.DefaultRequestHeaders.UserAgent.Clear();
                    e.DefaultRequestHeaders.UserAgent.ParseAdd($"WatchlistNotificationExternalService-{hostEnvironment.EnvironmentName}/1.0");

                    // The following settings are available in .NET 5.0 or greater
                    #if NET5_0_OR_GREATER
                        e.DefaultRequestVersion = new Version(2, 0);
                        e.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                    #endif
                })
                .AddHttpMessageHandler<LoggingHandler<WatchlistNotificationExternalService>>()
                .AddPolicyHandler(GetRetryPolicy());
            services.AddScoped<ComplianceScreeningService>();
            services.AddScoped<ProfileScreeningJob>();

            // Register the custom job factory
            services.AddSingleton<IJobFactory, CustomJobFactory>();

            // Register database retry policy as singleton
            services.AddSingleton<AsyncRetryPolicy>(sp =>
            {
                return GetDatabaseRetryPolicy(configuration);
            });

            // Configure Quartz
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("ProfileScreeningJob");
                q.AddJob<ProfileScreeningJob>(opts => opts.WithIdentity(jobKey));

                // Job Trigger
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("ProfileScreeningJobTrigger")
                    .WithCronSchedule(configuration.GetValue<string>("ProfileScreening:TriggerTime")));
            });

            services.AddQuartzHostedService(q =>
            {
                q.WaitForJobsToComplete = true;
            });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // network errors + 5xx + 408
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan}. " +
                                          $"Reason: {(outcome.Exception != null ? outcome.Exception.Message : outcome.Result.StatusCode.ToString())}");
                    });
        }

        private static AsyncRetryPolicy GetDatabaseRetryPolicy(IConfiguration configuration)
        {
            var maxRetryAttempts = configuration.GetValue<int>("ProfileScreening:MaxRetryAttempts");
            var initialRetryDelayMs = configuration.GetValue<int>("ProfileScreening:InitialRetryDelayMs");

            return Policy
                .Handle<SqlException>(ex => IsTransientSqlException(ex))
                .Or<DbUpdateConcurrencyException>()
                .WaitAndRetryAsync(
                    retryCount: maxRetryAttempts,
                    sleepDurationProvider: retryAttempt =>
                    {
                        // Database retries use milliseconds (100ms, 200ms, 400ms) vs HTTP retries which use seconds
                        // This is intentional - deadlocks are resolved quickly by SQL Server
                        var baseDelay = initialRetryDelayMs * Math.Pow(2, retryAttempt - 1);

                        // Add jitter to prevent thundering herd when multiple tasks retry simultaneously
                        var jitter = Math.Abs(Guid.NewGuid().GetHashCode() % 50);
                        return TimeSpan.FromMilliseconds(baseDelay + jitter);
                    },
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        var businessProfileId = context.ContainsKey("BusinessProfileId") ? context["BusinessProfileId"] : "Unknown";
                        Console.WriteLine(
                            $"DatabaseRetryPolicy: Transient error for BusinessProfileId {businessProfileId}. " +
                            $"Retrying in {timeSpan.TotalMilliseconds}ms (Attempt {retryCount}/{maxRetryAttempts}). Error: {exception.Message}");
                    });
        }

        private static bool IsTransientSqlException(SqlException sqlEx)
        {
            // SQL Server error codes that are typically transient:
            // 1205 = Deadlock
            // -2 = Timeout
            // 64 = Connection failed
            // 233 = Connection initialization error
            // 10053 = Transport-level error
            // 10054 = Transport-level error
            // 40197 = Service error processing request
            // 40501 = Service is busy
            // 40613 = Database unavailable
            return sqlEx.Number == 1205 ||
                   sqlEx.Number == -2 ||
                   sqlEx.Number == 64 ||
                   sqlEx.Number == 233 ||
                   sqlEx.Number == 10053 ||
                   sqlEx.Number == 10054 ||
                   sqlEx.Number == 40197 ||
                   sqlEx.Number == 40501 ||
                   sqlEx.Number == 40613;
        }
        #endregion Private Helper Methods
    }
}
