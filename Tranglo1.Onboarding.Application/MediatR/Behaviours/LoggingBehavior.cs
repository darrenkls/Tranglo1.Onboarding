using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            //_logger.LogInformation($"Handling {typeof(TRequest).Name}");
            var requestName = typeof(TRequest).Name;
            var requestJson = JsonSerializer.Serialize(request);
            _logger.LogInformation($"Request for {requestName} {requestJson}");

            var response = await next();

            if (response is IResult result && result.IsFailure)
            {
                try
                {
                    var responseJson = JsonSerializer.Serialize(response);
                    _logger.LogError($"Request {requestName} resulted in an error: {responseJson}");
                }
                catch (Exception ex)
                {
                    //log the error if possible. Exception on result.Value causing issue on logging
                    _logger.LogError($"Unexpected error occurred. {ex.Message}");
                }
            }

            return response;
        }

    }
}
