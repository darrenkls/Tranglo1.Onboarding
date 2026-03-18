using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCManagement, UACAction.Approve)]
    [Permission(Permission.KYCManagement.Action_NotifyUser_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class NotifyKYCUserCommand : BaseCommand<Result>
    {
        public int BusinessProfileCode { get; set; }
        public long KYCStatusCode { set; get; }
        public int AdminSolution { get; set; }
        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                return Task.FromResult("Notify User");
            }
            return base.GetAuditLogAsync(result);
        }


        public class UpdateKYCStatusCommandHandler : IRequestHandler<NotifyKYCUserCommand, Result>
        {
            private readonly IMapper _mapper;
            private readonly IApplicationUserRepository _applicationUserRepository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly ILogger<KYCStatusReviewsOutputDTO> _logger;
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly PartnerService _partnerService;
            private readonly IWebHostEnvironment _environment;

            public UpdateKYCStatusCommandHandler(IMapper mapper, IApplicationUserRepository applicationUserRepository, BusinessProfileService businessProfileService,
              IConfiguration config, PartnerService partnerService,
              INotificationService notificationService, ILogger<KYCStatusReviewsOutputDTO> logger, IBusinessProfileRepository businessProfileRepository,
              IWebHostEnvironment environment)
            {
                _mapper = mapper;
                _partnerService = partnerService;
                _applicationUserRepository = applicationUserRepository;
                _businessProfileService = businessProfileService;
                _notificationService = notificationService;
                _config = config;
                _logger = logger;
                _businessProfileRepository = businessProfileRepository;
                _environment = environment;
            }

            public async Task<Result> Handle(NotifyKYCUserCommand request, CancellationToken cancellationToken)
            {
                //check if adminSolution is null
                if (request.AdminSolution == 0)
                {
                    _logger.LogError($"[NotifyKYCUserCommand] Admin Solution is NULL ");
                    return Result.Failure("[NotifyKYCUserCommand] Tranglo Solution is NULL ");
                }

                var businessProfile = _businessProfileRepository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode).Result;

                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);

                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
                var trangloPortalUri = "";
                var insufficientReviewTemplate = "";
                var approveReviewTemplate = "";
                var SubjectTemplate = "";
                var WorkflowStatusEmail2 = "";
                var IncompleteSolution = "";
                var WorkflowStatusEmail1Url = "mailto:partnerships@tranglo.com";
                var WorkflowStatusEmail2Url = "";

                if (request.AdminSolution == Solution.Connect.Id)
                {
                    trangloPortalUri = "ConnectPortalUri";
                    insufficientReviewTemplate = "IncompleteReviewResultTemplate";
                    approveReviewTemplate = "ApproveReviewResultTemplate";
                    //SubjectTemplate = $"(Tranglo Connect) Action required: KYC review feedback for {businessProfile.CompanyName}";
                    WorkflowStatusEmail2 = "compliance@tranglo.com";
                    WorkflowStatusEmail2Url = "mailto:compliance@tranglo.com";
                    IncompleteSolution = "(Tranglo Connect)";
                }

                else if (request.AdminSolution == Solution.Business.Id)
                {
                    trangloPortalUri = "BusinessPortalUri";
                    insufficientReviewTemplate = "BusinessIncompleteReviewResultTemplate";
                    approveReviewTemplate = "BusinessApproveReviewResultTemplate";
                    //SubjectTemplate = $"(Tranglo Business) Action required: KYC review feedback for {businessProfile.CompanyName}";
                    IncompleteSolution = "(Tranglo Business)";


                    if (businessProfile.BusinessWorkflowStatus.Id <= 4)
                    {
                        WorkflowStatusEmail2 = "kyc@tranglo.com";
                        WorkflowStatusEmail2Url = "mailto:kyc@tranglo.com";
                    }
                    else if (businessProfile.BusinessWorkflowStatus.Id >= 5 && businessProfile.BusinessWorkflowStatus.Id <= 8)
                    {
                        WorkflowStatusEmail2 = "radinindra.ismail@tranglo.com";
                        WorkflowStatusEmail2Url = "mailto:radinindra.ismail@tranglo.com";
                    }
                }

                //call businessProfile
                var kYCStatus = KYCStatus.FindById<KYCStatus>(request.KYCStatusCode);

                if (bilateralPartnerFlow == null)
                {
                    return Result.Failure("Notify User is disabled for None Subscription");
                }

                if (request.AdminSolution == Solution.Connect.Id)
                {
                    if (bilateralPartnerFlow == PartnerType.Supply_Partner)
                    {
                        return Result.Failure("Notify User is disabled for Supply Partner");
                    }
                }

                // 3. Send the email
                if (kYCStatus.Id == KYCStatus.Insufficient_Incomplete.Id || kYCStatus.Id == KYCStatus.Verified.Id)
                {
                    long notificationTemplate = 9;
                    string finalTemplate = string.Empty;
                    string logMessage = string.Empty;

                    if (kYCStatus.Id == KYCStatus.Insufficient_Incomplete.Id)
                    {
                        finalTemplate = insufficientReviewTemplate;
                        notificationTemplate = 9;
                        logMessage = "Insufficient/Incomplete";
                        SubjectTemplate = $"{IncompleteSolution} Action required: KYC review feedback for {businessProfile.CompanyName}";
                    }
                    else if (kYCStatus.Id == KYCStatus.Verified.Id)
                    {
                        finalTemplate = approveReviewTemplate;
                        notificationTemplate = 6;
                        logMessage = "Approved";
                        SubjectTemplate = $"{IncompleteSolution} Your KYC has been approved - {businessProfile.CompanyName}";
                    }
                    var recipients = new List<RecipientsInputDTO>();

                    var recipientlist = new RecipientsInputDTO()
                    {
                        email = partnerRegistrationInfo.Email.Value,
                        name = businessProfile.ContactPersonName
                    };
                    recipients.Add(recipientlist);

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

                    EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO();
                    emailNotificationInputDTO.recipients = recipients;
                    emailNotificationInputDTO.cc = cc;
                    emailNotificationInputDTO.bcc = bcc;
                    emailNotificationInputDTO.RecipientName = businessProfile.CompanyName;
                    emailNotificationInputDTO.Email1 = "partnerships@tranglo.com";
                    emailNotificationInputDTO.Email1Url = WorkflowStatusEmail1Url;
                    emailNotificationInputDTO.Email2 = WorkflowStatusEmail2;
                    emailNotificationInputDTO.EmailUrl2 = WorkflowStatusEmail2Url;
                    emailNotificationInputDTO.SolutionName = IncompleteSolution;
                    emailNotificationInputDTO.LoginUrl = _config.GetValue<string>(trangloPortalUri);
                    emailNotificationInputDTO.NotificationTemplate = finalTemplate;
                    emailNotificationInputDTO.NotificationType = NotificationTypes.Email;
                    emailNotificationInputDTO.subject = SubjectTemplate;

                    var sendKYCNotificationEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);

                    if (sendKYCNotificationEmailResponse.IsFailure)
                    {
                        _logger.LogError("SendNotification", $"[NotifyKYCUserCommand] '{logMessage}' Notification failed for {businessProfile.ContactPersonName}. {sendKYCNotificationEmailResponse.Error}.");
                    }
                }

                return Result.Success(kYCStatus);
            }
        }
    }
}
