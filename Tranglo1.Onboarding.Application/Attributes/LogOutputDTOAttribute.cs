using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Attributes
{
    public class LogOutputDTOAttribute : Attribute, IAsyncResultFilter, IOrderedFilter
    {
        public int Order { get; set; } = 2; // Set the order to run second after input
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Execute the action
            var resultContext = await next();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<LogOutputDTOAttribute>>();

            // Check for exceptions or unsuccessful status codes before logging
            if (resultContext.Result is BadRequestObjectResult badRequestObjectResult)
            {
                // Log the bad request 400 error
                var responseJson = JsonSerializer.Serialize(badRequestObjectResult.Value);
                logger.LogInformation($"Bad Request Response: {responseJson}");
            }
        }
    }
}
