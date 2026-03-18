using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
    class BusinessProfileRegisteredEventHandler : BaseEventHandler<BusinessProfileRegisteredEvent>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ILogger<BusinessProfileRegisteredEvent> _logger;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public BusinessProfileRegisteredEventHandler(
            IBusinessProfileRepository businessProfileRepository,
            IPartnerRepository partnerRepository,
            ILogger<BusinessProfileRegisteredEvent> logger,
            BusinessProfileService businessProfileService,
            INotificationService notificationService,
            IWebHostEnvironment environment,
            IConfiguration config)
        {
            _logger = logger;
            _businessProfileRepository = businessProfileRepository;
            _businessProfileService = businessProfileService;
            _partnerRepository = partnerRepository;
            _notificationService = notificationService;
            _environment = environment;
            _config = config;
        }
        protected override async Task HandleAsync(BusinessProfileRegisteredEvent @event, CancellationToken cancellationToken)
        {
            DateTime? termsAcceptanceDate = null;

            var domainEvent = @event;

            PartnerType partnerTypeCode = null;
            string settlementCurrencyCode = null;

            if (domainEvent.IsTncTick)
            {
                termsAcceptanceDate = DateTime.UtcNow.Date;
            }
            //add checking for country and solution
            if (domainEvent.Solution != null && domainEvent.Solution.Id == Solution.Business.Id)
            {
                if (domainEvent.CountryISO2.CountryISO2 == CountryMeta.Malaysia.CountryISO2)
                {
                    //default TSB, MYR, Sales Partner
                    partnerTypeCode = PartnerType.Sales_Partner;
                    settlementCurrencyCode = "MYR";

                }
                else
                {
                    partnerTypeCode = PartnerType.Sales_Partner;
                    settlementCurrencyCode = "SGD";
                }
            }

            PartnerRegistration addPartner = new PartnerRegistration(
                domainEvent.BusinessProfile, domainEvent.Email, domainEvent.CustomerTypeCode, domainEvent.IMID,
                domainEvent.AgentLoginId, termsAcceptanceDate, domainEvent.LeadsOrigin, domainEvent.OtherLeadsOrigin);

            var partner = await _partnerRepository.AddPartnerRegistrationAsync(addPartner);

            PartnerSubscription addSubscription = new PartnerSubscription(
                partner.Value.Id, partnerTypeCode, domainEvent.Solution, domainEvent.TrangloEntity,
                settlementCurrencyCode, domainEvent.RspStagingId, domainEvent.SupplierPartnerStagingId, null, false);

            var subscription = await _partnerRepository.AddSubcriptionAsync(addSubscription);

            //Send Email
            if (domainEvent.Solution != null && domainEvent.Solution.Id == Solution.Business.Id)
            {
                var adminPortal = _config.GetValue<string>("IntranetUri");
                var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                string cultureName = Thread.CurrentThread.CurrentUICulture.Name;
                var generator = new Common.ContentGenerator(xsltTemplateRootPath);

                TrangloEntity trangloEntity = TrangloEntity.GetByEntityByTrangloId(domainEvent.TrangloEntity);
                string trangloEntityName = trangloEntity.Name;

                //notificationTemplateCode for AddSubscription
                long addSubscriptionTemplateCode = 27;

                //RecipientTypeCode for recipient, cc, bcc
                long recipientTypeCode = 1;
                long ccTypeCode = 2;
                long bccTypeCode = 3;

                var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, addSubscriptionTemplateCode);
                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, addSubscriptionTemplateCode);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, addSubscriptionTemplateCode);

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
                    writer.WriteStartElement("Subscription");                 // expected to match: <xsl:template match="Submission">                    
                    writer.WriteElementString("PartnerName", domainEvent.BusinessProfile.CompanyName);  // insert data for company name
                    writer.WriteElementString("LoginUrl", adminPortal);     // insert data for the url endpoint
                    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // insert data for current year
                    writer.WriteStartElement("Subscriptions");
                    writer.WriteElementString("EntityName", trangloEntityName);     // insert data for the EntityName
                    writer.WriteElementString("Entity", "Entity ");
                    writer.WriteElementString("PartnerType", PartnerType.Sales_Partner.Name); // insert data for the PartnerType
                    writer.WriteElementString("Partner", "PartnerType");
                    writer.WriteElementString("SolutionName", Solution.Business.Name);  // insert data for the SolutionName
                    writer.WriteElementString("Solution", "Solution");
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                string content = generator.GenerateContent(_xml.ToString(), "AddSubscriptionTemplate", cultureName);
                string body = content;

                Result<HttpStatusCode> sendSubscriptionEmailResponse = await _notificationService.SendNotification
                (
                    recipients,
                    bcc,
                    cc,
                    file,
                    $"Subscription Added for {domainEvent.BusinessProfile.CompanyName}",
                    body,
                    NotificationTypes.Email
                );

                if (sendSubscriptionEmailResponse.IsFailure)
                {
                    _logger.LogError("SendNotification", $"Add Subscription email notification failed for {domainEvent.BusinessProfile.CompanyName} . {sendSubscriptionEmailResponse.Error}.");
                }
                _logger.LogInformation($"Subscription Added for [{domainEvent.BusinessProfile.CompanyName}]");

            }
        }
    }
}
