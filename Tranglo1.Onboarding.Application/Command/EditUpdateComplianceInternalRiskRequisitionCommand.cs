using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.RBA;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class EditUpdateComplianceInternalRiskRequisitionCommand : BaseCommand<Result<RequisitionEditResult<RBARequisition>>>
    {
        public string RequisitionCode { get; set; }
        public EditUpdateComplianceInternalRiskInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<RequisitionEditResult<RBARequisition>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Edit Update Compliance Internal Risk Requisition";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class EditUpdateComplianceInternalRiskRequisitionCommandHandler : IRequestHandler<EditUpdateComplianceInternalRiskRequisitionCommand, Result<RequisitionEditResult<RBARequisition>>>
    {
        private readonly ApprovalManager<RBARequisition> _rbaApprovalManager;
        private readonly IRBARepository _rbarepository;
      

        public EditUpdateComplianceInternalRiskRequisitionCommandHandler(
            ApprovalManager<RBARequisition> rbaApprovalManager,
           IRBARepository rBARepository
            
        )
        {
            _rbaApprovalManager = rbaApprovalManager;
            _rbarepository = rBARepository;
            
        }

        public async Task<Result<RequisitionEditResult<RBARequisition>>> Handle(EditUpdateComplianceInternalRiskRequisitionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var context = new ValidationContext(request.InputDTO, null, null);
                var results = new List<ValidationResult>();
                var riskRanking = Enumeration.FindById<RiskRanking>(request.InputDTO.RiskRankingCode);

                var getRequisition = await _rbaApprovalManager.GetRequisitionByCodeAsync(request.RequisitionCode);

                if (getRequisition == null)
                {
                    return Result.Failure<RequisitionEditResult<RBARequisition>>($"Edit Update Compliance Internal Risk does not exist for RequisitionCode: {request.RequisitionCode}.");
                };

                //Do validations on the input
                if (!Validator.TryValidateObject(request.InputDTO, context, results, true))
                {
                    var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
                    return Result.Failure<RequisitionEditResult<RBARequisition>>($"Validation failed: {errors}");
                }

                var RbaDetails = await _rbarepository.GetRBAByRBACodeAsync(getRequisition.RBACode);
                if (RbaDetails.RiskRanking == riskRanking.Name)
                {
                    return Result.Failure<RequisitionEditResult<RBARequisition>>($"The RBA compliance rating update cannot proceed because the selected risk level matches the current risk level.");
                }


                getRequisition.RiskRanking = riskRanking.Name;
                getRequisition.Remarks = request.InputDTO.Remarks;;
                var updateRequisition = await _rbaApprovalManager.EditRequisition(getRequisition);

                return updateRequisition;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Result.Failure<RequisitionEditResult<RBARequisition>>($"Edit Update Compliance Internal Risk Requisition Failed : {ex.Message} ");
            }
        }
    }
}
