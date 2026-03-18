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
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.EventHandlers;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.Services.Notification;

namespace Tranglo1.Onboarding.Application.EventHandlers
{
    class ActiveInactivePartnerAccountStatusEventHandler : BaseEventHandler<ActiveInactivePartnerAccountStatusEvent>
    {
        private readonly ILogger<ActiveInactivePartnerAccountStatusEventHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IWebHostEnvironment _environment;

        public ActiveInactivePartnerAccountStatusEventHandler(
            ILogger<ActiveInactivePartnerAccountStatusEventHandler> logger,
            INotificationService notificationService,
            IConfiguration config,
            BusinessProfileService businessProfileService,
            IPartnerRepository partnerRepository,
            IWebHostEnvironment environment
            )
        {
            _logger = logger;
            _notificationService = notificationService;
            _config = config;
            _partnerRepository = partnerRepository;
            _businessProfileService = businessProfileService;
            _environment = environment;
        }

        protected override async Task HandleAsync(ActiveInactivePartnerAccountStatusEvent @event, CancellationToken cancellationToken)
        {
            var adminPortal = _config.GetValue<string>("IntranetUri");
            var domainEvent = @event;
            var currentYear = DateTime.Now.Year.ToString();

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            //notificationTemplateCode for active, inactive
            long activeTemplateCode = 14;
            long inactiveTemplateCode = 15;

            //RecipientTypeCode for recipient, cc, bcc
            long recipientTypeCode = 1;
            long ccTypeCode = 2;
            long bccTypeCode = 3;

            var partnerDetails = await _partnerRepository.GetPartnerDetailsByCodeAsync(@event.PartnerCode);
            var partnerSubs = await _partnerRepository.GetSubscriptionAsync(@event.PartnerSubsciptionCode);
            var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubs.TrangloEntity);
            var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(partnerDetails.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

            if (@event.PartnerAccountStatusType == PartnerAccountStatusType.Active)
            {
                var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, activeTemplateCode);
                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, activeTemplateCode);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, activeTemplateCode);

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
                    writer.WriteStartElement("Profile");                 // expected to match: <xsl:template match="Submission">                    
                    writer.WriteElementString("Name", businessProfile.CompanyName);  // TODO: insert data for company name
                    writer.WriteElementString("ChangeType", @event.ChangeType.Name);
                    writer.WriteElementString("Description", @event.Description);
                    writer.WriteElementString("LoginUrl", adminPortal);     // TODO: insert data for the url endpoint
                    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                    writer.WriteElementString("EntityName", trangloEntity?.Name.ToString());        // TBT-1950: For handle old partner record with not assigned to entity
                    writer.WriteElementString("SolutionName", partnerSubs.Solution?.Name.ToString());
                    writer.WriteElementString("PartnerType", partnerSubs.PartnerType?.Name.ToString());
                    writer.WriteElementString("Description", @event.Description);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                string content = generator.GenerateContent(_xml.ToString(), "ActivePartnerAccountStatusTemplate", cultureName);
                string body = content;

                Result<HttpStatusCode> sendActivePartnerStatusEmailResponse = await _notificationService.SendNotification
                (
                    recipients,
                    bcc,
                    cc,
                    file,
                    $"Account Status Change for {businessProfile.CompanyName}",
                    body,
                    NotificationTypes.Email
                );
                if (sendActivePartnerStatusEmailResponse.IsFailure)
                {
                    _logger.LogError("SendNotification", $"Account Status Change email notification failed for {businessProfile.CompanyName} . {sendActivePartnerStatusEmailResponse.Error}.");
                }

                _logger.LogInformation($"Account Status is active for[{businessProfile.CompanyName}]");
            }

            else if (@event.PartnerAccountStatusType == PartnerAccountStatusType.Inactive)
            {
                var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, inactiveTemplateCode);

                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, inactiveTemplateCode);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, inactiveTemplateCode);

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
                    writer.WriteStartElement("Profile");               // expected to match: <xsl:template match="Resubmission">                    
                    writer.WriteElementString("Name", businessProfile.CompanyName);  // TODO: insert data for company name
                    writer.WriteElementString("ChangeType", @event.ChangeType.Name);
                    writer.WriteElementString("Description", @event.Description);
                    writer.WriteElementString("LoginUrl", adminPortal);     // TODO: insert data for the url endpoint
                    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                    writer.WriteElementString("EntityName", trangloEntity?.Name.ToString());        // TBT-1950: For handle old partner record with not assigned to entity
                    writer.WriteElementString("SolutionName", partnerSubs.Solution?.Name.ToString());
                    writer.WriteElementString("PartnerType", partnerSubs.PartnerType?.Name.ToString());
                    writer.WriteEndElement();                               // end root element
                    writer.WriteEndDocument();
                }

                string content = generator.GenerateContent(_xml.ToString(), "InactivePartnerAccountStatusTemplate", cultureName);
                string body = content;

                Result<HttpStatusCode> sendInactivePartnerStatusEmailResponse = await _notificationService.SendNotification
                (
                    recipients,
                    bcc,
                    cc,
                    file,
                    $"Account Status Change for {businessProfile.CompanyName}",
                    body,
                    NotificationTypes.Email
                );
                if (sendInactivePartnerStatusEmailResponse.IsFailure)
                {
                    _logger.LogError("SendNotification", $"Account Status Change email notification failed for {businessProfile.CompanyName} . {sendInactivePartnerStatusEmailResponse.Error}.");
                }

                _logger.LogInformation($"Account Status is inactive for[{businessProfile.CompanyName}]");
            }
        }
    }
}
