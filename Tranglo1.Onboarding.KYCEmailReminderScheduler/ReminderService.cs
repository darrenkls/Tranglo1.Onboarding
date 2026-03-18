using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.KYCEmailReminderScheduler.Notification;
using static Dapper.SqlMapper;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler
{
    public class ImcompleteTBKYCPartner
    {
        public long PartnerCode { get; set; }
        public long BusinessProfileCode { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public long? BusinessKYCSubmissionStatusCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long PartnerSubscriptionCode { get; set; }
    }

    public class ExternalComplianceUser
    {
        public long BusinessProfileCode { get; set; }
        public string RoleCode { get; set; }
        public string ExternalUserRoleName { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class ReminderService
    {
        private NotificationService _notificationService;
        private IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public ReminderService(NotificationService notificationService, IConfiguration config, IHttpClientFactory httpClientFactory
            , IBusinessProfileRepository businessProfileRepository, IPartnerRepository partnerRepository)
        {
            _notificationService = notificationService;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task SendEmailReminder()
        {
            Log.Information("Incomplete KYC Email Reminder Process Started...");

            var connectionString = _config.GetConnectionString("DefaultConnection");
            int[] solutionCodes = JsonSerializer.Deserialize<int[]>(_config["Solutions"]);
            int kycReminderDays = _config.GetValue<int>("KYCReminderDays");
            int kycReminderIntervalInDays = _config.GetValue<int>("KYCReminderIntervalInDays");
            DateTime utcNow = DateTime.UtcNow.Date;

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                foreach (int solutionCode in solutionCodes)
                {
                    var impcompleteKYCPartnerSP = "dbo.GetIncompleteKYCPartners";

                    var reader = await conn.QueryMultipleAsync(
                        impcompleteKYCPartnerSP,
                        new
                        {
                            SolutionCode = solutionCode,
                            ReminderDays = kycReminderDays
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    var partners = await reader.ReadAsync<ImcompleteTBKYCPartner>();
                    partners = partners.Where(x => x.CreatedDate != null
                       && (int)((x.CreatedDate.Value.Date - utcNow).TotalDays % 3) == 0)
                        .ToList();

                    foreach (var partner in partners)
                    {
                        try
                        {
                            Log.Information($"Start Process Partner - ${JsonSerializer.Serialize(partner)}");

                            var toList = new List<RecipientsInputDTO>()
                            {
                                new RecipientsInputDTO()
                                {
                                    email = partner.Email,
                                    fullname = partner.CompanyName,
                                    name = partner.CompanyName
                                }
                            };

                            var externalComplianceEmailSP = "dbo.GetExternalComplianceEmailByBusinessProfileCode";

                            var complianceReader = await conn.QueryMultipleAsync(
                                externalComplianceEmailSP,
                                new
                                {
                                    BusinessProfileCode = partner.BusinessProfileCode,
                                    SolutionCode = Solution.Business.Id
                                },
                                commandType: CommandType.StoredProcedure
                                );

                            var complianceUsers = await complianceReader.ReadAsync<ExternalComplianceUser>();

                            var ccList = new List<RecipientsInputDTO>();

                            var emailRecipients = await _businessProfileRepository.GetRecipientEmail(RecipientType.CC.Id, NotificationTemplate.ImcompleteTBKYCReminder.Id);

                            emailRecipients.ForEach(o =>
                            {
                                Log.Information($"Processing Compliance User - ${JsonSerializer.Serialize(o)}");

                                if (!toList.Any(to => to.email == o.Email))
                                {
                                    ccList.Add(new RecipientsInputDTO()
                                    {
                                        email = o.Email,
                                        fullname = o.Name,
                                        name = o.Name
                                    });
                                }
                            });

                            complianceUsers.ToList().ForEach(o =>
                            {
                                Log.Information($"Processing Compliance User - ${JsonSerializer.Serialize(o)}");

                                if (!ccList.Any(cc => cc.email == o.Email) && !toList.Any(to => to.email == o.Email))
                                {
                                    ccList.Add(new RecipientsInputDTO()
                                    {
                                        email = o.Email,
                                        fullname = o.FullName,
                                        name = o.FullName
                                    });
                                }
                            });


                            var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partner.PartnerCode);
                            bool isTrangloConnect = partnerSubInfo.Any(x => x.Solution == Solution.Connect);
                            bool isTrangloBusiness = partnerSubInfo.Any(x => x.Solution == Solution.Business);

                            //string subject = "(Tranglo Business) Reminder: Complete your KYC Submission";

                            //Jaden: As this only get TB customer
                            string subject = string.Empty;
                            if (isTrangloConnect && isTrangloBusiness)
                            {
                                subject = "(Tranglo Connect + Tranglo Business) Reminder: Complete your KYC submission";
                            }
                            else if (isTrangloConnect)
                            {
                                subject = "(Tranglo Connect) Reminder: Complete your KYC submission";
                            }
                            else if (isTrangloBusiness)
                            {
                                subject = "(Tranglo Business) Reminder: Complete your KYC submission";
                            }

                            var kycReminderUrl = _config.GetValue<string>("IdentityServerUri");
                            var unsubscribeUrl = $"{kycReminderUrl}account/UnsubscribeKYCReminder/?partnerSubscriptionCode={partner.PartnerSubscriptionCode}";

                            EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                            emailNotificationInputDTO.recipients = toList;
                            emailNotificationInputDTO.cc = ccList;
                            emailNotificationInputDTO.bcc = null;
                            emailNotificationInputDTO.RecipientName = partner.CompanyName;
                            emailNotificationInputDTO.Url = kycReminderUrl;
                            emailNotificationInputDTO.NotificationTemplate = "TBPartnerKYCReminderTemplate";
                            emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                            emailNotificationInputDTO.subject = subject;
                            emailNotificationInputDTO.UnsubscribeUrl = unsubscribeUrl;
                            emailNotificationInputDTO.PartnerSubscriptionCode = partner.PartnerSubscriptionCode;

                            Log.Information($"Notification Request - ${JsonSerializer.Serialize(emailNotificationInputDTO)}");

                            var sendTBKYCReminderEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);

                            if (sendTBKYCReminderEmailResponse.IsFailure)
                            {
                                Log.Error("SendNotification", $"[ReminderService] TB KYC Email Reminder failed for Partner Code : {partner.PartnerCode} - {sendTBKYCReminderEmailResponse.Error}.");
                            }
                            else
                            {
                                Log.Information($"Partner Code : {partner.PartnerCode} - TB Incomplete KYC Reminder Email Sent.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Partner Code : {partner.PartnerCode} - {ex.ToString()}");
                        }
                    }
                }

                conn.Close();
            }

            Log.Information("Incomplete KYC Email Reminder Process Completed...");

        }
    }
}
