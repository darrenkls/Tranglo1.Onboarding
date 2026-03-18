using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.Edit)]
    public class UpdatePendingWhitelistIPCommand : BaseCommand<Result>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public WhitelistIPAddressInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update pending whitelist IP Address for PartnerSubscriptionCode: [{this.PartnerSubscriptionCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdatePendingWhitelistIPCommandHandler : IRequestHandler<UpdatePendingWhitelistIPCommand, Result>
    {
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<UpdatePendingWhitelistIPCommandHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IWebHostEnvironment _environment;

        public UpdatePendingWhitelistIPCommandHandler(PartnerService partnerService,
                                                        IPartnerRepository partnerRepository,
                                                        ILogger<UpdatePendingWhitelistIPCommandHandler> logger,
                                                        INotificationService notificationService,
                                                        BusinessProfileService businessProfileService,
                                                        IWebHostEnvironment environment)
        {
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
            _logger = logger;
            _notificationService = notificationService;
            _businessProfileService = businessProfileService;
            _environment = environment;
        }

        public async Task<Result> Handle(UpdatePendingWhitelistIPCommand request, CancellationToken cancellationToken)
        {
            // Update IsWhiteListed = true
            var update = await UpdateIsWhitelistedTrue(request);

            // Send email
            if (update.IsSuccess)
            {
                var sendEmail = await SendEmail(request);

                if (sendEmail.IsFailure)
                {
                    return Result.Failure($"Failed to send email notification");
                }
            }

            return Result.Success();
        }

        private async Task<Result> UpdateIsWhitelistedTrue(UpdatePendingWhitelistIPCommand request)
        {
            var dto = request.InputDTO;

            if (dto.Staging != null)
            {
                foreach (var i in dto.Staging)
                {
                    var stagingWhiteListIP = await _partnerService.GetWhitelistIPByIPAddressAsync(request.PartnerSubscriptionCode, i.IPAddressStart, i.IPAddressEnd);

                    if (stagingWhiteListIP != null && !stagingWhiteListIP.IsWhitelisted)
                    {
                        //update
                        stagingWhiteListIP.IsWhitelisted = true;
                        var update = await _partnerService.UpdateWhiteListIPAsync(stagingWhiteListIP);

                        if (update.IsFailure)
                        {
                            return Result.Failure($"Failed to update WhitelistIP for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}");
                        }
                    }
                }
            }

            if (dto.Production != null)
            {
                foreach (var i in dto.Production)
                {
                    var stagingWhiteListIP = await _partnerService.GetWhitelistIPByIPAddressAsync(request.PartnerSubscriptionCode, i.IPAddressStart, i.IPAddressEnd);

                    if (stagingWhiteListIP != null && !stagingWhiteListIP.IsWhitelisted)
                    {
                        //update
                        stagingWhiteListIP.IsWhitelisted = true;
                        var update = await _partnerService.UpdateWhiteListIPAsync(stagingWhiteListIP);

                        if (update.IsFailure)
                        {
                            return Result.Failure($"Failed to update WhitelistIP for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}");
                        }
                    }
                }
            }

            return Result.Success();
        }

        private async Task<Result> SendEmail(UpdatePendingWhitelistIPCommand request)
        {
            var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(partnerRegistration.BusinessProfileCode);
            var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
            var trangloEntity = await _partnerRepository.GetTrangloEntityMeta(partnerSubscription.TrangloEntity);

            List<string> stagingIpAddresses = new List<string>();
            List<string> productionIpAddresses = new List<string>();

            if (request.InputDTO.Staging != null)
            {
                foreach (var i in request.InputDTO.Staging)
                {
                    if (i.IPAddressStart != null && i.IPAddressEnd != null)
                    {
                        string rangedIPAddress = $"{ i.IPAddressStart } - { i.IPAddressEnd }";
                        stagingIpAddresses.Add(rangedIPAddress);
                    }
                    else if (i.IPAddressEnd == null)
                    {
                        string IPAddressStart = i.IPAddressStart;
                        stagingIpAddresses.Add(IPAddressStart);
                    }
                }
            }

            if (request.InputDTO.Production != null)
            {
                foreach (var i in request.InputDTO.Production)
                {
                    if (i.IPAddressStart != null && i.IPAddressEnd != null)
                    {
                        string rangedIPAddress = $"{ i.IPAddressStart } - { i.IPAddressEnd }";
                        productionIpAddresses.Add(rangedIPAddress);
                    }
                    else if (i.IPAddressEnd == null)
                    {
                        string IPAddressStart = i.IPAddressStart;
                        productionIpAddresses.Add(IPAddressStart);
                    }
                }
            }

            // Concatenate list of IP Addresses into a string
            string concatStagingIPAdresses = String.Join(" ,", stagingIpAddresses.ToArray()) ?? "-";
            string concatProductionIPAdresses = String.Join(" ,", productionIpAddresses.ToArray()) ?? "-";

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            //notificationTemplateCode for WhitelistIP
            long templateCode = 18;

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

            //Add Partner Registered Email as a recipient
            if (partnerRegistration.Email != null && businessProfile.Value.CompanyName != null)
            {
                var partnerRegisteredEmail = new RecipientsInputDTO()
                {
                    email = partnerRegistration.Email.Value,
                    name = businessProfile.Value.CompanyName
                };
                recipients.Add(partnerRegisteredEmail);
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
                writer.WriteStartElement("WhitelistIPTemplate");                 // expected to match: <xsl:template match="Submission">                    
                writer.WriteElementString("PartnerName", businessProfile.Value.CompanyName /*"Test"*/);  // TODO: insert data for company name
                writer.WriteElementString("StagingIPAddress", concatStagingIPAdresses);  // TODO: insert data for staging IP Address                
                writer.WriteElementString("ProductionIPAddress", concatProductionIPAdresses);  // TODO: insert data for production IP Address 
                writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                writer.WriteElementString("EntityName", trangloEntity.Name.ToString());
                writer.WriteElementString("SolutionName", partnerSubscription.Solution.Name.ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            var template = "WhitelistIPTemplate";
            string content = generator.GenerateContent(_xml.ToString(), template, cultureName);
            string body = content;

            Result<HttpStatusCode> sendResubmissionEmail = await _notificationService.SendNotification
            (
                recipients,
                bcc,
                cc,
                file,
                "Successful IP Address Whitelist",
                body,
                NotificationTypes.Email
            );

            return Result.Success();
        }
    }
}
