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
    }
}
