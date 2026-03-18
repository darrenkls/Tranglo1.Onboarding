using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Tranglo1.Onboarding.KYCEmailReminderScheduler.Jobs;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler.DependencyInjection
{
    internal static class QuartzExtensions
    {
        internal static IServiceCollection SetupQuartz(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(q =>
            {
                #region KYC - Reminder Job
                var kycReminderJobKey = new JobKey("KYCReminderJob");
                q.AddJob<KYCReminderJob>(opts => opts.WithIdentity(kycReminderJobKey));

                // Immediate Trigger
                q.AddTrigger(opts => opts.ForJob(kycReminderJobKey)
                    .WithIdentity(kycReminderJobKey + "ImmediateTrigger")
                    .StartNow()
                );

                // Cron Trigger
                q.AddTrigger(opts => opts.ForJob(kycReminderJobKey)
                    .WithIdentity(kycReminderJobKey + "CronTrigger")
                    .WithCronSchedule(configuration.GetValue<string>("KYCReminderJobSchedule"))
                );
                #endregion

                #region KYC - Auto Reject Partner Job
                var kycRejectExpiredPartnerJobKey = new JobKey("KYCRejectExpiredPartnerJob");
                q.AddJob<KYCRejectExpiredPartnerJob>(opts => opts.WithIdentity(kycRejectExpiredPartnerJobKey));

                // Immediate Trigger
                q.AddTrigger(opts => opts.ForJob(kycRejectExpiredPartnerJobKey)
                    .WithIdentity(kycRejectExpiredPartnerJobKey + "ImmediateTrigger")
                    .StartNow()
                );

                // Cron Trigger
                q.AddTrigger(opts => opts.ForJob(kycRejectExpiredPartnerJobKey)
                    .WithIdentity(kycRejectExpiredPartnerJobKey + "CronTrigger")
                    .WithCronSchedule(configuration.GetValue<string>("KYCRejectExpiredPartnerJobSchedule"))
                );
                #endregion
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            return services;
        }
    }
}
