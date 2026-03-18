using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Validations
{
    public static class ErrorHandlingMiddlewareExtension
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
