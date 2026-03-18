using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;


namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_ReleaseDocument_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class SaveDocumentReleasedCommand : BaseCommand<Result<string>>
    {
        public int BusinessProfileCode { get; set; }
        public long RequestId { get; set; }
        public List<IFormFile> uploadedFile  { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<string> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Document Released for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }
    internal class SaveDocumentReleasedCommandHandler : IRequestHandler<SaveDocumentReleasedCommand, Result<string>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveDocumentCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly IConfiguration _config;
        private readonly INotificationService notificationService;
        private readonly IMapper _mapper;
        private readonly PartnerService _partnerService;
        private readonly IWebHostEnvironment _environment;
        private readonly IPartnerRepository _partnerRepository;

        public SaveDocumentReleasedCommandHandler(BusinessProfileService businessProfileService,
                                                      IConfiguration config,
                                                      INotificationService notificationService,
                                                      PartnerService partnerService,
                                                      IMapper mapper,
                                                      ILogger<SaveDocumentCommandHandler> logger,
                                                      StorageManager storageManager,
                                                      IWebHostEnvironment environment,
                                                      IPartnerRepository partnerRepository)
        {
            _businessProfileService = businessProfileService;
            _partnerService = partnerService;
            _logger = logger;
            _storageManager = storageManager;
            _config = config;
            this.notificationService = notificationService;
            _mapper = mapper;
            _environment = environment;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<string>> Handle(SaveDocumentReleasedCommand request, CancellationToken cancellationToken)
        {
            var businessProfile = _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode).Result.Value;
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var partnerSubscriptions = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistrationInfo.Id);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);

            var entities = partnerSubscriptions.Where(x => x.TrangloEntity != null).Select(x => x.TrangloEntity).ToList();
            string entitiesConcat = String.Join(" / ", entities);

            //var trangloEntityName = TrangloEntity.GetByEntityByTrangloId(partnerRegistrationInfo.TrangloEntity);

            try
            {
                /* if(partnerRegistrationInfo.PartnerTypeCode.GetValueOrDefault() == PartnerType.Supply_Partner.Id)
                 {
                     //#40826 disable email send for supply partner
                     return Result.Failure<string>("Email Sending is disabled for Supply Partner");
                 }*/
                var documentErrorMessage = "";
                foreach (var multipleFiles in request.uploadedFile)
                {
                    var extension = Path.GetExtension(multipleFiles.FileName);
                    var allowedExtensions = new[] { ".jpg", ".JPG", ".jpeg", ".JPEG", ".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".PNG", ".zip" };

                    var fileSize = multipleFiles.Length;
                    int maxFileSizeMB = 30;
                    var maxFileSize = maxFileSizeMB * 1024 * 1024;

                    if (fileSize > maxFileSize)
                    {
                        _logger.LogError($"File {multipleFiles.FileName} exceeds the maximum file size allocated.");
                        documentErrorMessage = (documentErrorMessage + System.Environment.NewLine + $"Document {multipleFiles.FileName} current length is {multipleFiles.Length}mb which exceeds the current file upload size limit of {maxFileSizeMB}mb");
                        continue;
                    }

                    if (!allowedExtensions.Contains(extension))
                    {
                        _logger.LogError($"File {multipleFiles.FileName} extensions is not allowed.");
                        documentErrorMessage = (documentErrorMessage + System.Environment.NewLine + $"Document {multipleFiles.FileName} extension is not allowed");
                        continue;
                    }

                    using (var ms = new MemoryStream())
                    {
                        multipleFiles.CopyTo(ms);
                        ms.Position = 0;
                        var doc = await _storageManager.StoreAsync(ms, multipleFiles.FileName, multipleFiles.ContentType);
                        if (doc != null)
                        {
                            {

                                DocumentReleaseBP documentReleasedUpload = new DocumentReleaseBP()
                                {
                                    DocumentId = doc.DocumentId,
                                    BusinessProfile = businessProfile,
                                    IsReleased = true,
                                };

                                await _businessProfileService.AddDocumentReleasedUploadAsync(documentReleasedUpload);
                            }
                        }
                    }
                }

                var releaseDocumentEmailTemplate = "";
                var releaseDocumentSolution = "";
                var workflowStatusEmailContact = "";
                var workflowStatusEmailContactUrl = ""; 

                if (request.AdminSolution == Solution.Connect.Id)
                {
                    releaseDocumentEmailTemplate = "(Tranglo Connect) Tranglo KYC Release Documents";
                    releaseDocumentSolution = "(Tranglo Connect)";
                    workflowStatusEmailContact = "kyc@tranglo.com";
                    workflowStatusEmailContactUrl = "mailto:kyc@tranglo.com";
                }
                else if (request.AdminSolution == Solution.Business.Id)
                {
                    releaseDocumentEmailTemplate = "(Tranglo Business) Tranglo KYC Release Documents";
                    releaseDocumentSolution = "(Tranglo Business)";
                    if (businessProfile.CollectionTier.Id == 1)
                    {
                        workflowStatusEmailContact = "radinindra.ismail@tranglo.com";
                        workflowStatusEmailContactUrl = "mailto:radinindra.ismail@tranglo.com";
                    }
                    else if (businessProfile.CollectionTier.Id == 2 || businessProfile.CollectionTier.Id == 3)
                    {
                        workflowStatusEmailContact = "kyc@tranglo.com";
                        workflowStatusEmailContactUrl = "mailto:kyc@tranglo.com";
                    }
                }

                if(bilateralPartnerFlow == PartnerType.Sales_Partner)
                {
                    {

                        StringBuilder _xml = new StringBuilder();
                        using (XmlWriter writer = XmlWriter.Create(_xml))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement("Profile");
                            writer.WriteElementString("FullName", businessProfile.CompanyName);
                            writer.WriteElementString("Solution", releaseDocumentSolution);
                            writer.WriteElementString("Email", workflowStatusEmailContact);
                            writer.WriteElementString("emailUrl", workflowStatusEmailContactUrl);
                            writer.WriteElementString("LoginUrl", _config.GetValue<string>("ConnectPortalUri"));
                            writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                        }

                        //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
                        var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                        string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                        var generator = new Common.ContentGenerator(xsltTemplateRootPath);
                        string content = generator.GenerateContent(_xml.ToString(), "DocumentReleasedTemplate", cultureName);

                        long ccType = 2; long notificationTemplate = 12;
                        var ccInfo = await _businessProfileService.GetRecipientEmail(ccType, notificationTemplate);

                        long bccType = 3;
                        var bccInfo = await _businessProfileService.GetRecipientEmail(bccType, notificationTemplate);

                        string MailTextDocumentReleased = content;

                        var recipients = new List<RecipientsInputDTO>();

                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = partnerRegistrationInfo.Email.Value,
                            name = businessProfile.ContactPersonName
                        };
                        recipients.Add(recipientlist);

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

                        var sendEmailResponse = await notificationService.SendNotification(

                                            recipients,
                                            bcc,
                                            cc,
                                            request.uploadedFile,
                                            releaseDocumentEmailTemplate,
                                            MailTextDocumentReleased,
                                            NotificationTypes.Email
                                        );
                        return Result.Success<string>("Document have been uploaded");

                    }
                }

                else if(bilateralPartnerFlow == PartnerType.Supply_Partner)
                {
                    {

                        StringBuilder _xml = new StringBuilder();
                        using (XmlWriter writer = XmlWriter.Create(_xml))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement("Profile");
                            writer.WriteElementString("FullName", businessProfile.CompanyName);
                            writer.WriteElementString("Email", partnerRegistrationInfo.Email.Value);
                            writer.WriteElementString("LoginUrl", _config.GetValue<string>("ConnectPortalUri"));
                            writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                        }

                        //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
                        var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");
                        string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                        var generator = new Common.ContentGenerator(xsltTemplateRootPath);
                        string content = generator.GenerateContent(_xml.ToString(), "SupplyPartnerDocumentReleaseTemplate", cultureName);
                        long notificationTemplate = 25;

                        long recipientTypeCode = 1;
                        var recipientInfo = await _businessProfileService.GetRecipientEmail(recipientTypeCode, notificationTemplate);

                        long ccType = 2; 
                        var ccInfo = await _businessProfileService.GetRecipientEmail(ccType, notificationTemplate);

                        long bccType = 3;
                        var bccInfo = await _businessProfileService.GetRecipientEmail(bccType, notificationTemplate);

                        string MailTextDocumentReleased = content;

                        var recipients = new List<RecipientsInputDTO>();

                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = partnerRegistrationInfo.Email.Value,
                            name = businessProfile.ContactPersonName
                        };
                        recipients.Add(recipientlist);

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

                        var datetime = DateTime.Now;
                        var date = datetime.Date;

                        var sendEmailResponse = await notificationService.SendNotification(

                                            recipients,
                                            bcc,
                                            cc,
                                            request.uploadedFile,
                                           $"Release Document Sharing: {date.ToString("dd/MM/yyyy")} - {businessProfile.CompanyName} x {entitiesConcat}",
                                            MailTextDocumentReleased,
                                            NotificationTypes.Email
                                        );
                        return Result.Success<string>("Document have been uploaded");

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SaveDocumentReleasedCommand] {ex.Message}");
            }

            return Result.Failure<string>("Unable to upload document released.");
        }
    }
}
