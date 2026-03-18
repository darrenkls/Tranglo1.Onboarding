using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler.Jobs
{
    internal class KYCReminderJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KYCReminderJob> _logger;

        public KYCReminderJob(IServiceScopeFactory scopeFactory,
            ILogger<KYCReminderJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Incomplete KYC Email Reminder Job started...");

                using var scope = _scopeFactory.CreateScope();
                var reminderService = scope.ServiceProvider.GetService<ReminderService>();

                await reminderService.SendEmailReminder();

                _logger.LogInformation("Incomplete KYC Email Reminder Job completed...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Incomplete KYC Email Reminder Job failed.");
            }
        }
    }
}
