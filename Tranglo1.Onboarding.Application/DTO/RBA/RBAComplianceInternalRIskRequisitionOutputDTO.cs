using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class RBAComplianceInternalRIskRequisitionOutputDTO
    {
        public string RequisitionCode { get; set; }
        public long ComplianceRequisitionTypeCode { get; set; }
        public string ComplianceRequisitionTypeDescription { get; set; }
        public string TrangloEntity { get; set; }
        public int RequisitionStatus { get; set; }
        public string RequisitionStatusDescription { get; set; }
        public long SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public string CreatedBy { get; set; }
        public string Remarks { get; set; }
        public long RiskRankingCode { get; set; }
        public string RiskRanking { get; set; }
        public int BusinessProfileCode { get; set; }

    }
}
