using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.RBA;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class UpdateRBAComplianceInternalRiskCommand : BaseCommand<Result<UpdateRBAComplianceRatingOutputDTO>>
    {
        public int RbaCode { get; set; }
        public UpdateRBAComplianceRatingInputDTO inputDTO;

        public override Task<string> GetAuditLogAsync(Result<UpdateRBAComplianceRatingOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Compliance Rating";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdateRBAComplianceInternalRiskCommandHandler : IRequestHandler<UpdateRBAComplianceInternalRiskCommand, Result<UpdateRBAComplianceRatingOutputDTO>>
    {
        private readonly IRBARepository _rbarepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ApprovalManager<RBARequisition> _rbaApprovalManager;

        public UpdateRBAComplianceInternalRiskCommandHandler(
            IRBARepository rBARepository,
            IBusinessProfileRepository businessProfileRepository,
            ApprovalManager<RBARequisition> rbaApprovalManager
            )
        {
            _rbarepository = rBARepository;
            _businessProfileRepository = businessProfileRepository;
            _rbaApprovalManager = rbaApprovalManager;
        }

       
           
public async Task<Result<UpdateRBAComplianceRatingOutputDTO>> Handle(UpdateRBAComplianceInternalRiskCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var context = new ValidationContext(request.inputDTO, null, null);
                var results = new List<ValidationResult>();
                var riskRanking = Enumeration.FindById<RiskRanking>(request.inputDTO.RiskRankingCode);

                //Do validations on the input
                if (!Validator.TryValidateObject(request.inputDTO, context, results, true))
                {
                    var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
                    return Result.Failure<UpdateRBAComplianceRatingOutputDTO>($"Validation failed: {errors}");
                }

                var RbaDetails = await _rbarepository.GetRBAByRBACodeAsync(request.RbaCode);
                if (RbaDetails.RiskRanking == riskRanking.Name)
                {
                    return Result.Failure<UpdateRBAComplianceRatingOutputDTO>($"The RBA compliance rating update cannot proceed because the selected risk level matches the current risk level.");
                }

                var solution = Enumeration.FindById<Solution>(RbaDetails.Solution.Id);
                if (solution == null)
                    return Result.Failure<UpdateRBAComplianceRatingOutputDTO>($"Invalid Solution for RequisitionCode: {request.RbaCode} ");

                var existingRequisition = await _rbarepository.GetRBARequisitionsByRBACodeAsync(request.RbaCode);
                if (existingRequisition != null)
                {
                    var requisitionApprovalStatus = await _rbaApprovalManager.GetRequisitionHistoryByCodeAsync(existingRequisition.RequisitionCode);
                    if (requisitionApprovalStatus == null || !requisitionApprovalStatus.Any())
                    {
                        // Handle the case whern theres a pending requisition
                        return Result.Failure<UpdateRBAComplianceRatingOutputDTO>($"Unable to proceed as theres a pending requisition for RequisitionCode: {existingRequisition.RequisitionCode} ");
                    }
                    
                }

                // Requisition
                var runningNumberExists = await _businessProfileRepository.GetRequisitionRunningNumberLatest();
                var requisitionRunningNumber = new RequisitionRunningNumber();
                if (runningNumberExists == null)
                {
                    requisitionRunningNumber.Prefix = "IRR";
                    requisitionRunningNumber.RunningNumber = "000001";
                    requisitionRunningNumber = await _businessProfileRepository.AddRequisitionRunningNumber(requisitionRunningNumber);
                }
                else
                {
                    #region Running Number Increment
                    var rNum = Int64.Parse(runningNumberExists.RunningNumber);
                    rNum++;
                    var rNumString = rNum.ToString();
                    var rNumPadding = rNumString.PadLeft(6, '0');
                    runningNumberExists.RunningNumber = rNumPadding.ToString();
                    #endregion

                    requisitionRunningNumber = await _businessProfileRepository.UpdateRequisitionRunningNumber(runningNumberExists);
                }

           

                var requisition = new RBARequisition();

                requisition.RBACode = request.RbaCode;
                requisition.RiskRanking = riskRanking.Name;
                requisition.TrangloEntity = RbaDetails.TrangloEntity;
                requisition.Solution = RbaDetails.Solution;
                requisition.Remarks = request.inputDTO.Remarks;
                requisition.RequisitionCode = requisitionRunningNumber.Prefix + requisitionRunningNumber.RunningNumber;
                requisition.ComplianceRequisitionType = ComplianceRequisitionType.Update_Compliance_Internal_Risk_Rating;

                var requisitionResult = await _rbaApprovalManager.AddRequisition(requisition);

                var outputDTO = new UpdateRBAComplianceRatingOutputDTO();

                outputDTO.RequisitionCode = requisitionResult.RequisitionCode;

                return Result.Success(outputDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Result.Failure<UpdateRBAComplianceRatingOutputDTO>($"Update RBA Compliance Rating Failed : {ex.Message} ");
            }
        }
    }
}

    

             