using Microsoft.Extensions.Configuration;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.Onboarding.Domain.Repositories;
using MediatR;
using System.Threading;
using Tranglo1.Onboarding.Domain.Entities.OTP;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.EmailNotification;
using Microsoft.AspNetCore.Http;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCPartnerKYCApprovalList, UACAction.Approve)]
    public class RequestKYCApprovalOTPCommand : BaseCommand<Result<bool>>
    {
        public string RequisitionCode { get; set; }
        public string RequestID { get; set; }
        public string LoginId { get; set; }

        internal class RequestKYCApprovalOTPCommandHandler : IRequestHandler<RequestKYCApprovalOTPCommand, Result<bool>>
        {
            private readonly INotificationService _notificationService;
            private readonly IConfiguration _config;
            private readonly IWebHostEnvironment _environment;
            private readonly IOtpRepository _repository;
            private readonly TrangloUserManager _userManager;

            public RequestKYCApprovalOTPCommandHandler(
                INotificationService notificationService,
                IConfiguration configuration,
                IWebHostEnvironment environment,
                IOtpRepository repository,
                TrangloUserManager userManager
                )
            {
                _notificationService = notificationService;
                _config = configuration;
                _environment = environment;
                _repository = repository;
                _userManager = userManager;

            }

            public async Task<Result<bool>> Handle(RequestKYCApprovalOTPCommand request, CancellationToken cancellationToken)
            {
                RequisitionOTP otp = new RequisitionOTP();
                var randNum = System.Security.Cryptography.RandomNumberGenerator.GetInt32(999999);
                otp.OTP = randNum.ToString().PadLeft(6,'0');
                otp.RequisitionCode = request.RequisitionCode;
                otp.RequestID = "";
                await _repository.NewRequisitionOTPAsync(otp);
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

                var recipients = new List<RecipientsInputDTO>()
                {
                    new RecipientsInputDTO
                    {
                        email = applicationUser.Email.Value,
                        name = applicationUser.FullName.Value
                    }
                };
                //backlog didnt state need pretty UI for email 

                var content = $"Approval Code: {otp.OTP} <br> This code is only valid for 5 minutes";
                var timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                string subject = $"Requisition Approval Code for {otp.RequisitionCode} {timestamp}";
                var cc = new List<RecipientsInputDTO>();
                var bcc = new List<RecipientsInputDTO>();
                var sendEmailResponse = await _notificationService.SendNotification(
                                                recipients, cc, bcc,
                                                new List<IFormFile>() { },
                                                subject, content,
                                                NotificationTypes.Email);

                return Result.Success(true);

            }
        }
    }
}
