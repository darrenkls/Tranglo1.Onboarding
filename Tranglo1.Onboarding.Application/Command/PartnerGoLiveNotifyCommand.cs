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
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class PartnerGoLiveNotifyCommand : BaseCommand<Result>
	{
		public long PartnerCode { get; set; }
		public long PartnerSubscriptionCode { get; set; }
        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Send Go Live Notify Upon Admin Approval for Partner Code";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class PartnerGoLiveNotifyCommandHandler: IRequestHandler<PartnerGoLiveNotifyCommand, Result>
		{
            private readonly ILogger<PartnerGoLiveNotifyCommandHandler> _logger;
            private readonly BusinessProfileService _businessProfileService;
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IWebHostEnvironment _environment;

            public PartnerGoLiveNotifyCommandHandler(
                ILogger<PartnerGoLiveNotifyCommandHandler> logger,
                BusinessProfileService businessProfileService,
				IPartnerRepository partnerRepository,
                INotificationService notificationService,
                IConfiguration configuration, IWebHostEnvironment environment)
			{
                _businessProfileService = businessProfileService;
                _partnerRepository = partnerRepository;
                _notificationService = notificationService;
                _config = configuration;
                _logger = logger;
                _environment = environment;
			}

			public async Task<Result> Handle(PartnerGoLiveNotifyCommand request, CancellationToken cancellationToken)
			{
                //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
                var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                var generator = new Common.ContentGenerator(xsltTemplateRootPath);

                var partnerDetails = await _partnerRepository.GetPartnerDetailsByCodeAsync(request.PartnerCode);
                var subscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
                var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(subscription.TrangloEntity);
                var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(partnerDetails.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

                StringBuilder _xml = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(_xml))
				{
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Profile");
                    writer.WriteElementString("Name", businessProfile?.CompanyName );
                    writer.WriteElementString("Email1", "bd@tranglo.com");
                    writer.WriteElementString("LoginUrl", _config.GetValue<string>("ConnectPortalUri"));
                    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                    writer.WriteElementString("EntityName", trangloEntity.Name.ToString());
                    writer.WriteElementString("SolutionName", subscription.Solution.Name.ToString());
                    writer.WriteElementString("PartnerType", subscription.PartnerType.Name.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                //AdminAppovePartnerGoLiveTemplate
                string content = generator.GenerateContent(_xml.ToString(), "AdminAppovePartnerGoLiveTemplate", cultureName);

                long recipientTypeCode = 1;
                long ccTypeCode = 2;
                long bccTypeCode = 3;

                if (subscription.Environment != Domain.Entities.Environment.Production)
                {
                    return Result.Failure($"Partner not yet live. Current Env {subscription.Environment?.Name}");
                }

                long AdminApprovePartnrGoLiveTemplateCode = NotificationTemplate.AdminApprovePartnrGoLive.Id;

                var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, AdminApprovePartnrGoLiveTemplateCode);
                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, AdminApprovePartnrGoLiveTemplateCode);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, AdminApprovePartnrGoLiveTemplateCode);

                var recipients = new List<RecipientsInputDTO>();
                var cc = new List<RecipientsInputDTO>();
                var bcc = new List<RecipientsInputDTO>();

                bool isSupplyPartner = subscription.PartnerType.Id == PartnerType.Supply_Partner.Id;

                foreach (var emailist in recipientInfo)
                {
                    var recipientlist = new RecipientsInputDTO()
                    {                    
                        email = emailist.Email,
                        name = emailist.Name                      
                    };
                    recipients.Add(recipientlist);
                    
                    if (!isSupplyPartner)
                    {
                        var recipientlistPartner = new RecipientsInputDTO()
                        {
                            email = partnerDetails.Email.Value,
                            name = businessProfile.CompanyName
                        };
                        recipients.Add(recipientlistPartner);
                    }    
                }
                foreach (var emailist in ccInfo)
                {
                    if (isSupplyPartner)
                    {
                        var supplyPartnerRecipientList = new RecipientsInputDTO()
                        {
                            email = emailist.Email,
                            name = emailist.Name
                        };
                        recipients.Add(supplyPartnerRecipientList);
                    }
                    else
                    {
                        var cclist = new RecipientsInputDTO()
                        {
                            email = emailist.Email,
                            name = emailist.Name
                        };
                        cc.Add(cclist);
                    }                                       
                }
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

                Result<HttpStatusCode> sendGoLiveNotificationEmail = await _notificationService.SendNotification
                   (
                       recipients,
                       bcc,
                       cc,
                       file,
                       $"{businessProfile.CompanyName}",
                       content,
                       NotificationTypes.Email
                   );

                if (sendGoLiveNotificationEmail.IsFailure)
                {
                    _logger.LogError($"[PartnerGoLiveNotifyCommand] PartnerGoLiveNotification Send Error for {businessProfile.CompanyName}, " +
                        $"PartnerCode {request.PartnerCode}, PartnerSubscriptionCode: {request.PartnerSubscriptionCode} " +
                        $"Error : {sendGoLiveNotificationEmail.Error}");
                    return Result.Failure(sendGoLiveNotificationEmail.Error);
                }

                return Result.Success();
			}
		}
    }

}