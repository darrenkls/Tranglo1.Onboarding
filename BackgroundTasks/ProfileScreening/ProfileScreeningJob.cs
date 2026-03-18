using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using ProfileScreening.DTOs;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.ExternalServices.Watchlist;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace ProfileScreening
{
    internal class ProfileScreeningJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IBusinessProfileRepository _businessProfileRepository;
        private IWatchlistNotificationExternalService _watchlistNotificationExternalService;
        private readonly ILogger<ProfileScreeningJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncRetryPolicy _databaseRetryPolicy;

        public ProfileScreeningJob(IServiceScopeFactory scopeFactory, ILogger<ProfileScreeningJob> logger, 
            IConfiguration configuration, AsyncRetryPolicy databaseRetryPolicy)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
            _databaseRetryPolicy = databaseRetryPolicy;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Create stopwatch to log elapsed time
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTimeOffset.UtcNow;

            try
            {
                // Create a new scope to get required services
                using var scope = _scopeFactory.CreateScope();
                _businessProfileRepository = scope.ServiceProvider.GetRequiredService<IBusinessProfileRepository>();
                _watchlistNotificationExternalService = scope.ServiceProvider.GetRequiredService<IWatchlistNotificationExternalService>();

                // We will only screen business profiles with their KYC submitted by checking KYCSubmissionStatusCode and BusinessKYCSubmissionStatusCode
                // equals to submitted (Id = 2).
                var businessProfiles = await GetSubmittedKycBusinessProfilesAsync();

                // Perform screening for each business profile.
                var screeningResult = await ScreeningAsync(businessProfiles);

                // Send email notifications no matter there are changes or not.
                var result = await _watchlistNotificationExternalService.SendAsync(
                        changeDTOs: screeningResult.ChangeDtos,
                        isSingleProfileScreening: false,
                        singlePartnerName: string.Empty);

                stopwatch.Stop();

                // Log Screening Summary
                LogScreeningSummary(startTime, businessProfiles.Count, stopwatch, screeningResult, result.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileScreeningJob: An error occurred during job execution. Error: {Error}", ex.ToString());
                throw;
            }
        }

        #region Private Helper Methods
        private async Task<IReadOnlyList<BusinessProfile>> GetSubmittedKycBusinessProfilesAsync()
        {
            var businessProfiles = await _businessProfileRepository.GetSubmittedKycBusinessProfilesAsync();
            return businessProfiles;
        }

        private async Task<ScreeningOutputDTO> ScreeningAsync(IReadOnlyList<BusinessProfile> businessProfiles)
        {
            var changeDtos = new ConcurrentBag<ChangeDTO>();
            var successBusinessProfileScreenCount = 0;
            var totalScreeningInputCount = 0;
            var failedNameScreeningInputs = new ConcurrentBag<FailedScreeningInput>();
            var failedGetMatchedEntityScreeningInputs = new ConcurrentBag<FailedScreeningInput>();
            var concurrencyLimit = _configuration.GetValue<int>("ProfileScreening:ConcurrencyLimit");

            // Limit concurrency to avoid overwhelming the database/service
            using var semaphore = new SemaphoreSlim(concurrencyLimit);
            var tasks = new List<Task>();

            foreach (var businessProfile in businessProfiles)
            {
                await semaphore.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var policyContext = new Context();
                        policyContext["BusinessProfileId"] = businessProfile.Id;

                        // Execute with retry policy
                        await _databaseRetryPolicy.ExecuteAsync(async (context) =>
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var complianceScreeningService = scope.ServiceProvider.GetRequiredService<ComplianceScreeningService>();
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            var screeningResult = await complianceScreeningService.ScreeningAsync(businessProfile.Id);

                            if (screeningResult.IsFailure)
                            {
                                _logger.LogWarning("ProfileScreeningJob: Screening failed for BusinessProfileId {BusinessProfileId}. Error: {Error}",
                                    businessProfile.Id, screeningResult.Error);
                                return;
                            }

                            await unitOfWork.CommitAsync();

                            if (screeningResult.Value.FailedNameScreeningInputs.Count == 0 && screeningResult.Value.FailedGetMatchedEntityScreeningInputs.Count == 0)
                            {
                                Interlocked.Increment(ref successBusinessProfileScreenCount);
                            }

                            Interlocked.Add(ref totalScreeningInputCount, screeningResult.Value.ScreeningInputCount);

                            foreach (var item in screeningResult.Value.ChangeDTOs) changeDtos.Add(item);
                            foreach (var item in screeningResult.Value.FailedNameScreeningInputs) failedNameScreeningInputs.Add(item);
                            foreach (var item in screeningResult.Value.FailedGetMatchedEntityScreeningInputs) failedGetMatchedEntityScreeningInputs.Add(item);

                        }, policyContext);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ProfileScreeningJob: Failed to screen BusinessProfileId {BusinessProfileId} after all retry attempts. Error: {Error}",
                            businessProfile.Id, ex.ToString());
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return new ScreeningOutputDTO() 
            {
                ChangeDtos = changeDtos.ToList(),
                SuccessBusinessProfileScreenCount = successBusinessProfileScreenCount,
                ScreeningInputCount = totalScreeningInputCount,
                FailedNameScreeningInputs = failedNameScreeningInputs.ToList(),
                FailedGetMatchedEntityScreeningInputs = failedGetMatchedEntityScreeningInputs.ToList()
            };
        }

        private void LogScreeningSummary(DateTimeOffset startTime, int businessProfileCount, Stopwatch stopwatch, ScreeningOutputDTO screeningResult, bool isNotificationSentSuccessfully)
        {
            var summary = new System.Text.StringBuilder();

            summary.AppendLine();
            summary.AppendLine("=== Profile Screening Job Summary ===");
            summary.AppendLine($"Job started at: {startTime.ToOffset(TimeSpan.FromHours(8))}");
            summary.AppendLine($"Total business profiles: {businessProfileCount}");
            summary.AppendLine($"Successfully screened BusinessProfile: {screeningResult.SuccessBusinessProfileScreenCount}");
            summary.AppendLine($"Total screening inputs: {screeningResult.ScreeningInputCount}");
            summary.AppendLine();
            
            // Failed Name Screening Inputs
            summary.AppendLine($"Failed name screening inputs: {screeningResult.FailedNameScreeningInputs.Count}");
            if (screeningResult.FailedNameScreeningInputs.Count > 0)
            {
                summary.AppendLine("Failed Name Screening Details:");
                summary.AppendLine(screeningResult.GetFailedNameScreeningInputsJson());
            }
            summary.AppendLine();
            
            // Failed Get Matched Entity Screening Inputs
            summary.AppendLine($"Failed get matched entity screening inputs: {screeningResult.FailedGetMatchedEntityScreeningInputs.Count}");
            if (screeningResult.FailedGetMatchedEntityScreeningInputs.Count > 0)
            {
                summary.AppendLine("Failed Get Matched Entity Screening Details:");
                summary.AppendLine(screeningResult.GetFailedGetMatchedEntityScreeningInputsJson());
            }
            summary.AppendLine();

            // Notification
            summary.AppendLine($"Number of change detected: {screeningResult.ChangeDtos.Count}");
            if (screeningResult.ChangeDtos.Count > 0)
            {
                summary.AppendLine($"Watchlist notification sent {(isNotificationSentSuccessfully ? "successfully" : "unsuccessfully")}.");
            }

            // Job Completion
            summary.AppendLine($"Average Screening Time required per BusinessProfile (seconds): {stopwatch.Elapsed.TotalSeconds / businessProfileCount:0.00}");
            summary.AppendLine($"Average Screening Time required per ScreeningInput (seconds): {stopwatch.Elapsed.TotalSeconds / screeningResult.ScreeningInputCount:0.000000}");
            summary.AppendLine($"Job completed in {stopwatch.Elapsed.TotalMinutes:0.00} minutes ({stopwatch.Elapsed.TotalSeconds:0.00} seconds)");
            summary.AppendLine("======================================");
            
            // Log as a single entry - use LogWarning if there are failures, otherwise LogInformation
            if (screeningResult.FailedNameScreeningInputs.Count > 0 || 
                screeningResult.FailedGetMatchedEntityScreeningInputs.Count > 0 ||
                (screeningResult.ChangeDtos.Count > 0 && !isNotificationSentSuccessfully))
            {
                _logger.LogWarning(summary.ToString());
            }
            else
            {
                _logger.LogInformation(summary.ToString());
            }
        }
        #endregion Private Helper Methods
    }
}
