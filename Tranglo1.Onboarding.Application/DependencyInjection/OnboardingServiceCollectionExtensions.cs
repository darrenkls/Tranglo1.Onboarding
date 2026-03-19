using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tranglo1.Onboarding.Application.Managers;
using Tranglo1.Onboarding.Application.Services.Identity;

namespace Tranglo1.Onboarding.Application.DependencyInjection
{
    public static class OnboardingServiceCollectionExtensions
    {
        /// <summary>
        /// Returns the Onboarding.Application assembly for use in assembly scanning
        /// (MediatR, AutoMapper, FluentValidation).
        /// </summary>
        public static Assembly OnboardingApplicationAssembly =>
            typeof(OnboardingApplicationAssemblyMarker).Assembly;

        /// <summary>
        /// Registers onboarding-specific services from the Application layer.
        /// </summary>
        public static IServiceCollection AddOnboardingApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IntegrationManager>();
            services.AddScoped<ApplicationUserService>();

            return services;
        }
    }
}
