using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAccountStatus, UACAction.Edit)]
    [Permission(Permission.ManagePartnerAccountStatus.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerAccountStatus.Action_View_Code })]
    class SavePartnerAccountStatusCommand : BaseCommand<Result>
    {
        public long PartnerCode { get; set; }
        public long PartnerSusbscriptionCode { get; set; }
        public PartnerAccountStatusInputDTO PartnerAccStatus { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Edited partner account status";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePartnerAccountStatusCommandHandler : IRequestHandler<SavePartnerAccountStatusCommand, Result>
    {
        private readonly IMapper _mapper;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<SavePartnerAccountStatusCommandHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IConfiguration _configuration;

        public SavePartnerAccountStatusCommandHandler(IMapper mapper,
            ILogger<SavePartnerAccountStatusCommandHandler> logger,
            IPartnerRepository partnerRepository,
            INotificationService notificationService,
            IBusinessProfileRepository businessProfileRepository,
            BusinessProfileService businessProfileService,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _partnerRepository = partnerRepository;
            _logger = logger;
            _notificationService = notificationService;
            _businessProfileRepository = businessProfileRepository;
            _businessProfileService = businessProfileService;
            _configuration = configuration;
        }

        public async Task<Result> Handle(SavePartnerAccountStatusCommand request, CancellationToken cancellationToken)
        {
            var inputPartnerAccountStatus = request.PartnerAccStatus;

            var changeType = await _partnerRepository.GetChangeTypeAsync(inputPartnerAccountStatus.ChangeType); ;
            if (changeType == null)
            {
                return Result.Failure($"Change Type {inputPartnerAccountStatus.ChangeType} is not found.");
            }

            var accountStatus = await _partnerRepository.GetPartnerAccountStatusTypeAsync(inputPartnerAccountStatus.Status);
            if (accountStatus == null)
            {
                return Result.Failure($"Account Status {inputPartnerAccountStatus.Status} is not found.");
            }

            var partner = await _partnerRepository.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
            var subscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSusbscriptionCode);

            if (partner == null)
            {
                return Result.Failure("Partner does not exist");
            }

            var previousPartnerAccountStatus = await _partnerRepository.GetLatestPartnerAccountStatusByPartnerSubscription(request.PartnerSusbscriptionCode);

            #region Cannot set to Reject when Partner KYC Approval Status is Approved
            if (inputPartnerAccountStatus.Status == PartnerAccountStatusType.Rejected.Id)
            {
                int? partnerKYCApprovalStatus = await GetPartnerKYCApprovalStatusByBusinessProfile(partner.BusinessProfileCode);

                if (partnerKYCApprovalStatus.HasValue)
                {
                    ApprovalWorkflowEngine.Enum.ApprovalStatus partnerKYCApprovalStatusEnum = (ApprovalWorkflowEngine.Enum.ApprovalStatus)partnerKYCApprovalStatus.Value;

                    if (partnerKYCApprovalStatusEnum == ApprovalWorkflowEngine.Enum.ApprovalStatus.Approved)
                    {
                        return Result.Failure($"Unable to set Partner Account Status to {PartnerAccountStatusType.Rejected.Name} as Partner KYC Approval Status is {ApprovalWorkflowEngine.Enum.ApprovalStatus.Approved}");
                    }
                }
            }
            #endregion

            partner.ChangePartnerAccountStatus(changeType, inputPartnerAccountStatus.Description, accountStatus, request.PartnerSusbscriptionCode);

            if (subscription != null)
            {
                subscription.PartnerAccountStatusType = accountStatus;
            }
            else
            {
                return Result.Failure("Subscription does not exist");
            }

            var updatePartner = await _partnerRepository.UpdatePartnerRegistrationAsync(partner);
            var updateSubscription = await _partnerRepository.UpdateSubcriptionAsync(subscription);

            if (updatePartner.IsFailure || updateSubscription.IsFailure)
            {
                _logger.LogError($"[SavePartnerAccountStatusCommand] {updatePartner.Error} + {updateSubscription.Error}");

                return Result.Failure<COInformation>(
                            $"Update Partner Status failed for PartnerCode: {request.PartnerCode}, " +
                            $"PartnerSubscriptionCode: {request.PartnerSusbscriptionCode}."
                        );
            }

            #region Send rejection email
            bool isRequireSentRejectionEmail = accountStatus == PartnerAccountStatusType.Rejected;

            if (isRequireSentRejectionEmail)
            {
                Result isSentEmailResult = await SendRejectionEmailAsync(partner, subscription, changeType);
                if (isSentEmailResult.IsFailure)
                {
                    return Result.Failure(isSentEmailResult.Error);
                }
            }
            #endregion

            #region Send reactivation email
            bool isRequireSentReactivationEmail = previousPartnerAccountStatus != null
                && (previousPartnerAccountStatus.PartnerAccountStatusType == PartnerAccountStatusType.Inactive
                    || previousPartnerAccountStatus.PartnerAccountStatusType == PartnerAccountStatusType.Rejected)
                && accountStatus.Id == PartnerAccountStatusType.Active.Id;

            if (isRequireSentReactivationEmail)
            {
                Result isSentEmailResult = await SendReactivationPartnerAccountEmailAsync(partner, subscription, changeType, previousPartnerAccountStatus);
                if (isSentEmailResult.IsFailure)
                {
                    return Result.Failure(isSentEmailResult.Error);
                }
            }
            #endregion

            return Result.Success();
        }

        private async Task<int?> GetPartnerKYCApprovalStatusByBusinessProfile(int businessProfileCode)
        {
            var _connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                return await connection.QueryFirstOrDefaultAsync<int?>(
                    "dbo.GetPartnerKYCApprovalStatusByBusinessProfile",
                    new
                    {
                        BusinessProfileCode = businessProfileCode
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        private async Task<Result> SendRejectionEmailAsync(PartnerRegistration partner,
            PartnerSubscription subscription,
            ChangeType changeType)
        {
            Solution solution = subscription.Solution;
            BusinessProfile businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(partner.BusinessProfileCode);
            string partnerName = businessProfile.CompanyName;

            NotificationTemplate notificationTemplate = NotificationTemplate.ManualRejectPartnerApplication;

            List<RecipientsInputDTO> recipients = new List<RecipientsInputDTO>
            {
                new RecipientsInputDTO
                {
                    email = partner.Email.Value,
                    name = partnerName
                }
            };
            List<RecipientsInputDTO> ccList = (await _businessProfileService.GetRecipientEmail(RecipientType.CC.Id, notificationTemplate.Id))
                .Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                .ToList();
            List<RecipientsInputDTO> bccList = (await _businessProfileService.GetRecipientEmail(RecipientType.BCC.Id, notificationTemplate.Id))
                .Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                .ToList();

            EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO
            {
                recipients = recipients,
                cc = ccList,
                bcc = bccList,
                subject = $"({solution.Name}) - Your application has been rejected",
                Module = "CustomerIdentity",
                SubModule = "Manual Reject Partner Application",
                NotificationType = NotificationTypes.Email,
                NotificationTemplate = NotificationTemplate.ManualRejectPartnerApplication.Name + "Template",
                SolutionName = solution.Name,
                CompanyName = partnerName,
                Entity = partner.TrangloEntity
            };
            var sendEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);
            if (sendEmailResponse.IsFailure)
            {
                Log.Information("Sending Email for {NotificationTemplate} is FAILED! [{Error}]", notificationTemplate.Name, sendEmailResponse.Error);
                return Result.Failure("Sending email for Manual Reject Partner Application is failed!");
            }

            return Result.Success();
        }

        private async Task<Result> SendReactivationPartnerAccountEmailAsync(PartnerRegistration partner,
            PartnerSubscription subscription,
            ChangeType changeType,
            PartnerAccountStatus previousPartnerAccountStatus)
        {

            Solution solution = subscription.Solution;
            BusinessProfile businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(partner.BusinessProfileCode);
            string partnerName = businessProfile.CompanyName;

            NotificationTemplate notificationTemplate = NotificationTemplate.ReactivatePartnerAccount;

            List<RecipientsInputDTO> recipients = new List<RecipientsInputDTO>
            {
                new RecipientsInputDTO
                {
                    email = partner.Email.Value,
                    name = partnerName
                }
            };
            List<RecipientsInputDTO> ccList = (await _businessProfileService.GetRecipientEmail(RecipientType.CC.Id, notificationTemplate.Id))
                .Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                .ToList();
            List<RecipientsInputDTO> bccList = (await _businessProfileService.GetRecipientEmail(RecipientType.BCC.Id, notificationTemplate.Id))
                .Select(x => new RecipientsInputDTO { email = x.Email, name = x.Name })
                .ToList();

            EmailNotificationInputDTO emailNotificationInputDTO = new EmailNotificationInputDTO
            {
                recipients = recipients,
                cc = ccList,
                bcc = bccList,
                subject = $"({solution.Name}) – Welcome back! Your account has been reactivated",
                Module = "CustomerIdentity",
                SubModule = "Reactivate Partner Account",
                NotificationType = NotificationTypes.Email,
                NotificationTemplate = notificationTemplate.Name + "Template",
                SolutionName = solution.Name,
                CompanyName = partnerName,
                Entity = partner.TrangloEntity
            };
            var sendEmailResponse = await _notificationService.SendNotification(emailNotificationInputDTO);
            if (sendEmailResponse.IsFailure)
            {
                Log.Information("Sending Email for {NotificationTemplate} is FAILED! [{Error}]", notificationTemplate.Name, sendEmailResponse.Error);
                return Result.Failure("Sending email for Reactivate Partner Account is failed!");
            }

            return Result.Success();
        }
    }
}
