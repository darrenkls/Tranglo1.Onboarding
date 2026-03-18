using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.KYCEmailReminderScheduler.Notification;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler
{
    public class ExpiredKycPartner
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public long BusinessProfileCode { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public int BusinessKYCSubmissionStatusCode { get; set; }
        public string TrangloEntity { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class PartnerAccountStatusInputDTO
    {
        [Required(ErrorMessage = "Status is required")]
        public long Status { get; set; }
        [Required(ErrorMessage = "Change Type is required")]
        public long ChangeType { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(150, ErrorMessage = "Maximum length for description is 150 characters")]
        public string Description { get; set; }
    }

    public class SpResult
    {
        public bool Success { get; set; }
        public int? ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
    }

    internal class RejectKycPartnerService
    {
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<RejectKycPartnerService> _logger;
        private readonly string _defaultConnectionString;

        public RejectKycPartnerService(NotificationService notificationService, IConfiguration config
            , IBusinessProfileRepository businessProfileRepository, IPartnerRepository partnerRepository
            , ILogger<RejectKycPartnerService> logger)
        {
            _notificationService = notificationService;
            _config = config;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
            _defaultConnectionString = _config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task RejectExpiredKYCPartners()
        {
            _logger.LogInformation("Reject Expired KYC Partner Process Started...");

            int[] solutionCodes = JsonSerializer.Deserialize<int[]>(_config["Solutions"]);
            int kycExpiryDays = _config.GetValue<int>("KYCExpiryDays");

            foreach (int solutionCode in solutionCodes)
            {
                var expiredPartners = await GetExpiredKYCPartnersAsync(solutionCode, kycExpiryDays);
                foreach (var partner in expiredPartners)
                {
                    Result rejectInactivePartnerResult = await RejectExpiredKYCPartnerAsync(partner.PartnerCode, partner.PartnerSubscriptionCode, kycExpiryDays);
                    if (rejectInactivePartnerResult.IsFailure)
                    {
                        continue;
                    }

                    await SendAutoRejectPartnerApplicationEmailAsync(partner.CompanyName, partner.Email, solutionCode, partner.TrangloEntity, kycExpiryDays);
                }
            }

            _logger.LogInformation("Reject Expired KYC Partner Process Completed...");
        }

        private async Task<IEnumerable<ExpiredKycPartner>> GetExpiredKYCPartnersAsync(int solutionCode, int kycExpiredDays)
        {
            using SqlConnection connection = new SqlConnection(_defaultConnectionString);
            await connection.OpenAsync();

            var reader = await connection.QueryMultipleAsync(
                "dbo.GetExpiredKYCPartners",
                new
                {
                    SolutionCode = solutionCode,
                    ExpiryDays = kycExpiredDays
                },
                commandType: CommandType.StoredProcedure
            );

            return await reader.ReadAsync<ExpiredKycPartner>();
        }

        private async Task<Result> RejectExpiredKYCPartnerAsync(long partnerCode, long partnerSubscriptionCode, int kycExpiryDays)
        {
            var inputPartnerAccountStatus = new PartnerAccountStatusInputDTO
            {
                ChangeType = ChangeType.TEST_SIGN_UP.Id,
                Description = $"System rejected due to KYC inactive for more than {kycExpiryDays} days",
                Status = PartnerAccountStatusType.Rejected.Id
            };

            var partner = await _partnerRepository.GetPartnerRegistrationByCodeAsync(partnerCode);
            var subscription = await _partnerRepository.GetSubscriptionAsync(partnerSubscriptionCode);

            #region Cannot set to Reject when Partner KYC Approval Status is Approved
            if (inputPartnerAccountStatus.Status == PartnerAccountStatusType.Rejected.Id)
            {
                int? partnerKYCApprovalStatus = await GetPartnerKYCApprovalStatusByBusinessProfile(partner.BusinessProfileCode);

                if (partnerKYCApprovalStatus.HasValue)
                {
                    ApprovalWorkflowEngine.Enum.ApprovalStatus partnerKYCApprovalStatusEnum = (ApprovalWorkflowEngine.Enum.ApprovalStatus)partnerKYCApprovalStatus.Value;

                    if (partnerKYCApprovalStatusEnum == ApprovalWorkflowEngine.Enum.ApprovalStatus.Approved)
                    {
                        return Result.Failure($"Unable to set Partner Account Status to {PartnerAccountStatusType.Rejected.Name} as Partner KYC Approval Status is {ApprovalWorkflowEngine.Enum.ApprovalStatus.Approved}");
                    }
                }
            }
            #endregion

            if (subscription == null)
            {
                return Result.Failure("Subscription does not exist");
            }

            using var connection = new SqlConnection(_defaultConnectionString);
            await connection.OpenAsync();

            // Perform insert Partner Account Status, update Partner Registration & Partner Subscription (LastModifiedDate, LastModifiedBy and LastModifiedUserType)
            SpResult result = await connection.QuerySingleAsync<SpResult>(
                "dbo.SavePartnerAccountStatus",
                new
                {
                    PartnerCode = partnerCode,
                    PartnerSubscriptionCode = partnerSubscriptionCode,
                    BusinessProfileCode = partner.BusinessProfileCode,
                    ChangeType = inputPartnerAccountStatus.ChangeType,
                    Description = inputPartnerAccountStatus.Description,
                    Status = inputPartnerAccountStatus.Status,
                    CreatedBy = -1,
                    CreatedByUserType = -1,
                    CreatedDate = DateTime.UtcNow
                },
                commandType: CommandType.StoredProcedure);

            if (!result.Success)
            {
                _logger.LogError("[{Function}] Failed to insert partner account status. " +
                    "PartnerCode: {PartnerCode} " +
                    "Partner Subscription Code: {PartnerSubscriptionCode} " +
                    "Error Number: {ErrorNumber} " +
                    "Error Message: {ErrorMessage}"
                    , nameof(RejectExpiredKYCPartnerAsync), partnerCode, partnerSubscriptionCode, result.ErrorNumber, result.ErrorMessage);

                return Result.Failure(
                    $"Update Partner Status failed for PartnerCode: {partnerCode}, " +
                    $"PartnerSubscriptionCode: {partnerSubscriptionCode}."
                );
            }

            return Result.Success();
        }

        private async Task<int?> GetPartnerKYCApprovalStatusByBusinessProfile(int businessProfileCode)
        {
            using var connection = new SqlConnection(_defaultConnectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<int?>(
                "dbo.GetPartnerKYCApprovalStatusByBusinessProfile",
                new
                {
                    BusinessProfileCode = businessProfileCode
                },
                commandType: CommandType.StoredProcedure);
        }

        private async Task<bool> SendAutoRejectPartnerApplicationEmailAsync(string partnerName,
            string partnerEmail,
            int solutionCode,
            string entity,
            int kycExpiryDays)
        {
            NotificationTemplate notificationTemplate = NotificationTemplate.AutoRejectPartnerApplication;

            // Get Recipients Email Address
            List<EmailRecipient> bccList = await _businessProfileRepository.GetRecipientEmail(RecipientType.BCC.Id, notificationTemplate.Id);
            List<EmailRecipient> ccList = await _businessProfileRepository.GetRecipientEmail(RecipientType.CC.Id, notificationTemplate.Id);

            var solution = Enumeration.FindById<Solution>(solutionCode);

            EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO
            {
                recipients = new List<RecipientsInputDTO>
                {
                    new RecipientsInputDTO
                    {
                        email = partnerEmail,
                        name = partnerName
                    }
                },
                cc = ccList.Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                    .ToList(),
                bcc = bccList.Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                    .ToList(),
                subject = $"({solution.Name}) – Application status: Rejected",
                Module = "CustomerIdentity",
                SubModule = "Auto Reject Partner Application",
                NotificationType = NotificationTypes.Email,
                NotificationTemplate = notificationTemplate.Name + "Template",
                SolutionName = solution.Name,
                CompanyName = partnerName,
                Entity = entity,
                AutoRejectPartnerApplicationExpiredDays = kycExpiryDays
            };

            var sendEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);
            if (sendEmailResponse.IsFailure)
            {
                _logger.LogInformation("Sending Email for Auto Reject Partner is FAILED! [{Error}]", sendEmailResponse.Error);
                return false;
            }

            return true;
        }
    }
}
