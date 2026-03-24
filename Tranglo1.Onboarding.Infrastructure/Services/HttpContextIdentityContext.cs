using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
    internal class HttpContextIdentityContext : IIdentityContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextIdentityContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal CurrentUser =>
            _httpContextAccessor.HttpContext?.User ?? ClaimsPrincipal.Current;
    }
}
