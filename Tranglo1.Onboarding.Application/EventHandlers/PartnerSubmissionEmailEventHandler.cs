using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.Common.EventHandlers;
using Tranglo1.Onboarding.Application.Services.Notification;
using System.IO;
using System.Text;
using System.Xml;
using Tranglo1.Onboarding.Domain.Events;
using System;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using System.Collections.Generic;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using System.Net;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.EventHandlers
{
    class PartnerSubmissionEmailEventHandler : BaseEventHandler<PartnerSubmissionEmailEvent>
    {
        private readonly ILogger<PartnerSubmissionEmailEventHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IApplicationUserRepository _applicationUserRepository;

        public PartnerSubmissionEmailEventHandler(
            ILogger<PartnerSubmissionEmailEventHandler> logger,
            INotificationService notificationService,
            IConfiguration config,
            IBusinessProfileRepository businessProfileRepository,
            IWebHostEnvironment environment,
            IApplicationUserRepository applicationUserRepository)
        {
            _logger = logger;
            _notificationService = notificationService;
            _config = config;
            _businessProfileRepository = businessProfileRepository;
            _environment = environment;
            _applicationUserRepository = applicationUserRepository;
        }
      
        protected override async Task HandleAsync(PartnerSubmissionEmailEvent @event, CancellationToken cancellationToken)
        {
            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);
            var applicationUserInfo = await _applicationUserRepository.GetApplicationUserByUserId(@event.UserId);
            var businessProfileInfo = await _businessProfileRepository.GetBusinessProfileByCodeAsync(@event.BusinessProfileCode);

            // 2. Use xsl to inject the properties from @event
            StringBuilder _xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(_xml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PartnerSubmission");
                writer.WriteElementString("PICName", @event.PICName);
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            string content = generator.GenerateContent(_xml.ToString(), "PartnerSubmissionEmailTemplate", cultureName);

            var recipient = new List<RecipientsInputDTO>()
                            { new RecipientsInputDTO()
                                { email = applicationUserInfo.Email.Value, name = businessProfileInfo.CompanyName }
                            };

            Result<HttpStatusCode> sendBusinessPartnerGoLiveEmailResponse =
                            await _notificationService.SendNotification
                            (
                                recipient, null,
                                null, new List<IFormFile>() { },
                                "Your Application for Tranglo Business is Under Review",
                                content, NotificationTypes.Email
                            );

        }
    }
}
