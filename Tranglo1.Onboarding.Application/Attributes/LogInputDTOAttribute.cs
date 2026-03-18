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
    public class LogInputDTOAttribute : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = 1; // Set the order to run first
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<LogInputDTOAttribute>>();

            var inputDtoJson = JsonSerializer.Serialize(context.ActionArguments);
            logger.LogInformation($"Request for inputDTOs: {inputDtoJson}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed here
        }
    }
}
