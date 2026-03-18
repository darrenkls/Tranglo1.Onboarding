using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace MediatR
{
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<TRequest> _logger;
        public IUnitOfWork _UnitOfWork { get; }

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger,
                                            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _UnitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                //TODO: log into physical file for debugging
                _logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}", requestName, request);

                try
                {
                    //Rollback
                    await _UnitOfWork.RollbackAsync();
                }
                catch (Exception exRoll)
                {
                    //log the error if possible. Exception on result.Value causing issue on ErrorHandling
                    _logger.LogError($"Unexpected rollback error occurred. {exRoll.Message}");
                }

                throw;
            }
        }
    }
}
