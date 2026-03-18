using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.ApprovalWorkflowEngine.Enum;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{

    internal class GetRBAComplianceInternalRiskRequisitionQuery : BaseQuery<Result<RBAComplianceInternalRIskRequisitionOutputDTO>>
    {
        public string RequisitionCode { get; set; }

        internal class GetRBAComplianceInternalRiskRequisitionQueryHandler : IRequestHandler<GetRBAComplianceInternalRiskRequisitionQuery, Result<RBAComplianceInternalRIskRequisitionOutputDTO>>
        {
            private readonly ApprovalManager<RBARequisition> _approvalManager;
            private readonly IRBARepository _rbarepository;

            public GetRBAComplianceInternalRiskRequisitionQueryHandler(ApprovalManager<RBARequisition> approvalManager,IRBARepository rBARepository)
            {
                _approvalManager = approvalManager;
                _rbarepository = rBARepository;
            }

            public async Task<Result<RBAComplianceInternalRIskRequisitionOutputDTO>> Handle(GetRBAComplianceInternalRiskRequisitionQuery request, CancellationToken cancellationToken)
            {
                var requisition = await _rbarepository.GetRBARequisitionsByRequisitionCodeAsync(request.RequisitionCode);

                var solution = Enumeration.FindById<Solution>(requisition.Solution.Id);
                if (solution == null)
                    return Result.Failure<RBAComplianceInternalRIskRequisitionOutputDTO>($"Invalid Solution for RequisitionCode: {request.RequisitionCode} ");

                var requisitionType = Enumeration.FindById<ComplianceRequisitionType>(requisition.ComplianceRequisitionType.Id);
                if (requisitionType == null)
                    return Result.Failure<RBAComplianceInternalRIskRequisitionOutputDTO>($"Invalid Solution for RequisitionCode: {request.RequisitionCode} ");

                string requisitionStatus = GetEnumNameById<RequisitionStatus>((int)requisition.RequisitionStatus);
                int requisitionStatusCode = (int)Enum.Parse(typeof(RequisitionStatus), requisition.RequisitionStatus.ToString());
                var riskRankingCode = Enumeration.FindByName<RiskRanking>(requisition.RiskRanking);
                var rbadetails = await _rbarepository.GetRBAByRBACodeAsync(requisition.RBACode);
                

                RBAComplianceInternalRIskRequisitionOutputDTO result = new RBAComplianceInternalRIskRequisitionOutputDTO();

                result.RequisitionCode = requisition.RequisitionCode;
                result.ComplianceRequisitionTypeCode = requisitionType.Id;
                result.ComplianceRequisitionTypeDescription = requisitionType.Name;
                result.TrangloEntity = requisition.TrangloEntity;
                result.SolutionCode = solution.Id;
                result.SolutionDescription = solution.Name;
                result.RequisitionStatus = requisitionStatusCode;
                result.RequisitionStatusDescription = requisitionStatus;
                result.Remarks = requisition.Remarks;
                result.CreatedBy = requisition.CreatedBy;
                result.RiskRanking = requisition.RiskRanking;
                result.RiskRankingCode = riskRankingCode.Id;
                result.BusinessProfileCode = int.Parse(rbadetails.BusinessProfileCode.Value.ToString());

                if (requisition == null)
                {
                    return Result.Failure<RBAComplianceInternalRIskRequisitionOutputDTO>("Requisition not found. Please ensure the requisition code is correct and try again.");
                }

                return Result.Success(result);
            }
        }
        private static string GetEnumNameById<TEnum>(int id) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), id);
        }

    }
}
