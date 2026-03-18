using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Command
{
	//[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.Edit)]
    internal class UpdatePartnerApiSettingsConfirmWhitelistCommand : BaseCommand<Result>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public PartnerAPISettingsConfirmWhitelistUpdateInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Updated parter API settings confirmation for PartnerSubscriptionCode: [{this.PartnerSubscriptionCode}]";
                return Task.FromResult(_description);
            }
            return Task.FromResult<string>(null);
        }
    }
    internal class UpdatePartnerApiSettingsConfirmWhitelistCommandHandler : IRequestHandler<UpdatePartnerApiSettingsConfirmWhitelistCommand, Result>
    {
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<UpdatePartnerApiSettingsConfirmWhitelistCommandHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public UpdatePartnerApiSettingsConfirmWhitelistCommandHandler(PartnerService partnerService,
                                                                        IPartnerRepository partnerRepository,
                                                                        ILogger<UpdatePartnerApiSettingsConfirmWhitelistCommandHandler> logger,
                                                                        INotificationService notificationService,
                                                                        BusinessProfileService businessProfileService,
                                                                        IConfiguration config,
                                                                        IWebHostEnvironment environment)
        {
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
            _logger = logger;
            _notificationService = notificationService;
            _businessProfileService = businessProfileService;
            _config = config;
            _environment = environment;
        }

        public async Task<Result> Handle(UpdatePartnerApiSettingsConfirmWhitelistCommand request, CancellationToken cancellationToken)
        {
            var partnerApiSetting = await _partnerService.GetPartnerAPISettingByCodeAsync(request.InputDTO.PartnerApiSettingId);
            
            if ((partnerApiSetting == null && request.InputDTO.IsPartnerConfirmWhitelisted == true) || (partnerApiSetting != null && request.InputDTO.IsPartnerConfirmWhitelisted == true && partnerApiSetting.IsPartnerConfirmWhitelisted == false)) {
                var update = await UpdatePartnerConfirmation(request);
                if (update.IsSuccess)
                {
                    var sendEmail = await SendEmail(request);
                    if (sendEmail.IsFailure)
                    {
                        return Result.Failure($"Failed to send email notification");
                    }
                }
            }
            return Result.Success();

        }

        public async Task<Result> UpdatePartnerConfirmation(UpdatePartnerApiSettingsConfirmWhitelistCommand request)
        {
            try
            {
                var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
                var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
                var partnerAPISettingList = _partnerService.GetPartnerAPISettingByPartnerSubscriptionCodeAsync(request.PartnerSubscriptionCode).Result;
                foreach (var item in partnerAPISettingList)
                {
                    item.IsPartnerConfirmWhitelisted = request.InputDTO.IsPartnerConfirmWhitelisted;
                    await _partnerService.UpdatePartnerAPISettingAsync(item);
                }

                return Result.Success(request.InputDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdatePartnerApiSettingsConfirmWhitelistCommand] {ex.Message}");
            }
            return Result.Failure<PartnerAPISettingsConfirmWhitelistUpdateInputDTO>(
                            $"Update partner api settings confirmation failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
                        );
        }

        private async Task<Result> SendEmail(UpdatePartnerApiSettingsConfirmWhitelistCommand request)
        {
            var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            string partnerAPIURL = $"{_config.GetValue<string>("IntranetUri")}/dashboard/api/manage-partner-api";

            //notificationTemplateCode for WhitelistIP
            long templateCode = 22;

            //RecipientTypeCode for recipient, cc, bcc
            long recipientTypeCode = 1;
            long ccTypeCode = 2;
            long bccTypeCode = 3;

            var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, templateCode);
            var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, templateCode);
            var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, templateCode);

            var recipients = new List<RecipientsInputDTO>();
            foreach (var emailist in recipientInfo)
            {
                var recipientlist = new RecipientsInputDTO()
                {
                    email = emailist.Email,
                    name = emailist.Name
                };
                recipients.Add(recipientlist);
            }

            var cc = new List<RecipientsInputDTO>();
            foreach (var emailist in ccInfo)
            {
                var cclist = new RecipientsInputDTO()
                {
                    email = emailist.Email,
                    name = emailist.Name
                };
                cc.Add(cclist);
            }

            var bcc = new List<RecipientsInputDTO>();
            foreach (var emailist in bccInfo)
            {
                var bcclist = new RecipientsInputDTO()
                {
                    email = emailist.Email,
                    name = emailist.Name
                };
                bcc.Add(bcclist);
            }

            var file = new List<IFormFile> { };

            StringBuilder _xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(_xml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PartnerConfirmationTemplate");
                writer.WriteElementString("Recipient", "SD team");
                writer.WriteElementString("PartnerName", businessProfile.Value.CompanyName);
                writer.WriteElementString("PartnerAPIUrl", partnerAPIURL);
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            var template = "PartnerConfirmationTemplate";
            string content = generator.GenerateContent(_xml.ToString(), template, cultureName);

            string body = content;


            Result<HttpStatusCode> sendResubmissionEmail = await _notificationService.SendNotification
            (
                recipients,
                bcc,
                cc,
                file,
                "Tranglo IP Whitelist Confirmation",
                body,
                NotificationTypes.Email
            );

            return Result.Success();
        }
    }
}
