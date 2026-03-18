using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace ProfileScreening
{
    public class CustomJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            using var scope = _serviceProvider.CreateScope();
            var job = scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            if (job == null)
            {
                throw new InvalidOperationException($"Cannot create an instance of job type {bundle.JobDetail.JobType}.");
            }
            return job;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
