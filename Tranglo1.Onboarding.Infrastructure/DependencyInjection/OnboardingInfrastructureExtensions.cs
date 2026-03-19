using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance;
using Tranglo1.Onboarding.Domain.ExternalServices.Watchlist;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Commons;
using Tranglo1.Onboarding.Infrastructure.ExternalServices;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Infrastructure.DependencyInjection
{
    public static class OnboardingInfrastructureExtensions
    {
        public static IServiceCollection AddOnboardingInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
            services.AddScoped<IScreeningRepository, ScreeningRepository>();
            services.AddScoped<IRBARepository, RBARepository>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<ISignUpCodeRepository, SignUpCodeRepository>();
            services.AddScoped<IExternalUserRoleRepository, ExternalUserRoleRepository>();
            services.AddScoped<IStaffEntityQueryService, ApplicationUserRepository>();
            services.AddScoped<CsvExporter>();

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

                    e.DefaultRequestVersion = new Version(2, 0);
                    e.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
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

                    e.DefaultRequestVersion = new Version(2, 0);
                    e.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                })
                .AddHttpMessageHandler<LoggingHandler<WatchlistNotificationExternalService>>()
                .AddPolicyHandler(GetRetryPolicy());

            return services;
        }

        #region Private Helper Methods
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan}. " +
                                          $"Reason: {(outcome.Exception != null ? outcome.Exception.Message : outcome.Result.StatusCode.ToString())}");
                    });
        }
        #endregion Private Helper Methods
    }
}
