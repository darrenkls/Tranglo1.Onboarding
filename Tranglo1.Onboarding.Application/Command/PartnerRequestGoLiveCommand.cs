using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerRequestGoLive, UACAction.Edit)]
    [Permission(Permission.RequestGoLiveButton.Action_RequestGoLive_Code,
        new int[] { (int)PortalCode.Connect },
        new string[] { })]
    internal class PartnerRequestGoLiveCommand : BaseCommand<Result<PartnerRequestGoLiveOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public string UserBearerToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerRequestGoLiveOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Partner Onboarding Status for Partner Code: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }



        internal class PartnerRequestGoLiveCommandHandler : IRequestHandler<PartnerRequestGoLiveCommand, Result<PartnerRequestGoLiveOutputDTO>>
        {
            private readonly ILogger<PartnerRequestGoLiveCommandHandler> _logger;
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IBusinessProfileRepository _businessProfileRepository;

            public PartnerRequestGoLiveCommandHandler(ILogger<PartnerRequestGoLiveCommandHandler> logger,
                                                        INotificationService notificationService,
                                                        IConfiguration config,
                                                        IPartnerRepository partnerRepository,
                                                        IBusinessProfileRepository businessProfileRepository)
            {
                _logger = logger;
                _logger = logger;
                _notificationService = notificationService;
                _config = config;
                _partnerRepository = partnerRepository;
            }

            public async Task<Result<PartnerRequestGoLiveOutputDTO>> Handle(PartnerRequestGoLiveCommand request, CancellationToken cancellationToken)
            {
                var adminPortal = _config.GetValue<string>("IntranetUri");
                var currentYear = DateTime.Now.Year.ToString();

                var partnerSubscription = _partnerRepository.GetSubscriptionsByPartnerCodeAsync(request.PartnerCode);
                var partnerRegistration = await _partnerRepository.GetPartnerDetailsByCodeAsync(request.PartnerCode);
                var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(partnerRegistration.BusinessProfileCode);
                bool isTrangloConnect = partnerSubscription.Result.Any(x => x.Solution == Solution.Connect);
                bool isTrangloBusiness = partnerSubscription.Result.Any(x => x.Solution == Solution.Business);

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

                try
                {
                    var recipients = new List<RecipientsInputDTO>
                        {
                            new RecipientsInputDTO()
                            {
                                email = partnerRegistration.Email.Value,
                                name = businessProfile.CompanyName
                            }
                        };

                    var cc = new List<RecipientsInputDTO>();

                    var bcc = new List<RecipientsInputDTO>();

                    EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                    emailNotificationInputDTO.recipients = recipients;
                    emailNotificationInputDTO.cc = cc;
                    emailNotificationInputDTO.bcc = bcc;
                    emailNotificationInputDTO.Url = adminPortal;
                    emailNotificationInputDTO.RecipientName = businessProfile.CompanyName;
                    emailNotificationInputDTO.NotificationTemplate = "BusinessAccountGoLiveTemplate";
                    emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                    emailNotificationInputDTO.subject = $"({solution}) Your account is ready - {businessProfile.CompanyName}";

                    var sendInviteUserNewUserEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);

                    if (sendInviteUserNewUserEmailResponse.IsFailure)
                    {
                        _logger.LogError("SendNotification", $"[CustomerUserInvitationSubmittedEventHandler] Invite New User Notification failed for {businessProfile.CompanyName}. {sendInviteUserNewUserEmailResponse.Error}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[CustomerUserInvitationSubmittedEventHandler] {ex.ToString()}");
                }

                var result = new PartnerRequestGoLiveOutputDTO
                {
                    PartnerCode = request.PartnerCode,
                    PartnerSubscriptionCode = request.PartnerSubscriptionCode,
                    Status = true
                };
                return Result.Success(result);
            }


            //try
            //{


            /*                var domainEvent = @event;
            */

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            //var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            //string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            //var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            ////notificationTemplateCode for PartnerOnboarding
            //long partnerOnboardingTemplateCode = 17;

            ////RecipientTypeCode for recipient, cc, bcc
            //long recipientTypeCode = 1;
            //long ccTypeCode = 2;
            //long bccTypeCode = 3;

            //var partnerDetails = await _partnerRepository.GetPartnerDetailsByCodeAsync(request.PartnerCode);
            //var partnerSubs = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
            //var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubs.TrangloEntity);
            //var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(partnerDetails.BusinessProfileCode);

            //BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();


            //var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, partnerOnboardingTemplateCode);
            //var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, partnerOnboardingTemplateCode);
            //var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, partnerOnboardingTemplateCode);

            //var recipients = new List<RecipientsInputDTO>();

            //foreach (var emailist in recipientInfo)
            //{
            //    var recipientlist = new RecipientsInputDTO()
            //    {
            //        email = emailist.Email,
            //        name = emailist.Name
            //    };
            //    recipients.Add(recipientlist);
            //}

            //var cc = new List<RecipientsInputDTO>();

            //foreach (var emailist in ccInfo)
            //{
            //    var cclist = new RecipientsInputDTO()
            //    {
            //        email = emailist.Email,
            //        name = emailist.Name
            //    };
            //    cc.Add(cclist);
            //}

            //var bcc = new List<RecipientsInputDTO>();

            //foreach (var emailist in bccInfo)
            //{
            //    var bcclist = new RecipientsInputDTO()
            //    {
            //        email = emailist.Email,
            //        name = emailist.Name
            //    };
            //    bcc.Add(bcclist);
            //}

            //    var file = new List<IFormFile> { };

            //    StringBuilder _xml = new StringBuilder();
            //    using (XmlWriter writer = XmlWriter.Create(_xml))
            //    {
            //        writer.WriteStartDocument();
            //        writer.WriteStartElement("Profile");                 // expected to match: <xsl:template match="Submission">                    
            //        writer.WriteElementString("Name", businessProfile.CompanyName);  // TODO: insert data for partner user name
            //        writer.WriteElementString("LoginUrl", adminPortal);     // TODO: insert data for the url endpoint
            //        writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
            //        writer.WriteElementString("EntityName", trangloEntity.Name.ToString());
            //        writer.WriteElementString("SolutionName", partnerSubs.Solution.Name.ToString());
            //        writer.WriteElementString("PartnerType", partnerSubs.PartnerType.Name.ToString());
            //        writer.WriteEndElement();
            //        writer.WriteEndDocument();
            //    }

            //    string content = generator.GenerateContent(_xml.ToString(), "PartnerRequestedGoLiveTemplate", cultureName);
            //    string body = content;

            //    Result<HttpStatusCode> sendPartnerOnboardingEmail = await _notificationService.SendNotification
            //    (
            //        recipients,
            //        bcc,
            //        cc,
            //        file,
            //        $"{businessProfile.CompanyName}",
            //        body,
            //        NotificationTypes.Email
            //    );

            //    if (sendPartnerOnboardingEmail.IsFailure)
            //    {
            //        _logger.LogError($"[PartnerRequestGoLiveCommand] Account Status Change email notification failed for {businessProfile.CompanyName}. {sendPartnerOnboardingEmail.Error}.");
            //    }

            //    _logger.LogInformation($"Account Status is active for [{businessProfile.CompanyName}]");

            //    var result = new PartnerRequestGoLiveOutputDTO
            //    {
            //        PartnerCode = request.PartnerCode,
            //        PartnerSubscriptionCode = request.PartnerSubscriptionCode,
            //        Status = true
            //    };
            //    return Result.Success(result);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"[PartnerRequestGoLiveCommand] {ex.Message}");
            //}
            //return Result.Failure<PartnerRequestGoLiveOutputDTO>(
            //            $"Request GoLive Failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
            //        );
            //}

        }
    }
}