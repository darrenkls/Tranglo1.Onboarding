using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.Requisition;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCPartnerKYCApprovalList, UACAction.Approve)]
    [Permission(Permission.KYCManagementPartnerKYCApprovalList.Action_Approval_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementPartnerKYCApprovalList.Action_View_Code })]
    public class RejectKYCRequisitionCommand : BaseCommand<Result<ApprovalWorkflowResult>>
    {
        public KYCRequisitionRejectInputDTO KYCRequisitionRejectInputDTO { get; set; }
        public int UserId { get; set; }
        public string EntityCode { get; set; }
        public int AdminSolution { get; set; }
        public override Task<string> GetAuditLogAsync(Result<ApprovalWorkflowResult> result)
        {
            if (result.IsSuccess) 
            {
                return Task.FromResult<string>("Rejected KYC case");
            }

            return base.GetAuditLogAsync(result);
        }

        internal class RejectKYCRequisitionCommandHandler : IRequestHandler<RejectKYCRequisitionCommand, Result<ApprovalWorkflowResult>>
        {
            private readonly ApprovalManager<PartnerKYCStatusRequisition> _approvalManager;
            private readonly IOtpRepository _repository;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;

            public RejectKYCRequisitionCommandHandler(ApprovalManager<PartnerKYCStatusRequisition> approvalManager, 
                IOtpRepository repository,
                IBusinessProfileRepository businessProfileService,
                IPartnerRepository partnerRepository)
            {
                _approvalManager = approvalManager;
                _repository = repository;
                _businessProfileRepository = businessProfileService;
                _partnerRepository = partnerRepository;
            }

            public async Task<Result<ApprovalWorkflowResult>> Handle(RejectKYCRequisitionCommand request, CancellationToken cancellationToken)
            {
                var otpValidate = await _repository.ValidateOTPAsync(new Domain.Entities.OTP.RequisitionOTP
                {
                    RequestID = "",
                    RequisitionCode = request.KYCRequisitionRejectInputDTO.RequisitionCode,
                    OTP = request.KYCRequisitionRejectInputDTO.OTP
                }, request.UserId);

                if (!otpValidate)
                {
                    return Result.Failure<ApprovalWorkflowResult>("This code is invalid, please request a new one");
                }
                var requsition = await _approvalManager.GetRequisitionByCodeAsync(request.KYCRequisitionRejectInputDTO.RequisitionCode);
                var getBusinessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(requsition.BusinessProfileCode);
                var partnerProfile = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(getBusinessProfile.Id);
                var partnerSubscriptions = await _partnerRepository.GetSalesPartnerSubscriptionListAsync(partnerProfile.Id);

                if (!partnerSubscriptions.Exists(x => x.TrangloEntity == null))
                {
                    //if (!partnerProfile.TrangloEntity.ToUpper().Equals(request.EntityCode.ToUpper()))
                    if (!partnerSubscriptions.Exists(x => x.TrangloEntity == request.EntityCode))
                    {
                        return Result.Failure<ApprovalWorkflowResult>("Cannot Approve Other Entity Requisition");
                    }
                }

                var result = await _approvalManager.RejectAsync(requsition, request.KYCRequisitionRejectInputDTO.Remarks);

                if (result.IsFailure())
                {
                    return Result.Failure<ApprovalWorkflowResult>(result.Error);
                }
               
                return Result.Success(result);
            }
        }
    }
}
