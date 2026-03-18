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
using Tranglo1.Onboarding.Application.Managers;
using Tranglo1.Onboarding.Domain.Common;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Command
{
	//[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.Edit)]
    internal class UpdatePartnerApiSettingsCommand : BaseCommand<Result>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public UpdatePartnerAPISettings Staging { get; set; }
        public UpdatePartnerAPISettings Production { get; set; }
        public PartnerAPISettingsUpdateInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Edited API settings";
                return Task.FromResult(_description);
            }
            return Task.FromResult<string>(null);
        }
    }
    internal class UpdatePartnerApiSettingsCommandHandler : IRequestHandler<UpdatePartnerApiSettingsCommand, Result>
    {
        private readonly IntegrationManager _integrationManager;
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<UpdatePartnerApiSettingsCommandHandler> _logger;
        private readonly BusinessProfileService _businessProfileService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public UpdatePartnerApiSettingsCommandHandler(
            IntegrationManager integrationManager,
            PartnerService partnerService,
            IPartnerRepository partnerRepository,
            ILogger<UpdatePartnerApiSettingsCommandHandler> logger,
            BusinessProfileService businessProfileService,
            INotificationService notificationService,
            IConfiguration config, IWebHostEnvironment environment)
        {
            _integrationManager = integrationManager;
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
            _logger = logger;
            _businessProfileService = businessProfileService;
            _notificationService = notificationService;
            _config = config;
            _environment = environment;
        }

        public async Task<Result> Handle(UpdatePartnerApiSettingsCommand request, CancellationToken cancellationToken)
        {
            var save = await UpdatePartnerAPISettings(request);
            if (save.IsSuccess)
            {
                if(request.Staging.IPAddress.Count > 0) { 
                    if (!string.IsNullOrEmpty(request.Staging.IPAddress.ToString()) || !string.IsNullOrEmpty(request.Production.IPAddress.ToString()))
                    {
                        var sendWhitelistEmail = await SendWhitelistIPRequestEmail(request);
                        if (sendWhitelistEmail.IsFailure)
                        {
                            return Result.Failure($"Failed to send email notification");
                        }
                    }
                }
                if (!string.IsNullOrEmpty(request.Staging.APIStatusCallbackURL) || !string.IsNullOrEmpty(request.Production.APIStatusCallbackURL))
                {
                    var sendCallbackURLEmail = await SendCallbackURLRequestEmail(request);
                    if (sendCallbackURLEmail.IsFailure)
                    {
                        return Result.Failure($"Failed to send email notification");
                    }
                }
            }
            return Result.Success();
        }

        public async Task<Result> UpdatePartnerAPISettings(UpdatePartnerApiSettingsCommand request)
        {
            try
            {
                var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
                var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
                PartnerAPISetting stgPartnerAPISetting = _partnerService.GetPartnerAPISettingByCodeAsync(request.Staging.PartnerAPISettingId).Result;
                stgPartnerAPISetting.PartnerCode = request.PartnerCode;
                stgPartnerAPISetting.PartnerSubscriptionCode = request.PartnerSubscriptionCode;
                stgPartnerAPISetting.APIStatusCallbackURL = request.Staging.APIStatusCallbackURL;
                stgPartnerAPISetting.IsREST = request.Staging.IsREST;
                stgPartnerAPISetting.IsSOAP = request.Staging.IsSOAP;

                var result = await _partnerService.UpdatePartnerAPISettingAsync(stgPartnerAPISetting);                

                if (partnerSubscription.RspStagingId != null)
                {
                    await _integrationManager.UpdateCallbackAsync(
                        partnerSubscription.RspStagingId,
                        request.Staging.APIStatusCallbackURL
                    );
                }
                
                var whitelistList = request.Staging.IPAddress;
                foreach (var whitelistItem in whitelistList)
                {
                    WhitelistIP ip = new WhitelistIP()
                    {
                        PartnerCode = request.PartnerCode,
                        PartnerSubscriptionCode = request.PartnerSubscriptionCode,
                        IPAddressStart = whitelistItem.IPAddressStart,
                        IPAddressEnd = whitelistItem.IPAddressEnd,
                        IsRangeIP = whitelistItem.IsRangeIPAddress,
                        IsWhitelisted = false,
                        Environment = (int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id,
                    };
                    await _partnerService.AddWhiteListAsync(ip);
                }
                return Result.Success(request.InputDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdatePartnerApiSettingsCommand] {ex.Message}");
            }
            return Result.Failure<PartnerAPISettingsUpdateInputDTO>(
                            $"Update partner settings failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode :{request.PartnerSubscriptionCode}."
                        );
        }

        private async Task<Result> SendWhitelistIPRequestEmail(UpdatePartnerApiSettingsCommand request)
        {
            var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
            var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            string partnerAPIURL = $"{_config.GetValue<string>("IntranetUri")}/dashboard/api/manage-partner-api";

            //notificationTemplateCode for WhitelistIP
            long templateCode = 20;

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

            PartnerAPISetting stagePartnerAPISetting = _partnerService.GetPartnerAPISettingByCodeAsync(request.Staging.PartnerAPISettingId).Result;
            var stringDomain = String.Empty;
            var stringDomain2 = String.Empty;

            if (stagePartnerAPISetting.IsREST == true && stagePartnerAPISetting.IsSOAP == false)
            {
                var apiUrl = await _partnerService.GetApiUrlAsync((int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id, Enumeration.FindById<APIType>(1));
                stringDomain = apiUrl.StringDomain;
            }
            if (stagePartnerAPISetting.IsREST == false && stagePartnerAPISetting.IsSOAP == true)
            {
                var apiUrl = await _partnerService.GetApiUrlAsync((int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id, Enumeration.FindById<APIType>(2));
                stringDomain = apiUrl.StringDomain;
            }
            if (stagePartnerAPISetting.IsREST == true && stagePartnerAPISetting.IsSOAP == true)
            {
                var apiUrl = await _partnerService.GetApiUrlAsync((int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id, Enumeration.FindById<APIType>(1));
                var apiUrl2 = await _partnerService.GetApiUrlAsync((int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id, Enumeration.FindById<APIType>(2));

                stringDomain = apiUrl.StringDomain;
                stringDomain2 = apiUrl2.StringDomain;
            }
            var file = new List<IFormFile> { };

            StringBuilder _xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(_xml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("IPAddressSubmittedTemplate");
                writer.WriteElementString("Recipient", recipients.FirstOrDefault().name);
                writer.WriteElementString("PartnerName", businessProfile.Value.CompanyName);
                writer.WriteElementString("PartnerEntity", partnerSubscription.TrangloEntity);
                writer.WriteElementString("SolutionName", "Connect");
                writer.WriteElementString("PreferredApi", stringDomain);
                writer.WriteElementString("PreferredApi2", stringDomain2);
                writer.WriteElementString("PartnerAPIUrl", partnerAPIURL);
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            
            var template = "IPAddressSubmittedTemplate";
            string content = generator.GenerateContent(_xml.ToString(), template, cultureName);
            string body = content;

            Result<HttpStatusCode> sendResubmissionEmail = await _notificationService.SendNotification
            (
                recipients,
                bcc,
                cc,
                file,
                "IP Address Whitelist Request",
                body,
                NotificationTypes.Email
            );

            return Result.Success();
        }

        private async Task<Result> SendCallbackURLRequestEmail(UpdatePartnerApiSettingsCommand request)
        {
            //var partnerAPISettings = await _partnerService.GetPartnerAPISettingByPartnerCodeAsync(request.PartnerCode);
            var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);
            var stagingCallbackURL = string.Empty;
            var productionCallbackURL = string.Empty;
            if (!string.IsNullOrEmpty(request.InputDTO.Staging.APIStatusCallbackURL.ToString()))
            {
                stagingCallbackURL = request.InputDTO.Staging.APIStatusCallbackURL;
            }

            if (!string.IsNullOrEmpty(request.InputDTO.Production.APIStatusCallbackURL.ToString()))
            {
                productionCallbackURL = request.InputDTO.Production.APIStatusCallbackURL;
            }

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            string partnerAPIURL = $"{_config.GetValue<string>("IntranetUri")}/dashboard/api/manage-partner-api";

            //notificationTemplateCode for CallbackURL
            long templateCode = 21;

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
                writer.WriteStartElement("CallbackURLSubmittedTemplate");
                writer.WriteElementString("Recipient", recipients.FirstOrDefault().name);
                writer.WriteElementString("PartnerName", businessProfile.Value.CompanyName);
                writer.WriteElementString("StagingCallbackURL", stagingCallbackURL);
                writer.WriteElementString("ProductionCallbackURL", productionCallbackURL);
                writer.WriteElementString("PartnerAPIUrl", partnerAPIURL);
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            var template = "CallbackURLSubmittedTemplate";
            string content = generator.GenerateContent(_xml.ToString(), template, cultureName);
            string body = content;

            Result<HttpStatusCode> sendResubmissionEmail = await _notificationService.SendNotification
            (
                recipients,
                bcc,
                cc,
                file,
                "Callback URL Configuration Request",
                body,
                NotificationTypes.Email
            );

            return Result.Success();
        }
    }
}
