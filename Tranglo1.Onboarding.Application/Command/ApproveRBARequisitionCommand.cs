using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    
    public class ApproveRBARequisitionCommand : BaseCommand<Result<List<ApproveRBARequisitionOutputDTO>>>
    {
        public int UserId { get; set; }
        public ApproveRBARequisitionInputDTO ApproveRBARequisitionInputDTO { get; set; }
        public string LoginId { get; set; }
       

        internal class ApproveRBARequisitionCommandHandler : IRequestHandler<ApproveRBARequisitionCommand, Result<List<ApproveRBARequisitionOutputDTO>>>
        {
       
            private readonly ApprovalManager<RBARequisition> _approvalManager;
            private readonly IOtpRepository _otpRepository;
            private readonly TrangloUserManager _userManager;
            private readonly IRBARepository _rbaRepository;

            public ApproveRBARequisitionCommandHandler(
                ApprovalManager<RBARequisition> approvalManager,
                IOtpRepository otpRepository,
                TrangloUserManager trangloUserManager,
                IRBARepository rbaRepository
              )
            {
               
                _approvalManager = approvalManager;
                _otpRepository = otpRepository;
                _userManager = trangloUserManager;
                _rbaRepository = rbaRepository;

            }

            public async Task<Result<List<ApproveRBARequisitionOutputDTO>>> Handle(ApproveRBARequisitionCommand request, CancellationToken cancellationToken)
            {
                try
                {


                    var otpValidate = await _otpRepository.ValidateOTPAsync(new Domain.Entities.OTP.RequisitionOTP
                    {
                        RequestID = "",
                        RequisitionOTPGroupId = request.ApproveRBARequisitionInputDTO.RequisitionGroupId,
                        OTP = request.ApproveRBARequisitionInputDTO.OTP
                    }, request.UserId);

                    if (!otpValidate)
                    {
                        return Result.Failure<List<ApproveRBARequisitionOutputDTO>>("This code is invalid, please request a new one");
                    }

                    List<ApproveRBARequisitionOutputDTO> outputDTO = new List<ApproveRBARequisitionOutputDTO>();
                    foreach (var rba in request.ApproveRBARequisitionInputDTO.Requisitions)
                        {
                            var rbaRequisition = await _rbaRepository.GetRBARequisitionsByRequisitionCodeAsync(rba.RequisitionCode);

                            //check requisition by rbacode

                            if (rbaRequisition != null)
                            {
                                var rbaCode = rbaRequisition.RBACode;
                                var previousRiskRankingRecord = await _rbaRepository.GetLatestRBARequisitionsByRBACodeAsync(rbaCode);
                                ;

                                if (previousRiskRankingRecord.PreviousRiskRanking is null)
                                {
                                    var rbaRecord = await _rbaRepository.GetRBAByRBACodeAsync(rbaCode);
                                    rbaRequisition.PreviousRiskRanking = rbaRecord.RiskRanking;
                                    var updateRequisition = await _approvalManager.EditRequisition(rbaRequisition);

                                }
                                else
                                {
                                    var prevRecord = await _rbaRepository.GetPreviousRBARequisitionByRBACodeAsync(rbaCode);
                                    var rbaRequisitiontoUpdate = await _rbaRepository.GetRBARequisitionsByRequisitionCodeAsync(rba.RequisitionCode);
                                    rbaRequisitiontoUpdate.PreviousRiskRanking = prevRecord.RiskRanking;
                                    var updateRequisitionResult = await _approvalManager.EditRequisition(rbaRequisitiontoUpdate);
                                }

                                var approveUpdateComplianceInternalRiskRequisition = await _approvalManager.ApproveAsync(rbaRequisition);

                                if (approveUpdateComplianceInternalRiskRequisition.IsFailure())
                                {
                                    return Result.Failure<List<ApproveRBARequisitionOutputDTO>>(approveUpdateComplianceInternalRiskRequisition.Error);
                                }

                                if (approveUpdateComplianceInternalRiskRequisition.RequisitionStatus == ApprovalWorkflowEngine.Enum.RequisitionStatus.Completed)
                                {

                                var dto = new ApproveRBARequisitionOutputDTO();
                                dto.RequisitionCode = rba.RequisitionCode;
                                outputDTO.Add(dto);

                                var rbaDetail = await _rbaRepository.GetRBAByRBACodeAsync(rbaRequisition.RBACode);
                                    rbaDetail.RiskRanking = rbaRequisition.RiskRanking;
                                    var updateResult = await _rbaRepository.UpdateRBAAsync(rbaDetail);
                                    if (updateResult == null)
                                    {
                                        return Result.Failure<List<ApproveRBARequisitionOutputDTO>>($"Failed to approve update compliance internal risk rating for RequisitionCode: {rba.RequisitionCode}.");
                                    }
                                }

                            }
                            else
                            {
                                return Result.Failure<List<ApproveRBARequisitionOutputDTO>>($"Update Compliance Internal Risk Rating Requisition Not Found for RequisitionCode: {rba.RequisitionCode}.");
                            }

                        }

                        return Result.Success(outputDTO);
                    
                }
                catch (Exception ex)
                {

                    return Result.Failure<List<ApproveRBARequisitionOutputDTO>>($"Approve Update Compliance Internal Risk Rating Requisition Failed.");
                }
            }
        }
    }
}

