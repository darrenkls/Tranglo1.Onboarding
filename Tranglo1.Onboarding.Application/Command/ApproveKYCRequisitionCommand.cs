using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.Requisition;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCPartnerKYCApprovalList, UACAction.Approve)]
    [Permission(Permission.KYCManagementPartnerKYCApprovalList.Action_Approval_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementPartnerKYCApprovalList.Action_View_Code })]
    public class ApproveKYCRequisitionCommand : BaseCommand<Result<ApprovalWorkflowResult>>
    {
        public int UserId { get; set; }
        public string EntityCode { get; set; }
        public KYCRequisitionApproveInputDTO KYCRequisitionApproveInputDTO { get; set; }
        public string LoginId { get; set; }
        public int AdminSolution { get; set; }
        public override Task<string> GetAuditLogAsync(Result<ApprovalWorkflowResult> result)
        {
            if (result.IsSuccess)
            {
                return Task.FromResult<string>("Approved KYC case");
            }

            return base.GetAuditLogAsync(result);
        }

        internal class ApproveKYCRequisitionCommandHandler : IRequestHandler<ApproveKYCRequisitionCommand, Result<ApprovalWorkflowResult>>
        {
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly ApprovalManager<PartnerKYCStatusRequisition> _approvalManager;
            private readonly IOtpRepository _otpRepository;
            private readonly IWebHostEnvironment _environment;
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly TrangloUserManager _userManager;
            private readonly IPartnerRepository _partnerRepository;
            private readonly PartnerService _partnerService;

            public ApproveKYCRequisitionCommandHandler(IBusinessProfileRepository businessProfileRepository,
                ApprovalManager<PartnerKYCStatusRequisition> approvalManager,
                BusinessProfileService businessProfileService,
                IOtpRepository otpRepository,
                IWebHostEnvironment environment,
                INotificationService notification,
                IConfiguration configuration,
                TrangloUserManager trangloUserManager,
                IPartnerRepository partnerRepository,
                PartnerService partnerService
                )
            {
                _businessProfileRepository = businessProfileRepository;
                _approvalManager = approvalManager;
                _businessProfileService = businessProfileService;
                _otpRepository = otpRepository;
                _environment = environment;
                _notificationService = notification;
                _config = configuration;
                _userManager = trangloUserManager;
                _partnerRepository = partnerRepository;
                _partnerService = partnerService;
            }

            public async Task<Result<ApprovalWorkflowResult>> Handle(ApproveKYCRequisitionCommand request, CancellationToken cancellationToken)
            {
                var requsition = await _approvalManager.GetRequisitionByCodeAsync(request.KYCRequisitionApproveInputDTO.RequisitionCode);
                var getBusinessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(requsition.BusinessProfileCode);
                var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(getBusinessProfile.Id);
                var partnerSubscriptions = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerProfile.Id);
                var isPartnerConnect = partnerSubscriptions.FindAll(x => x.Solution == Solution.Connect);
                var isPartnerBusiness = partnerSubscriptions.FindAll(x => x.Solution == Solution.Business);
                var partnerSubInfo = await _partnerRepository.GetPartnerSubcriptionByPartnerCodeAndSolutionCodeAsync(partnerProfile.Id, request.AdminSolution);
                var partnerSubList = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerProfile.Id);
                KYCSubmissionStatus kYCSubmissionStatus = new KYCSubmissionStatus();

                CollectionTier businessCollectionTier = null;
                if (request.AdminSolution == Solution.Connect.Id)
                {
                    businessCollectionTier = null;
                }
                else if (request.AdminSolution == Solution.Business.Id)
                {
                    businessCollectionTier = Enumeration.FindById<CollectionTier>(getBusinessProfile.CollectionTier.Id);
                }

                PartnerType partnerType = null;
                if (partnerSubscriptions.Exists(x => x.PartnerType == PartnerType.Sales_Partner))
                {
                    partnerType = PartnerType.Sales_Partner;
                }
                else if (partnerSubscriptions.All(x => x.PartnerType == PartnerType.Supply_Partner))
                {
                    partnerType = PartnerType.Supply_Partner;
                }

                //var partnerType = Enumeration.FindById<PartnerType>(partnerProfile.PartnerTypeCode.GetValueOrDefault());
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

                if (applicationUser is TrangloStaff && partnerType == null)
                {
                    return Result.Failure<ApprovalWorkflowResult>(
                                            $"PartnerType is NULL for {getBusinessProfile.Id}");
                }
                // some code at need to be call there PostKYCStatusReviews
                //_requisitionUserIdentityContext.RoleCode = request.RoleCode;
                var otpValidate = await _otpRepository.ValidateOTPAsync(new Domain.Entities.OTP.RequisitionOTP
                {
                    RequestID = "",
                    RequisitionCode = request.KYCRequisitionApproveInputDTO.RequisitionCode,
                    OTP = request.KYCRequisitionApproveInputDTO.OTP
                }, request.UserId);

                if (!otpValidate)
                {
                    return Result.Failure<ApprovalWorkflowResult>("This code is invalid, please request a new one");
                }

                if (!partnerSubscriptions.Exists(x => x.TrangloEntity == null))
                {
                    //if (!partnerProfile.TrangloEntity.ToUpper().Equals(request.EntityCode.ToUpper()))
                    if (!partnerSubscriptions.Exists(x => x.TrangloEntity == request.EntityCode))
                    {
                        return Result.Failure<ApprovalWorkflowResult>("Cannot Approve Other Entity Requisition");
                    }
                }

                if (requsition.KYCStatusCode == KYCStatus.Pending_Higher_Approval.Id)
                {
                    _approvalManager.ApprovalConfiguration.MaxLevelRequired = 2;
                }
                else
                {
                    _approvalManager.ApprovalConfiguration.MaxLevelRequired = 1;
                }

                var result = await _approvalManager.ApproveAsync(requsition, request.KYCRequisitionApproveInputDTO.Remarks);

                if (result.IsFailure())
                {
                    return Result.Failure<ApprovalWorkflowResult>(result.Error);
                }
                else if (result.RequisitionStatus == ApprovalWorkflowEngine.Enum.RequisitionStatus.Completed)
                {
                    var kycStatus = Enumeration.FindById<KYCStatus>(requsition.KYCStatusCode);
                    var solution = string.Empty;
                    string approveReviewTemplate = string.Empty;

                    if (kycStatus == KYCStatus.Pending_Higher_Approval)
                    {
                        kycStatus = KYCStatus.Verified;
                    }

                    if (request.AdminSolution == Solution.Connect.Id && request.AdminSolution == Solution.Business.Id)
                    {
                        solution = "Tranglo Connect + Tranglo Business";
                    }
                    else if (request.AdminSolution == Solution.Connect.Id)
                    {
                        solution = "Tranglo Connect";
                        kYCSubmissionStatus = Enumeration.FindById<KYCSubmissionStatus>(getBusinessProfile.KYCSubmissionStatus.Id);
                        getBusinessProfile.AssignKYCStatus(kycStatus, partnerType, request.AdminSolution, kYCSubmissionStatus);
                        approveReviewTemplate = "ApproveReviewResultTemplate";
                    }
                    else if (request.AdminSolution == Solution.Business.Id)
                    {
                        solution = "Tranglo Business";
                        kYCSubmissionStatus = Enumeration.FindById<KYCSubmissionStatus>(getBusinessProfile.BusinessKYCSubmissionStatus.Id);
                        getBusinessProfile.AssignBusinessKYCStatus(kycStatus, partnerType, request.AdminSolution, kYCSubmissionStatus, businessCollectionTier);
                        approveReviewTemplate = "BusinessApproveReviewResultTemplate";
                    }

                    var update = await _businessProfileService.UpdateBusinessProfileAsync(getBusinessProfile);

                    if (update == null)
                    {
                        return Result.Failure<ApprovalWorkflowResult>(
                                    $"Update Business Profile failed for {requsition.BusinessProfileCode}."
                                );
                    }

                    //Golive if KYC is Verified
                    if (requsition.KYCStatusCode == 2)
                    {
                        //var getPartnerOnboarding = await _partnerRepository.GetPartnerOnboardingEventAsync(partnerSubInfo.Id);
                        Result<PartnerProfileChangedEvent> createPartnerOnboarding = new Result<PartnerProfileChangedEvent>();

                        var loginURL = request.AdminSolution == Solution.Business.Id
                            ? _config.GetValue<string>("BusinessPortalUri")
                            : _config.GetValue<string>("ConnectPortalUri");
                        foreach (var s in partnerSubList)
                        {
                            if (s.Solution == Solution.Business)
                            {
                                if (s.Environment != Domain.Entities.Environment.Production) //To prevent entity tracking issue
                                {
                                    // Partner Environment change to Production once GOLive
                                    s.SetBusinessEnvironment();
                                    await _partnerRepository.UpdatePartnerSubscriptionsAsync(s);
                                }

                                var trangloEntity = s.TrangloEntity;
                                var partnerTypeCode = s.PartnerType.Id;
                                var businessNatureDescription = getBusinessProfile.BusinessNature.Name;
                                Result<ContactNumber> createContactNumber = string.IsNullOrWhiteSpace(getBusinessProfile.ContactNumber.Value) || string.IsNullOrWhiteSpace(getBusinessProfile.ContactNumber.DialCode) ? null
                                    : ContactNumber.Create(getBusinessProfile.ContactNumber.DialCode, getBusinessProfile.ContactNumber.CountryISO2Code, getBusinessProfile.ContactNumber.Value);

                                var emailResult = Email.Create(partnerProfile.Email.Value);
                                if (getBusinessProfile.BusinessNature == BusinessNature.Other)
                                {
                                    businessNatureDescription = getBusinessProfile.ForOthers;
                                }

                                var partnerOnboarding = new PartnerProfileChangedEvent(trangloEntity, request.AdminSolution, partnerTypeCode, partnerProfile.Id, partnerProfile.PartnerId, s.Id, partnerSubInfo.SettlementCurrencyCode, getBusinessProfile.CompanyRegisteredCountryMeta.CountryISO2,
                                    getBusinessProfile.CompanyName, getBusinessProfile.CompanyRegistrationNo, getBusinessProfile.CompanyRegisteredAddress, getBusinessProfile.IDExpiryDate, getBusinessProfile.DateOfBirth, createContactNumber.Value, emailResult.Value,
                                    getBusinessProfile.CompanyRegisteredZipCodePostCode, getBusinessProfile.BusinessNature.Id, businessNatureDescription, s.Environment.Id, getBusinessProfile.DateOfIncorporation,
                                    getBusinessProfile.IncorporationCompanyTypeCode, partnerProfile.CustomerTypeCode, getBusinessProfile.CollectionTier.Id, getBusinessProfile.Id);
                                createPartnerOnboarding = await _partnerRepository.AddPartnerOnboardingCreationEventAsync(partnerOnboarding);

                                if (createPartnerOnboarding.IsFailure)
                                {
                                    return Result.Failure<ApprovalWorkflowResult>($"Unable to create Partner Onboarding Event");

                                }
                            }
                        }

                        var recipients = new List<RecipientsInputDTO>
                        {
                            new RecipientsInputDTO()
                            {
                                email = partnerProfile.Email.Value,
                                name = getBusinessProfile.CompanyName
                            }
                        };

                        var cc = new List<RecipientsInputDTO>();

                        var bcc = new List<RecipientsInputDTO>();

                        EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                        emailNotificationInputDTO.recipients = recipients;
                        emailNotificationInputDTO.cc = cc;
                        emailNotificationInputDTO.bcc = bcc;
                        emailNotificationInputDTO.LoginUrl = loginURL;
                        emailNotificationInputDTO.SolutionName = solution;
                        emailNotificationInputDTO.RecipientName = getBusinessProfile.CompanyName;
                        emailNotificationInputDTO.NotificationTemplate = !String.IsNullOrEmpty(approveReviewTemplate)
                            ? approveReviewTemplate
                            : "ApproveReviewResultTemplate";
                        emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                        emailNotificationInputDTO.subject = $"({solution}) Your KYC has been approved - {getBusinessProfile.CompanyName}";

                        var sendInviteUserNewUserEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);
                    }
                }
                else if (result.RequisitionStatus == ApprovalWorkflowEngine.Enum.RequisitionStatus.InProgress)
                {
                    // Send L2 request approval email
                    // 1. get the email template
                    //var xsltTemplateRootPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "\\wwwroot\\templates\\emailtemplate";
                    var xsltTemplateRootPath = Path.Combine(_environment.ContentRootPath, "templates/emailtemplate");

                    var pendingHigherSubjectTemplate = "";
                    var pendingHigherTemplate = "";
                    if (request.AdminSolution == Solution.Connect.Id)
                    {
                        pendingHigherSubjectTemplate = $"Pending Higher Approval {getBusinessProfile.CompanyName} x {request.EntityCode}";
                        pendingHigherTemplate = "PartnerKYCRequisitionTemplate";
                    }
                    else if (request.AdminSolution == Solution.Business.Id)
                    {
                        pendingHigherSubjectTemplate = $"(Tranglo Business) Pending Higher Approval {getBusinessProfile.CompanyName} x {request.EntityCode}";
                        pendingHigherTemplate = "BusinessPartnerKYCRequisitionTemplate";
                    }
                    string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                    var generator = new Common.ContentGenerator(xsltTemplateRootPath);

                    // 2. Use xsl to inject the properties from @event
                    StringBuilder _xml = new StringBuilder();
                    using (XmlWriter writer = XmlWriter.Create(_xml))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Profile");
                        writer.WriteElementString("FullName", "Head of Compliance");
                        writer.WriteElementString("PartnerName", getBusinessProfile.CompanyName);
                        writer.WriteElementString("LoginUrl", _config.GetValue<string>("IntranetUri"));
                        writer.WriteElementString("CurrentYear", "&copy; " + DateTime.Today.Year.ToString());
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    string content = generator.GenerateContent(_xml.ToString(), pendingHigherTemplate, cultureName);
                    //bool isProduction = _environment.EnvironmentName.ToLower() == Environments.Production.ToLower();
                    long authorityLevelCode = 2;
                    var recipients = new List<RecipientsInputDTO>();
                    var cc = new List<RecipientsInputDTO>();
                    var bcc = new List<RecipientsInputDTO>();
                    long notificationTemplate = 26;
                    var recipientInfo = await _businessProfileRepository.GetRecipientEmailByAuthorityLevel(authorityLevelCode, 1, notificationTemplate);
                    var ccInfo = await _businessProfileRepository.GetRecipientEmail(2, notificationTemplate);

                    foreach (var emailist in recipientInfo)
                    {
                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = emailist.Email,
                            name = emailist.Name
                        };
                        recipients.Add(recipientlist);
                    }
                    foreach (var cclist in ccInfo)
                    {
                        var recipientlist = new RecipientsInputDTO()
                        {
                            email = cclist.Email,
                            name = cclist.Name
                        };
                        cc.Add(recipientlist);
                    }
                    var sendEmailResponse = await _notificationService.SendNotification(
                                                    recipients, bcc, cc,
                                                    new List<IFormFile>() { },
                                                    pendingHigherSubjectTemplate, content,
                                                    NotificationTypes.Email);
                }
                return Result.Success(result);
            }
        }
    }
}