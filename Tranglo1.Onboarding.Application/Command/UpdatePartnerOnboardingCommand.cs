using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerOnboarding;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerOnboardProgress, UACAction.Edit)]
    [Permission(Permission.ManagePartnerOnboardProgress.Action_Update_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerOnboardProgress.Action_View_Code })]
    internal class UpdatePartnerOnboardingCommand : BaseCommand<Result>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public string UserBearerToken { get; set; }
		public PartnerOnboardingInputDTO UpdatePartnerOnboarding { get; set; }
        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Updated workflow status";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }  
    }

    internal class UpdatePartnerOnboardingCommandHandler : IRequestHandler<UpdatePartnerOnboardingCommand, Result>
    {
        private readonly ILogger<UpdatePartnerOnboardingCommandHandler> _logger;
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IWebHostEnvironment _environment;

        public UpdatePartnerOnboardingCommandHandler(ILogger<UpdatePartnerOnboardingCommandHandler> logger,
            PartnerService partnerService, IPartnerRepository partnerRepository, INotificationService notificationService,
            IConfiguration config, BusinessProfileService businessProfileService, IWebHostEnvironment environment)
        {
            _partnerService = partnerService;
            _logger = logger;
            _partnerRepository = partnerRepository;
            _notificationService = notificationService;
            _config = config;
            _businessProfileService = businessProfileService;
            _environment = environment;
        }

        public async Task<Result> Handle(UpdatePartnerOnboardingCommand request, CancellationToken cancellationToken)
        {
            var partner = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
            var inputPartnerRegistration = request.UpdatePartnerOnboarding;

            partner.AgreementOnboardWorkflowStatusCode = inputPartnerRegistration.AgreementWorkflowCode;
            partnerSubscription.APIIntegrationOnboardWorkflowStatusCode = inputPartnerRegistration.APIIntegrationWorkflowCode;
            //partnerBusiness.APIIntegrationOnboardWorkflowStatusCode = inputPartnerRegistration.APIIntegrationWorkflowCode;

            //var partnerOnboarding = await _partnerService.IsPartnerReadyGoLive(request.PartnerCode, request.PartnerSubscriptionCode, request.UserBearerToken);
            var updatePartner = await _partnerService.UpdatePartnerRegistrationAsync(partner);
            var updatePartnerSubscription = await _partnerRepository.UpdateSubcriptionAsync(partnerSubscription);

            if (updatePartner.IsFailure && updatePartnerSubscription.IsFailure)
            {
                return Result.Failure($"Failed to update Partner Onboarding Status " +
                    $"for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}");
            }

            if (inputPartnerRegistration.AgreementWorkflowCode == 3 && inputPartnerRegistration.APIIntegrationWorkflowCode == 3)
            {
                var isReadyGoLive = await _partnerService.IsPartnerReadyGoLive(request.PartnerCode, request.PartnerSubscriptionCode, false); //check
                if (isReadyGoLive)
                {
                    var sendNotification = await SendEmail(request.PartnerCode, request.PartnerSubscriptionCode);
                    if (sendNotification.IsFailure)
                    {
                        return Result.Failure<UpdatePartnerOnboardingCommand>($"Failed to send notification for Partner Onboarding subscription(s). PartnerSubscriptionCode: {request.PartnerSubscriptionCode}");
                    }
                }
            }

            return Result.Success();
        }

        public async Task<Result> SendEmail(long partnerCode, long partnerSubscriptionCode)
        {
            var connectPortal = _config.GetValue<string>("ConnectPortalUri");
            var currentYear = DateTime.Now.Year.ToString();


            var partnerSubscription = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerCode);
            var partnerRegistration = await _partnerRepository.GetPartnerDetailsByCodeAsync(partnerCode);
            var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerRegistration.TrangloEntity);
            var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);

            bool isTrangloConnect = partnerSubscription.Any(x => x.Solution == Solution.Connect);
            bool isTrangloBusiness = partnerSubscription.Any(x => x.Solution == Solution.Business);
            string solution = string.Empty;

            if (isTrangloBusiness && isTrangloConnect)
            {
                solution = "Tranglo Connect + Tranglo Business";
            }
            else if (isTrangloConnect)
            {
                solution = "Tranglo Connect";
            }
            else if (isTrangloBusiness)
            {
                solution = "Tranglo Business";
            }

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            //var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            //string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            //var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            //notificationTemplateCode for PartnerOnboarding
            long partnerOnboardingTemplateCode = 16;

            //RecipientTypeCode for recipient, cc, bcc
            long recipientTypeCode = 1;
            long ccTypeCode = 2;
            long bccTypeCode = 3;

            //var partnerDetails = await _partnerRepository.GetPartnerDetailsByCodeAsync(partnerCode);
            //var partnerSubs = 

            BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();


            var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, partnerOnboardingTemplateCode);
            var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, partnerOnboardingTemplateCode);
            var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, partnerOnboardingTemplateCode);

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

            var recipientlistPartner = new RecipientsInputDTO()
            {
                email = partnerRegistration.Email.Value,
                name = businessProfile.CompanyName
            };
            recipients.Add(recipientlistPartner);

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


            try
            {
                EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                emailNotificationInputDTO.recipients = recipients;
                emailNotificationInputDTO.cc = cc;
                emailNotificationInputDTO.bcc = bcc;
                emailNotificationInputDTO.Url = connectPortal;
                emailNotificationInputDTO.RecipientName = businessProfile.CompanyName;
                emailNotificationInputDTO.NotificationTemplate = "BusinessAccountGoLiveTemplate";
                emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                emailNotificationInputDTO.subject = $"({solution}) Your account is ready - {businessProfile.CompanyName}";

                var sendInviteUserNewUserEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);

                if (sendInviteUserNewUserEmailResponse.IsFailure)
                {
                    _logger.LogError("SendNotification", $"[UpdatePartnerOnboardingCommand] Invite New User Notification failed for {businessProfile.CompanyName}. {sendInviteUserNewUserEmailResponse.Error}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdatePartnerOnboardingCommand] {ex.ToString()}");
            }

            //var file = new List<IFormFile> { };

            //StringBuilder _xml = new StringBuilder();
            //using (XmlWriter writer = XmlWriter.Create(_xml))
            //{
            //    writer.WriteStartDocument();
            //    writer.WriteStartElement("PartnerOnboarding");                 // expected to match: <xsl:template match="Submission">                    
            //    writer.WriteElementString("PartnerName", businessProfile.CompanyName);  // TODO: insert data for partner user name
            //    writer.WriteElementString("LoginUrl", connectPortal);     // TODO: insert data for the url endpoint
            //    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
            //    writer.WriteElementString("EntityName", trangloEntity.Name.ToString());
            //    writer.WriteElementString("SolutionName", partnerSubs.Solution.Name.ToString());
            //    writer.WriteElementString("PartnerType", partnerSubs.PartnerType.Name.ToString());
            //    writer.WriteEndElement();
            //    writer.WriteEndDocument();
            //}

            //string content = generator.GenerateContent(_xml.ToString(), "PartnerOnboardingTemplate", cultureName);
            //string body = content;

            //Result<HttpStatusCode> sendPartnerOnboardingEmail = await _notificationService.SendNotification
            //(
            //    recipients,
            //    bcc,
            //    cc,
            //    file,
            //    $"[{businessProfile.CompanyName}]",
            //    body,
            //    NotificationTypes.Email
            //);

            //if (sendPartnerOnboardingEmail.IsFailure)
            //{
            //    _logger.LogError("SendNotification", $"Account Status Change email notification failed for {businessProfile.CompanyName} . {sendPartnerOnboardingEmail.Error}.");
            //}

            //_logger.LogInformation($"Account Status is active for[{businessProfile.CompanyName}]");

            return Result.Success();
        }
    }
}
