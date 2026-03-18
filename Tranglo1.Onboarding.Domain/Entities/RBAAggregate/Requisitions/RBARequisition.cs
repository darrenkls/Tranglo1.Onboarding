using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.ApprovalWorkflowEngine.Models;

namespace Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions
{
    public class RBARequisition : RequisitionDetail
    {
        public long RBACode { get; set; }
        public ComplianceRequisitionType ComplianceRequisitionType { get; set; }
        public string TrangloEntity { get; set; }
        public Solution Solution { get; set; }
        public string Remarks { get; set; }
        public string RiskRanking { get; set; }
        public string PreviousRiskRanking { get; set; }
    }
}
