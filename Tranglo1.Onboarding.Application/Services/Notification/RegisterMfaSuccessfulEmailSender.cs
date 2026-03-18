using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;

namespace Tranglo1.Onboarding.Application.Services.Notification
{
    public sealed class RegisterMfaSuccessfulEmailSender
    {
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RegisterMfaSuccessfulEmailSender> _logger;

        public RegisterMfaSuccessfulEmailSender(TrangloUserManager userManager,
            IBusinessProfileRepository businessProfileRepository,
            INotificationService notificationService,
            ILogger<RegisterMfaSuccessfulEmailSender> logger)
        {
            _userManager = userManager;
            _businessProfileRepository = businessProfileRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result> NotifyUserAsync(ApplicationUser user)
        {
            #region Send Register 2FA Successful Email for Customer
            if (user is CustomerUser customerUser)
            {
                var partnerSubscriptions = await _userManager.GetPartnerSubscriptionsForUserAsync(customerUser);
                var partnerSubscriptionsGroupedBySolutionCode = partnerSubscriptions
                    .GroupBy(x => x.SolutionCode)
                    .Select(g => new
                    {
                        SolutionCode = g.Key,
                        PartnerSubscriptions = g.ToList()
                    })
                    .ToList();

                foreach (var solutionGroup in partnerSubscriptionsGroupedBySolutionCode)
                {
                    string solutionName = String.Empty;
                    if (solutionGroup.SolutionCode.HasValue)
                    {
                        var solution = Enumeration.FindById<Solution>(solutionGroup.SolutionCode.Value);
                        solutionName = solution.Name;
                    }

                    string entityName = String.Empty;
                    var entity = solutionGroup.PartnerSubscriptions
                        .Where(x => !String.IsNullOrEmpty(x.TrangloEntity))
                        .OrderBy(x => x.Id)
                        .FirstOrDefault()?.TrangloEntity;
                    if (entity != null)
                    {
                        entityName = TrangloEntity.GetByEntityByTrangloId(entity)?.Name;
                    }

                    var sendEmailResult = await SendEmailAsync(user, solutionName, entityName);
                    if (sendEmailResult.IsFailure)
                    {
                        return Result.Failure("Register two-factor authentication (2FA) is completed. " +
                            "Fail to send the successful two-factor authentication (2FA) registration email. Please contact the support.");
                    }
                }
            }
            #endregion
            #region Send Register 2FA Successful Email for Non-Customer
            else
            {
                Result sendEmailResult = await SendEmailAsync(user, null, null);
                if (sendEmailResult.IsFailure)
                {
                    return Result.Failure("Register two-factor authentication (2FA) is completed. " +
                        "Fail to send the successful two-factor authentication (2FA) registration email. Please contact the support.");
                }
            }
            #endregion

            return Result.Success();
        }

        private async Task<Result> SendEmailAsync(ApplicationUser user,
            string solutionName,
            string entityName)
        {
            NotificationTemplate notificationTemplate = NotificationTemplate.Register2faSuccessful;
            string subject = "Successful two-factor authentication (2FA) enabled";
            if (!String.IsNullOrEmpty(solutionName))
                subject = $"({solutionName}) {subject}";

            List<EmailRecipient> bccList = await _businessProfileRepository.GetRecipientEmail(RecipientType.BCC.Id, notificationTemplate.Id);
            List<EmailRecipient> ccList = await _businessProfileRepository.GetRecipientEmail(RecipientType.CC.Id, notificationTemplate.Id);

            EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO
            {
                recipients = new List<RecipientsInputDTO>
                {
                    new RecipientsInputDTO
                    {
                        email = user.Email.Value,
                        name = user.FullName.Value
                    }
                },
                cc = ccList.Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                    .ToList(),
                bcc = bccList.Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                    .ToList(),
                subject = subject,
                Module = "CustomerIdentity",
                SubModule = "Successfully Register 2FA",
                NotificationType = NotificationTypes.Email,
                NotificationTemplate = notificationTemplate.Name + "Template",
                SolutionName = solutionName,
                RecipientName = user.FullName.Value,
                Entity = entityName
            };

            var sendEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);
            if (sendEmailResponse.IsFailure)
            {
                _logger.LogError("Sending Email for two-factor authentication (2FA) registration is FAILED! [{Error}]", sendEmailResponse.Error);
                return Result.Failure("Fail to send the two-factor authentication (2FA) registration email. Please contact the support.");
            }

            return Result.Success();
        }
    }
}
