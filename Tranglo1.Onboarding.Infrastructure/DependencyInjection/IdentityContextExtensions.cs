using Tranglo1.Onboarding.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityContextExtensions
    {
        public static IServiceCollection AddBackendIdentityContext(this IServiceCollection services)
        {
            services.AddSingleton<IIdentityContext, BackendIdentityContext>();
            return services;
        }

        public static IServiceCollection AddWebIdentityContext(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IIdentityContext, HttpContextIdentityContext>();
            return services;
        }
    }
}
