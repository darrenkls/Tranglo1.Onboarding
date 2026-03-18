using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace MediatR
{
    class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;
        public IUnitOfWork _UnitOfWork { get; }

        public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger,
                               IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _UnitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation($"Rollback Error Handling Behaviour for {requestName}");

            var response = await next();

            if (response is IResult result && result.IsFailure)
            {
                try
                {
                    //Rollback
                    await _UnitOfWork.RollbackAsync();
                }
                catch (Exception ex)
                {
                    //log the error if possible. Exception on result.Value causing issue on ErrorHandling
                    _logger.LogError($"Unexpected rollback error occurred. {ex.Message}");
                }
            }

            return response;
        }

    }
}
