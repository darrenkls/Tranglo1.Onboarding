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
	class BusinessProfilePendingHigherApprovalEventHandler : BaseEventHandler<BusinessProfilePendingHigherApprovalEvent> // INotificationHandler<DomainEventNotification<CustomerUserEmailVerifiedEvent>>
    {
        private readonly ILogger<BusinessProfilePendingHigherApprovalEvent> _logger;
		private readonly TrangloUserManager userManager;
		private readonly BusinessProfileService _businessProfileService;
        private readonly PartnerService _partnerService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IPartnerRepository _partnerRepository;

        public BusinessProfilePendingHigherApprovalEventHandler(
            ILogger<BusinessProfilePendingHigherApprovalEvent> logger,
             IConfiguration config,
             PartnerService partnerService,
            TrangloUserManager userManager, INotificationService notificationService,
             IApplicationUserRepository applicationUserRepository,
            BusinessProfileService businessProfileService,
            IWebHostEnvironment environment,
            IPartnerRepository partnerRepository)
        {
            _logger = logger;
            _partnerService = partnerService;
            _applicationUserRepository = applicationUserRepository;
            _notificationService = notificationService;
            _config = config;
            this.userManager = userManager;
			_businessProfileService = businessProfileService;
            _environment = environment;
            _partnerRepository = partnerRepository;
        }

        protected override async Task HandleAsync(BusinessProfilePendingHigherApprovalEvent @event, CancellationToken cancellationToken)
        {
            var domainEvent = @event;

            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync
                 (@event.BusinessProfileCode);
            var customerUserRegistration = await _applicationUserRepository.GetCustomerUserRegistrationsByLoginIdAsync
                  (partnerRegistrationInfo.Email.Value);

            var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistrationInfo.Id);

            var entities = partnerSubscriptions.Where(x => x.TrangloEntity != null).Select(x => x.TrangloEntity).ToList();
            string entitiesConcat = String.Join(" / ", entities);

            var pendingTemplate = "";
            var pendingEmailSubject = "";
            var pendingSolution = "";
            if (@event.AdminSolution == Solution.Connect.Id)
            {
                pendingTemplate = "PendingReviewResultTemplate";
                pendingEmailSubject = $"(Tranglo Connect) Pending Higher Approval: {@event.CompanyName} x {entitiesConcat}";
                pendingSolution = "(Tranglo Connect)";
            }
            else if (@event.AdminSolution == Solution.Business.Id)
            {
                pendingTemplate = "BusinessPendingReviewResultTemplate";
                pendingEmailSubject = $"(Tranglo Business) Pending Higher Approval: {@event.CompanyName} x {entitiesConcat}";
                pendingSolution = "(Tranglo Business)";
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
                writer.WriteElementString("Name", @event.CompanyName);
                writer.WriteElementString("Solution", pendingSolution);
                writer.WriteElementString("LoginUrl", _config.GetValue<string>("IntranetUri"));
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                writer.WriteEndElement(); 
                writer.WriteEndDocument();
            }
            string content = generator.GenerateContent(_xml.ToString(), pendingTemplate, cultureName);

            // 3. Send the email
            long recipientType = 1; long notificationTemplate = 8;
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

            Result<HttpStatusCode> sendPendingApprovalNotificationEmailResponse = await _notificationService.SendNotification(recipients, bcc, cc, new List<IFormFile>() { },
                pendingEmailSubject, content, NotificationTypes.Email);

            if (sendPendingApprovalNotificationEmailResponse.IsFailure)
            {
                _logger.LogError("SendNotification", $"Pending Higher Approval Request Notification to HOD failed for {@event.CompanyName} . {sendPendingApprovalNotificationEmailResponse.Error}.");
            }

            _logger.LogInformation($"Business profile is ready for approval for customer [{@event.CompanyName}]");

        }
    }
}
