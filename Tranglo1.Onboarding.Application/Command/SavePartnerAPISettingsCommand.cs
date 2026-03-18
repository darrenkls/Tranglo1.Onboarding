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
    [Permission(Permission.APISettings.Action_Action_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.APISettings.Action_View_Code })]
    internal class SavePartnerAPISettingsCommand : BaseCommand<Result>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public PartnerAPISettings Staging { get; set; }
        public PartnerAPISettings Production { get; set; }
        public PartnerAPISettingsInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Added API Settings for PartnerSubscriptionCode: [{this.PartnerSubscriptionCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePartnerAPISettingsCommandHandler : IRequestHandler<SavePartnerAPISettingsCommand, Result>
    {
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IntegrationManager _integrationManager;
        private readonly ILogger<SavePartnerAPISettingsCommandHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly BusinessProfileService _businessProfileService;
        private static IConfiguration _config;
        private static string strCryptoKey;
        private static byte[] IV;
        private readonly IWebHostEnvironment _environment;

        public SavePartnerAPISettingsCommandHandler(
            IntegrationManager integrationManager,
            PartnerService partnerService,
            IPartnerRepository partnerRepository,
            ILogger<SavePartnerAPISettingsCommandHandler> logger,
            INotificationService notificationService,
            BusinessProfileService businessProfileService,
            IConfiguration config,
            IWebHostEnvironment environment)
        {
            _integrationManager = integrationManager;
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
            _logger = logger;
            _notificationService = notificationService;
            _config = config;
            _businessProfileService = businessProfileService;
            _environment = environment;
        }
        public async Task<Result> Handle(SavePartnerAPISettingsCommand request, CancellationToken cancellationToken)
        {
            var save = await SavePartnerAPISettings(request);
            var apiUserId = await _partnerRepository.GetPartnerAPISettingByAPIUserIdAsync(request.PartnerCode, request.Staging.APIUserId);
            var secretKey = await _partnerRepository.GetPartnerAPISettingBySecretKeyAsync(request.PartnerCode, request.Staging.SecretKey);
            try
            {
                if (save.IsSuccess)
                {
                    //remove checking for production first since is hidden in portal
                    //if (request.Staging.IPAddress.Count > 0 || request.Production.IPAddress.Count > 0)
                    if (request.Staging.IPAddress.Count > 0)
                    {
                        var sendWhitelistEmail = await SendWhitelistIPRequestEmail(request);
                        if (sendWhitelistEmail.IsFailure)
                        {
                            return Result.Failure($"Failed to send email notification");
                        }
                    }
                    //remove checking for production first since is hidden in portal
                    //if (!string.IsNullOrEmpty(request.Staging.APIStatusCallbackURL.ToString()) || !string.IsNullOrEmpty(request.Production.APIStatusCallbackURL.ToString()))
                    if (!string.IsNullOrEmpty(request.Staging.APIStatusCallbackURL))
                    {
                        var sendCallbackURLEmail = await SendCallbackURLRequestEmail(request);
                        if (sendCallbackURLEmail.IsFailure)
                        {
                            return Result.Failure($"Failed to send email notification");
                        }
                    }

                    return Result.Success();
                }
                else if (apiUserId.Count > 0 && secretKey.Count > 0) { return Result.Failure($"This API User ID  and Secret Key already exists"); }
                else if (apiUserId.Count > 0) { return Result.Failure($"This API User ID already exists"); }
                else if (secretKey.Count > 0) { return Result.Failure($"This Secret Key already exists"); }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SavePartnerAPISettingsCommand] {ex.Message}");
            }
            return Result.Failure<PartnerAPISettingsInputDTO>(
                            $"Add partner settings failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
                        );
        }

        public async Task<Result<PartnerAPISettingsInputDTO>> SavePartnerAPISettings(SavePartnerAPISettingsCommand request)
        {
            var apiUserId = await _partnerRepository.GetPartnerAPISettingByAPIUserIdAsync(request.PartnerCode,request.Staging.APIUserId);
            var secretKey = await _partnerRepository.GetPartnerAPISettingBySecretKeyAsync(request.PartnerCode,request.Staging.SecretKey);
            try
            {
                if (apiUserId.Count == 0 && secretKey.Count == 0)
                {
                    var password = Encrypt(request.Staging.Password);
                    PartnerAPISetting stgPartnerAPISetting = new PartnerAPISetting()
                    {
                        PartnerCode = request.PartnerCode,
                        PartnerSubscriptionCode = request.PartnerSubscriptionCode,
                        APIUserId = request.Staging.APIUserId,
                        Password = password,
                        SecretKey = request.Staging.SecretKey,
                        APIStatusCallbackURL = request.Staging.APIStatusCallbackURL,
                        EnvironmentCode = (int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id,
                        IsConfigured = false,
                        IsPartnerConfirmWhitelisted = request.InputDTO.IsPartnerConfirmWhitelisted,
                        IsSOAP = request.Staging.IsSOAP,
                        IsREST = request.Staging.IsREST
                    };

                    var result = await _partnerService.AddPartnerAPISettingAsync(stgPartnerAPISetting);

                    var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
                    var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
                    var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubscription.TrangloEntity);

                    if (partnerSubscription.RspStagingId != null)
                    {
                        await _integrationManager.AddApiSettingsAsync(
                            partnerSubscription.RspStagingId,
                            request.Staging.APIUserId,
                            password,
                            request.Staging.SecretKey,
                            partnerRegistration.Email,
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
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SavePartnerAPISettingsCommand] {ex.Message}");
            }
            return Result.Failure<PartnerAPISettingsInputDTO>(
                            $"Add partner settings failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
                        );
        }

        private async Task<Result> SendWhitelistIPRequestEmail(SavePartnerAPISettingsCommand request)
        {
            var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);
            var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
            var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubscription.TrangloEntity);
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
            var stringDomain = String.Empty;
            var stringDomain2 = String.Empty;
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

            if (request.Staging.IsREST == true && request.Staging.IsSOAP == false)
            {
                var apiUrl = await _partnerService.GetApiUrlAsync((int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id, Enumeration.FindById<APIType>(1));
                stringDomain = apiUrl.StringDomain;
            }
            if (request.Staging.IsREST == false && request.Staging.IsSOAP == true)
            {
                var apiUrl = await _partnerService.GetApiUrlAsync((int)Tranglo1.Onboarding.Domain.Entities.Environment.Staging.Id, Enumeration.FindById<APIType>(2));
                stringDomain = apiUrl.StringDomain;
            }
            if (request.Staging.IsREST == true && request.Staging.IsSOAP == true)
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
                //foreach (var recipient in recipients)
                //{
                //    writer.WriteElementString("Recipient", recipient.name);
                //}
                writer.WriteElementString("Recipient", recipients.FirstOrDefault().name);
                writer.WriteElementString("PartnerName", businessProfile.Value.CompanyName);
                writer.WriteElementString("PartnerEntity", trangloEntity.Name.ToString());
                writer.WriteElementString("SolutionName", "Connect");
                writer.WriteElementString("PreferredApi", stringDomain);
                writer.WriteElementString("PreferredApi2", stringDomain2);
                writer.WriteElementString("PartnerAPIUrl", partnerAPIURL);
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                writer.WriteElementString("SolutionName", partnerSubscription.Solution.Name.ToString());
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

        private async Task<Result> SendCallbackURLRequestEmail(SavePartnerAPISettingsCommand request)
        {
            //var partnerAPISettings = await _partnerService.GetPartnerAPISettingByPartnerCodeAsync(request.PartnerCode);
            var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);
            var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
            var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubscription.TrangloEntity);

            var stagingCallbackURL = string.Empty;
            var productionCallbackURL = string.Empty;
            if (!string.IsNullOrEmpty(request.InputDTO.Staging.APIStatusCallbackURL))
            {
                stagingCallbackURL = request.InputDTO.Staging.APIStatusCallbackURL;
            }

            if (!string.IsNullOrEmpty(request.InputDTO.Production.APIStatusCallbackURL))
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
                        writer.WriteElementString("EntityName", trangloEntity.Name.ToString());
                        writer.WriteElementString("SolutionName", partnerSubscription.Solution.Name.ToString());
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

        private static string GetHash(string plainText)
        {
            byte[] plainBytes = null;
            System.Security.Cryptography.MD5CryptoServiceProvider hashEngine = null;
            byte[] hashBytes = null;
            string hashText = null;

            plainBytes = Encoding.UTF8.GetBytes(plainText);

            hashEngine = new System.Security.Cryptography.MD5CryptoServiceProvider();
            hashBytes = hashEngine.ComputeHash(plainBytes);
            hashText = BitConverter.ToString(hashBytes).Replace("-", "");
            return hashText;
        }

        public static string Encrypt(string plainText)
        {
            strCryptoKey = _config.GetValue<string>("CryptoKey");
            IV = new byte[] { 50, 199, 10, 159, 132, 55, 236, 189, 51, 243, 244, 91, 17, 136, 39, 230 };

            string workText = plainText.Replace(Convert.ToChar(0x0).ToString(), "");

            byte[] strBytes = Encoding.UTF8.GetBytes(workText);
            byte[] strKeyBytes = Encoding.UTF8.GetBytes(GetHash(strCryptoKey));

            System.Security.Cryptography.RijndaelManaged rijndael = new System.Security.Cryptography.RijndaelManaged();
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

            System.Security.Cryptography.ICryptoTransform cryptoTransform = null;

            cryptoTransform = rijndael.CreateEncryptor(strKeyBytes, IV);

            System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, cryptoTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            cryptoStream.Write(strBytes, 0, strBytes.Length);
            cryptoStream.FlushFinalBlock();

            string encrypted = Convert.ToBase64String(memoryStream.ToArray());

            memoryStream.Close();
            cryptoStream.Close();
            return encrypted;
        }
    }
}
