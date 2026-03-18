using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.OTP;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.Services.Notification;

namespace Tranglo1.Onboarding.Application.Command
{
    public class RequestRBAOTPCommand : BaseCommand<Result<RBARequisitionOTPOutputDTO>>
    {
        public List<RBARequisitionOTPInputDTO> RequisitionCodes { get; set; }

        public string RequestID { get; set; }
        public long UserId { get; set; }

        internal class RequestRBAOTPCommandHandler : IRequestHandler<RequestRBAOTPCommand, Result<RBARequisitionOTPOutputDTO>>
        {
            private readonly INotificationService _notificationService;
            private readonly IOtpRepository _otpRepository;
            private readonly IApplicationUserRepository _applicationUserRepository;
            private readonly ILogger<RequestRBAOTPCommandHandler> _logger;
            private readonly TrangloUserManager _userManager;

            public RequestRBAOTPCommandHandler(
                INotificationService notificationService,
                IOtpRepository otpRepository,
                IApplicationUserRepository applicationUserRepository,
                ILogger<RequestRBAOTPCommandHandler> logger,
                 TrangloUserManager userManager
                )
            {
                _notificationService = notificationService;
                _otpRepository = otpRepository;
                _applicationUserRepository = applicationUserRepository;
                _logger = logger;
                _userManager = userManager;
            }

            public async Task<Result<RBARequisitionOTPOutputDTO>> Handle(RequestRBAOTPCommand request, CancellationToken cancellationToken)
            {

                var randNum = System.Security.Cryptography.RandomNumberGenerator.GetInt32(999999).ToString().PadLeft(6, '0');
                var requisitionOTPGroupId = Guid.NewGuid();
                string requisitionCodeConcat = "";

                bool isFirstLoop = true;

                foreach (var req in request.RequisitionCodes)
                {
                    RequisitionOTP otp = new RequisitionOTP();
                    otp.OTP = randNum;
                    otp.RequestID = "";
                    otp.RequisitionOTPGroupId = requisitionOTPGroupId;
                    otp.RequisitionCode = req.RequisitionCode;

                    if (isFirstLoop)
                    {
                        requisitionCodeConcat = req.RequisitionCode;
                    }
                    else
                    {
                        requisitionCodeConcat += String.Concat(",", req.RequisitionCode);
                    }

                    await _otpRepository.NewRequisitionOTPAsync(otp);

                    isFirstLoop = false;
                }

                ApplicationUser applicationUser = await _applicationUserRepository.GetApplicationUserByUserId(request.UserId);

                var recipients = new List<RecipientsInputDTO>()
                {
                new RecipientsInputDTO
                 {
                    // insert email and name here
                        email = applicationUser.Email.Value,
                        name = applicationUser.FullName.Value
                 }
                 };
                var content = $"Approval Code: {randNum} <br> This code is only valid for 5 minutes";
                var timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                string subject = $"Requisition Approval Code for {requisitionCodeConcat} {timestamp}";
                var cc = new List<RecipientsInputDTO>();
                var bcc = new List<RecipientsInputDTO>();
                var sendEmailResponse = await _notificationService.SendNotification(
                                                recipients, cc, bcc,
                                                new List<IFormFile>() { },
                                                subject, content,
                                                NotificationTypes.Email);

                RBARequisitionOTPOutputDTO requisitionOTPOutputDTO = new RBARequisitionOTPOutputDTO()
                {
                    RequisitionGroupId = requisitionOTPGroupId
                };

                if (sendEmailResponse.IsFailure)
                {
                    _logger.LogError($"[RequestFeeOTPCommand] Send Email Response Failed: {sendEmailResponse.Error}.");
                    return Result.Failure<RBARequisitionOTPOutputDTO>("There is an issue in sending the OTP to the desired email. Please contact administrator.");
                }

                return Result.Success(requisitionOTPOutputDTO);

            }
        }
    }
}

