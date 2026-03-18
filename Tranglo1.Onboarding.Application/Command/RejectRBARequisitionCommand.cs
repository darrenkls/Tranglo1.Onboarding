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
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.RBA;

namespace Tranglo1.Onboarding.Application.Command
{
    public class RejectRBARequisitionCommand : BaseCommand<Result<List<RejectRBARequisitionOutputDTO>>>
    {
        public int UserId { get; set; }
        public RejectRBARequisitionInputDTO RejectRBARequisitionInputDTO { get; set; }
        public string LoginId { get; set; }
     

        internal class RejectRBARequisitionCommandHandler : IRequestHandler<RejectRBARequisitionCommand, Result<List<RejectRBARequisitionOutputDTO>>>
        {
          
            private readonly ApprovalManager<RBARequisition> _approvalManager;
            private readonly IOtpRepository _otpRepository;
            private readonly TrangloUserManager _userManager;
            private readonly IRBARepository _rbaRepository;

            public RejectRBARequisitionCommandHandler(
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

            public async Task<Result<List<RejectRBARequisitionOutputDTO>>> Handle(RejectRBARequisitionCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var otpValidate = await _otpRepository.ValidateOTPAsync(new Domain.Entities.OTP.RequisitionOTP
                    {
                        RequestID = "",
                        RequisitionOTPGroupId = request.RejectRBARequisitionInputDTO.RequisitionGroupId,
                        OTP = request.RejectRBARequisitionInputDTO.OTP
                    }, request.UserId);

                    if (!otpValidate)
                    {
                        return Result.Failure<List<RejectRBARequisitionOutputDTO>>("This code is invalid, please request a new one");
                    }

                    List<RejectRBARequisitionOutputDTO> outputDTO = new List<RejectRBARequisitionOutputDTO>();
                    foreach (var rba in request.RejectRBARequisitionInputDTO.Requisitions)
                    {
                        var rbaRequisition = await _rbaRepository.GetRBARequisitionsByRequisitionCodeAsync(rba.RequisitionCode);

                        if (rbaRequisition != null)
                        {
                            var rbaCode = rbaRequisition.RBACode;
                            var previousRiskRankingRecord = await _rbaRepository.GetLatestRBARequisitionsByRBACodeAsync(rbaCode);

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
                            var rejectUpdateComplianceInternalRiskRequisition = await _approvalManager.RejectAsync(rbaRequisition);

                            if (rejectUpdateComplianceInternalRiskRequisition.IsFailure())
                            {
                                return Result.Failure<List<RejectRBARequisitionOutputDTO>>(rejectUpdateComplianceInternalRiskRequisition.Error);
                            }
                            var dto = new RejectRBARequisitionOutputDTO();
                            dto.RequisitionCode = rba.RequisitionCode;
                            outputDTO.Add(dto);
                           
                        }
                        else
                        {
                            return Result.Failure<List<RejectRBARequisitionOutputDTO>>($"Update Compliance Internal Ranking Rating Requisition Not Found for RequisitionCode: {rba.RequisitionCode}.");
                        }
                    }
                    return Result.Success(outputDTO);
                }
                catch (Exception ex)
                {

                    return Result.Failure<List<RejectRBARequisitionOutputDTO>>($"Reject Update Compliance Internal Ranking Rating Requisition Failed.");
                }
            }
        }
    }
}
