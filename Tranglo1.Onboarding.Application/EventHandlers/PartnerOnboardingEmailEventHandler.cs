using CSharpFunctionalExtensions;
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
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.EventHandlers;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.Services.Notification;

namespace Tranglo1.Onboarding.Application.EventHandlers
{
    class PartnerOnboardingEmailEventHandler : BaseEventHandler<PartnerOnboardingEmailEvent>
    {
        private readonly ILogger<PartnerOnboardingEmailEventHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IWebHostEnvironment _environment;


        public PartnerOnboardingEmailEventHandler(
            ILogger<PartnerOnboardingEmailEventHandler> logger,
            INotificationService notificationService,
            IConfiguration config,
            PartnerService partnerService,
            BusinessProfileService businessProfileService,
            IPartnerRepository partnerRepository,
            IWebHostEnvironment environment)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _notificationService = notificationService;
            _config = config;
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
            _environment = environment;
        }


        protected override async Task HandleAsync(PartnerOnboardingEmailEvent @event, CancellationToken cancellationToken)
        {
            var connectPortal = _config.GetValue<string>("ConnectPortalUri");
            var currentYear = DateTime.Now.Year.ToString();

            var domainEvent = @event;


            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            //notificationTemplateCode for PartnerOnboarding
            long partnerOnboardingTemplateCode = 16;

            //RecipientTypeCode for recipient, cc, bcc
            long recipientTypeCode = 1;
            long ccTypeCode = 2;
            long bccTypeCode = 3;

            var partnerDetails = await _partnerRepository.GetPartnerDetailsByCodeAsync(@event.PartnerCode);
            var partnerSubs = await _partnerRepository.GetSubscriptionAsync(@event.PartnerSubsciptionCode);
            var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubs.TrangloEntity);
            var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(partnerDetails.BusinessProfileCode);

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
                email = partnerDetails.Email.Value,
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

            var file = new List<IFormFile> { };

            StringBuilder _xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(_xml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PartnerOnboarding");                 // expected to match: <xsl:template match="Submission">                    
                writer.WriteElementString("PartnerName", businessProfile.CompanyName);  // TODO: insert data for partner user name
                writer.WriteElementString("LoginUrl", connectPortal);     // TODO: insert data for the url endpoint
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                writer.WriteElementString("EntityName", trangloEntity.Name.ToString());
                writer.WriteElementString("SolutionName", partnerSubs.Solution.Name.ToString());
                writer.WriteElementString("PartnerType", partnerSubs.PartnerType.Name.ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            string content = generator.GenerateContent(_xml.ToString(), "PartnerOnboardingTemplate", cultureName);
            string body = content;

            Result<HttpStatusCode> sendPartnerOnboardingEmail = await _notificationService.SendNotification
            (
                recipients,
                bcc,
                cc,
                file,
                $"[{businessProfile.CompanyName}]",
                body,
                NotificationTypes.Email
            );

            if (sendPartnerOnboardingEmail.IsFailure)
            {
                _logger.LogError("SendNotification", $"Account Status Change email notification failed for {businessProfile.CompanyName} . {sendPartnerOnboardingEmail.Error}.");
            }

            _logger.LogInformation($"Account Status is active for[{businessProfile.CompanyName}]");

        }



    }

}

