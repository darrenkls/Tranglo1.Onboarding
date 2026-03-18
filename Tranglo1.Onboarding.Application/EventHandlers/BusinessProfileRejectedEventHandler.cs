using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.EventHandlers;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Entities.Specifications.CustomerUserBusinessProfileRoles;
using Tranglo1.Onboarding.Domain.Entities.Specifications.CustomerUserBusinessProfiles;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Domain.Common;
using System.Linq;
using System;
using System.Xml;
using System.Text;
using Tranglo1.Onboarding.Application.Services.Notification;
using System.Net;
using CSharpFunctionalExtensions;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Tranglo1.Onboarding.Application.CustomerUserList.EventHandlers
{
	class BusinessProfileRejectedEventHandler : BaseEventHandler<BusinessProfileRejectedEvent> // INotificationHandler<DomainEventNotification<CustomerUserEmailVerifiedEvent>>
    {
        private readonly ILogger<BusinessProfileRejectedEvent> _logger;
		private readonly TrangloUserManager userManager;
		private readonly BusinessProfileService _businessProfileService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly PartnerService _partnerService;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IWebHostEnvironment _environment;

        public BusinessProfileRejectedEventHandler(
            ILogger<BusinessProfileRejectedEvent> logger,
            IConfiguration config,PartnerService partnerService,IApplicationUserRepository applicationUserRepository,
            TrangloUserManager userManager, INotificationService notificationService,
            BusinessProfileService businessProfileService,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _partnerService = partnerService;
            _applicationUserRepository=applicationUserRepository;
            _notificationService = notificationService;
            _config = config;
            this.userManager = userManager;
			_businessProfileService = businessProfileService;
            _environment = environment;
        }

        protected override async Task HandleAsync(BusinessProfileRejectedEvent @event, CancellationToken cancellationToken)
        {
            var domainEvent = @event;

            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync
               (@event.BusinessProfileCode);
            var customerUserRegistration = await _applicationUserRepository.GetCustomerUserRegistrationsByLoginIdAsync
                  (partnerRegistrationInfo.Email.Value);
            var rejectTemplate = "";
            var rejectEmailSubject = "";
            if (@event.AdminSolution == Solution.Connect.Id)
            {
                rejectTemplate = "RejectReviewResultTemplate";
                rejectEmailSubject = $"(Tranglo Connect) KYC Review Result Reject {@event.CompanyName}";
            }
            else if (@event.AdminSolution == Solution.Business.Id)
            {
                rejectTemplate = "BusinessRejectReviewResultTemplate";
                rejectEmailSubject = $"(Tranglo Business) KYC Review Result Reject {@event.CompanyName}";
            }

            // 1. get the email template
            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            // 2. Use xsl to inject the properties from @event
            StringBuilder _xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(_xml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Profile"); 
                writer.WriteElementString("Email", "kyc@tranglo.com");
                writer.WriteElementString("Name", @event.CompanyName);
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                writer.WriteEndElement(); 
                writer.WriteEndDocument();
            }
            string content = generator.GenerateContent(_xml.ToString(), rejectTemplate, cultureName);

            // 3. Send the email
            long recipientType = 1; long notificationTemplate = 7;
            var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientType, notificationTemplate);

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
          
            long ccType = 2;
            var ccInfo = await _businessProfileService.GetRecipientEmail(ccType, notificationTemplate);

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

            long bccType = 3;
            var bccInfo = await _businessProfileService.GetRecipientEmail(bccType, notificationTemplate);

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

       
            Result<HttpStatusCode> sendRejectedNotificationEmailResponse = await _notificationService.SendNotification(recipients, bcc, cc, new List<IFormFile>() { },
                rejectEmailSubject, content, NotificationTypes.Email);

            if (sendRejectedNotificationEmailResponse.IsFailure)
            {
                _logger.LogError("SendNotification", $"Rejected Notification failed for {@event.CompanyName} . {sendRejectedNotificationEmailResponse.Error}.");
            }

            _logger.LogInformation($"Business profile is ready for approval for customer [{@event.CompanyName}]");

        }
    }
}
