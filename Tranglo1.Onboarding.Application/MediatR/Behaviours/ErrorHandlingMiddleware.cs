using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ValidationException = Tranglo1.Onboarding.Application.Common.Exceptions.ValidationException;

namespace Tranglo1.Onboarding.Application.Validations
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log issues and handle exception response
            var type = exception.GetType();

            if (exception.GetType() == typeof(ValidationException))
            {
                var code = HttpStatusCode.UnprocessableEntity;
                var result = JsonConvert.SerializeObject(
                    new {   isSuccess = false, 
                            errors = ((ValidationException)exception).Errors,
                            type = "http link to get more error information.",
                            title = "One or more validation errors occurred.",
                            detail = "See the errors property for details."
                    });

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)code;
                return context.Response.WriteAsync(result);

            }
            else
            {
                var code = HttpStatusCode.InternalServerError;
                var result = JsonConvert.SerializeObject(new { isSuccess = false, error = exception.Message });
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                return context.Response.WriteAsync(result);
            }
        }
    }
}
