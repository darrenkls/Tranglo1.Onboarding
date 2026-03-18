using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.Common.EventHandlers;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.Services.Notification;

namespace Tranglo1.Onboarding.Application.EventHandlers
{
    class SubmissionResubmissionEmailEventHandler : BaseEventHandler<SubmissionResubmissionEmailEvent>
    {
        private readonly ILogger<SubmissionResubmissionEmailEventHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IWebHostEnvironment _environment;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public SubmissionResubmissionEmailEventHandler(
            ILogger<SubmissionResubmissionEmailEventHandler> logger,
            INotificationService notificationService,
            IConfiguration config,
            BusinessProfileService businessProfileService,
            IWebHostEnvironment environment,
            IApplicationUserRepository applicationUserRepository,
            IBusinessProfileRepository businessProfileRepository)
        {
            _logger = logger;
            _notificationService = notificationService;
            _config = config;
            _businessProfileService = businessProfileService;
            _environment = environment;
            _applicationUserRepository = applicationUserRepository;
            _businessProfileRepository = businessProfileRepository;
        }

        protected override async Task HandleAsync(SubmissionResubmissionEmailEvent @event, CancellationToken cancellationToken)
        {
            var adminPortal = _config.GetValue<string>("IntranetUri");
            var currentYear = DateTime.Now.Year.ToString();

            //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
            var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
            string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var generator = new Common.ContentGenerator(xsltTemplateRootPath);

            //notificationTemplateCode for submission, resubmission
            long submissionTemplateCode = 11;
            long resubmissionTemplateCode = 10;

            //RecipientTypeCode for recipient, cc, bcc
            long recipientTypeCode = 1;
            long ccTypeCode = 2;
            long bccTypeCode = 3;

            //Solution
            var kycSubmissionEmailSubject = "";
            var submissionSalution = "";
            var submissionSolution = "";
            var submissionTyper = "";

            if (@event.solutionCode == Solution.Connect.Id)
            {
                kycSubmissionEmailSubject = $"(Tranglo Connect) KYC Submission For Review {@event.CompanyName}";
                submissionSalution = "Tranglo Compliance";
                submissionSolution = "(Tranglo Connect)";
                submissionTyper = "compliance";
            }
            else if (@event.solutionCode == Solution.Business.Id)
            {
                kycSubmissionEmailSubject = $"(Tranglo Business) KYC Submission For Review {@event.CompanyName}";
                submissionSolution = "(Tranglo Business)";
                if (@event.CollectionTier?.Id == CollectionTier.Tier_1.Id)
                {
                    submissionSalution = "Tranglo KYC Operation";
                    submissionTyper = "your";
                }
                else if (@event.CollectionTier?.Id == CollectionTier.Tier_2.Id || @event.CollectionTier?.Id == CollectionTier.Tier_3.Id)
                {
                    submissionSalution = "Tranglo Compliance";
                    submissionTyper = "compliance";
                }
            }

            if ((@event.KYCSubmissionStatusCode == KYCSubmissionStatus.Submitted.Id || @event.KYCSubmissionStatusCode == KYCSubmissionStatus.Draft.Id) && @event.KYCSubmissionDate == null)
            {
                List<EmailRecipient> recipientInfo = new List<EmailRecipient>();
                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, submissionTemplateCode);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, submissionTemplateCode);
                if (@event.solutionCode == Solution.Connect.Id)
                {
                    recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, submissionTemplateCode);
                }
                else if (@event.solutionCode == Solution.Business.Id)
                {
                    recipientInfo = await _businessProfileService.GetRecipientEmailByCollectionTier(@event.CollectionTier.Id, recipientTypeCode, submissionTemplateCode);
                }
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

                EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                emailNotificationInputDTO.recipients = recipients;
                emailNotificationInputDTO.cc = null;
                emailNotificationInputDTO.bcc = null;
                emailNotificationInputDTO.RecipientName = submissionSalution;
                emailNotificationInputDTO.SolutionName = submissionSolution;
                emailNotificationInputDTO.Typer = submissionTyper;
                emailNotificationInputDTO.CompanyName = @event.CompanyName;
                emailNotificationInputDTO.LoginUrl = adminPortal;
                emailNotificationInputDTO.NotificationTemplate = "SubmissionforReviewTemplate";
                emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                emailNotificationInputDTO.subject = kycSubmissionEmailSubject;

                var sendKycSubmitEmail = await _notificationService.SendNotification(emailNotificationInputDTO);

                if (sendKycSubmitEmail.IsFailure)
                {
                    _logger.LogError("SendNotification", $"[SubmissionResubmissionEmailEventHandler] Submit KYC for review Email failed for Partner Code : {submissionSalution} - {sendKycSubmitEmail.Error}.");
                }
                else
                {
                    _logger.LogInformation($"Applicationuser Email : {submissionSalution} - Submit KYC for review Email Sent.");
                }


                //StringBuilder _xml = new StringBuilder();
                //using (XmlWriter writer = XmlWriter.Create(_xml))
                //{
                //    writer.WriteStartDocument();
                //    writer.WriteStartElement("Submission");                 // expected to match: <xsl:template match="Submission">
                //    writer.WriteElementString("Salutation", submissionSalution);  //TODO: insert data for salutation
                //    writer.WriteElementString("Solution", submissionSolution);  //TODO: insert data for solution
                //    writer.WriteElementString("Typer", submissionTyper);  //TODO: insert data for typer  
                //    writer.WriteElementString("Name", @event.CompanyName);  // TODO: insert data for company name                  
                //    writer.WriteElementString("LoginUrl", adminPortal);     // TODO: insert data for the url endpoint
                //    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());  // TODO: insert data for current year
                //    writer.WriteEndElement();
                //    writer.WriteEndDocument();
                //}

                //var submissionTemplate = "SubmissionforReviewTemplate";
                //string content = generator.GenerateContent(_xml.ToString(), submissionTemplate, cultureName);
                //string body = content;

                //Result<HttpStatusCode> sendResubmissionEmail = await _notificationService.SendNotification
                //(
                //    recipients,
                //    bcc,
                //    cc,
                //    file,
                //    kycSubmissionEmailSubject,
                //    body,
                //    NotificationTypes.Email
                //);
            }

            else if ((@event.KYCSubmissionStatusCode == KYCSubmissionStatus.Submitted.Id || @event.KYCSubmissionStatusCode == KYCSubmissionStatus.Draft.Id) && @event.KYCSubmissionDate != null)
            {
                //Solution
                var kycResubmissionEmailSubject = "";
                var resubmissionSalutation = "";
                var resubmissionSolution = "";
                var resubmissionTyper = "";
                ;
                if (@event.solutionCode == Solution.Connect.Id)
                {
                    kycResubmissionEmailSubject = $"(Tranglo Connect) KYC Updated for Review {@event.CompanyName}";
                    resubmissionSalutation = "Tranglo Compliance";
                    resubmissionSolution = "(Tranglo Connect)";
                    resubmissionTyper = "compliance";
                }
                else if (@event.solutionCode == Solution.Business.Id)
                {
                    kycResubmissionEmailSubject = $"(Tranglo Business) KYC Updated for Review {@event.CompanyName}";
                    resubmissionSolution = "(Tranglo Business)";
                    if (@event.CollectionTier?.Id == CollectionTier.Tier_1.Id)
                    {
                        resubmissionSalutation = "Tranglo KYC Operation";
                        resubmissionTyper = "your";
                    }
                    else if (@event.CollectionTier?.Id == CollectionTier.Tier_2.Id || @event.CollectionTier?.Id == CollectionTier.Tier_3.Id)
                    {
                        resubmissionSalutation = "Tranglo Compliance";
                        resubmissionTyper = "compliance";
                    }
                }

                List<EmailRecipient> recipientInfo = new List<EmailRecipient>();

                var ccInfo = await _businessProfileService.GetRecipientEmail(ccTypeCode, resubmissionTemplateCode);
                var bccInfo = await _businessProfileService.GetRecipientEmail(bccTypeCode, resubmissionTemplateCode);

                if (@event.solutionCode == Solution.Connect.Id)
                {
                    recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, resubmissionTemplateCode);
                }
                else if (@event.solutionCode == Solution.Business.Id)
                {
                    recipientInfo = await _businessProfileService.GetRecipientEmailByCollectionTier(@event.CollectionTier.Id, recipientTypeCode, resubmissionTemplateCode);
                }

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

                EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                emailNotificationInputDTO.recipients = recipients;
                emailNotificationInputDTO.cc = null;
                emailNotificationInputDTO.bcc = null;
                emailNotificationInputDTO.RecipientName = resubmissionSalutation;
                emailNotificationInputDTO.SolutionName = resubmissionSolution;
                emailNotificationInputDTO.Typer = resubmissionTyper;
                emailNotificationInputDTO.CompanyName = @event.CompanyName;
                emailNotificationInputDTO.LoginUrl = adminPortal;
                emailNotificationInputDTO.NotificationTemplate = "ResubmissionforReview";
                emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                emailNotificationInputDTO.subject = kycResubmissionEmailSubject;

                var sendKycSubmitEmail = await _notificationService.SendNotification(emailNotificationInputDTO);

                if (sendKycSubmitEmail.IsFailure)
                {
                    _logger.LogError("SendNotification", $"[SubmissionResubmissionEmailEventHandler] Resubmit KYC for review Email failed for Partner : {submissionSalution} - {sendKycSubmitEmail.Error}.");
                }
                else
                {
                    _logger.LogInformation($"Partner : {submissionSalution} - Resubmit KYC for review Email Sent.");
                }

                //StringBuilder _xml = new StringBuilder();
                //using (XmlWriter writer = XmlWriter.Create(_xml))
                //{
                //    writer.WriteStartDocument();
                //    writer.WriteStartElement("Resubmission");               // expected to match: <xsl:template match="Resubmission">
                //    writer.WriteElementString("Salutation", resubmissionSalutation);  //TODO: insert data for salutation   
                //    writer.WriteElementString("Solution", resubmissionSolution);  //TODO: insert data for salutation 
                //    writer.WriteElementString("Typer", resubmissionTyper);  //TODO: insert data for typer  
                //    writer.WriteElementString("Name", @event.CompanyName);  // TODO: insert data for company name                    
                //    writer.WriteElementString("LoginUrl", adminPortal);     // TODO: insert data for the url endpoint
                //    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                //    writer.WriteEndElement();                               // end root element
                //    writer.WriteEndDocument();
                //}

                //var resubmissionTemplate = "ResubmissionforReview";
                //string content = generator.GenerateContent(_xml.ToString(), resubmissionTemplate, cultureName);
                //string body = content;

                //Result<HttpStatusCode> sendResubmissionEmail = await _notificationService.SendNotification
                //(
                //    recipients,
                //    bcc,
                //    cc,
                //    file,
                //    kycResubmissionEmailSubject,
                //    body,
                //    NotificationTypes.Email
                //);
            }


            //Send Email to partner after submit for review
            if (@event.CustomerSolution != null && @event.KYCSubmissionStatusCode == KYCSubmissionStatus.Draft.Id)
            {
                var applicationUserInfo = await _applicationUserRepository.GetApplicationUserByUserId(@event.UserId);
                var businessProfileInfo = await _businessProfileRepository.GetBusinessProfileByCodeAsync(@event.BusinessProfileCode);
                var emailSubject = "";

                if (@event.CustomerSolution == ClaimCode.Connect)
                {
                    emailSubject = $"({Solution.Connect.Name}) Your KYC submission is under review";
                }
                else if (@event.CustomerSolution == ClaimCode.Business)
                {
                    emailSubject = $"({Solution.Business.Name}) Your KYC submission is under review";
                }

                // 2. Use xsl to inject the properties from @event
                //StringBuilder _xml = new StringBuilder();
                //using (XmlWriter writer = XmlWriter.Create(_xml))
                //{
                //    writer.WriteStartDocument();
                //    writer.WriteStartElement("PartnerSubmission");
                //    writer.WriteElementString("PICName", @event.PICName);
                //    writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                //    writer.WriteEndElement();
                //    writer.WriteEndDocument();
                //}
                //string content = generator.GenerateContent(_xml.ToString(), "PartnerSubmissionEmailTemplate", cultureName);

                var recipients = new List<RecipientsInputDTO>()
                {
                    new RecipientsInputDTO()
                    {
                        email = applicationUserInfo.Email.Value,
                        name = businessProfileInfo.CompanyName
                    }
                };

                EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                emailNotificationInputDTO.recipients = recipients;
                emailNotificationInputDTO.cc = null;
                emailNotificationInputDTO.bcc = null;
                emailNotificationInputDTO.RecipientName = @event.PICName;
                emailNotificationInputDTO.NotificationTemplate = "PartnerSubmissionEmailTemplate";
                emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                emailNotificationInputDTO.subject = emailSubject;

                var sendKycSubmitEmail = await _notificationService.SendNotification(emailNotificationInputDTO);

                if (sendKycSubmitEmail.IsFailure)
                {
                    _logger.LogError("SendNotification", $"[SubmissionResubmissionEmailEventHandler] Submit KYC for review Email failed for Partner : {@event.PICName} - {sendKycSubmitEmail.Error}.");
                }
                else
                {
                    _logger.LogInformation($"Partner : {@event.PICName} - Submit KYC for review Email Sent.");
                }
            }
        }
    }
}