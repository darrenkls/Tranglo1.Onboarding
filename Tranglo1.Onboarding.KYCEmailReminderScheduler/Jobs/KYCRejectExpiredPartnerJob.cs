using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler.Jobs
{
    internal class KYCRejectExpiredPartnerJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KYCRejectExpiredPartnerJob> _logger;

        public KYCRejectExpiredPartnerJob(IServiceScopeFactory scopeFactory,
            RejectKycPartnerService rejectKycPartnerService,
            ILogger<KYCRejectExpiredPartnerJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Reject Expired KYC Partner Job started...");

                using var scope = _scopeFactory.CreateScope();
                var rejectKycPartnerService = scope.ServiceProvider.GetService<RejectKycPartnerService>();

                await rejectKycPartnerService.RejectExpiredKYCPartners();

                _logger.LogInformation("Reject Expired KYC Partner Job completed...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reject Expired KYC Partner Job failed.");
            }
        }
    }
}
