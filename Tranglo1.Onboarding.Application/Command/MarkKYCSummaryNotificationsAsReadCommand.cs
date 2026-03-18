using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.KYCCustomerSummaryFeedbackNotification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementReviewSummary.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class MarkKYCSummaryNotificationsAsReadCommand : BaseCommand<Result<MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public int? AdminSolution { get; set; }

        internal class MarkKYCSummaryNotificationsAsReadCommandHandler : IRequestHandler<MarkKYCSummaryNotificationsAsReadCommand, Result<MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO>>
        {
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly ILogger<MarkKYCSummaryNotificationsAsReadCommandHandler> _logger;

            public MarkKYCSummaryNotificationsAsReadCommandHandler(IBusinessProfileRepository businessProfileRepository,
                ILogger<MarkKYCSummaryNotificationsAsReadCommandHandler> logger)
            {
                _businessProfileRepository = businessProfileRepository;
                _logger = logger;
            }

            public async Task<Result<MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO>> Handle(MarkKYCSummaryNotificationsAsReadCommand request, CancellationToken cancellationToken)
            {
                Specification<KYCCustomerSummaryFeedbackNotification> spec = new UnreadKYCCustomerSummaryFeedbackNotificationByBusinessProfile(request.BusinessProfileCode, request.AdminSolution ?? 0);
                MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO response = new MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO();

                try
                {
                    await _businessProfileRepository.UpdateKYCCustomerSummaryFeedbackNotificationsAsReadAsync(spec, cancellationToken);
                    response.IsSuccess = true;

                    return Result.Success(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{0}]", nameof(MarkKYCSummaryNotificationsAsReadCommandHandler));
                    return Result.Failure<MarkKYCCustomerSummaryFeedbackNotificationsAsReadOutputDTO>("Failed to mark KYC Customer Summary Notification(s) as read.");
                }
            }
        }
    }
}
