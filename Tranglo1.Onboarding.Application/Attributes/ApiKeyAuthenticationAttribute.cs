using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthenticationAttribute : Attribute, IAsyncActionFilter
    {
        
        private readonly string _APIKEYHEADER = "X-T1-API-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var _configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            string apiKey = _configuration.GetValue<string>("ApiKeyValue");
            if(!context.HttpContext.Request.Headers.TryGetValue(_APIKEYHEADER, out var requestApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Api Key Not Provided"
                };
                return;
            }
            if (!apiKey.Equals(requestApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = "Invalid Api Key"
                };
                return;
            }


            await next();
        }
    }
}
