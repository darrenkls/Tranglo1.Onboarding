using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;

namespace Tranglo1.Onboarding.Application.DTO.RBA
{
    public class RBAResultsOutputDTO
    {
        public string CustomerName { get; set; }
        public long RBACode { get; set; }
        public DateTime? RBAScreeningDate { get; set; }
        public DateTime? DisplayDateReportRuns { get; set; }
        public Guid? ResultId { get; set; }
        public int? RiskScore { get; set; }
        public string RiskRanking { get; set; }
        public long RiskRankingCode { get; set; }
        public List<ResultDesc> rbaDetails { get; set; }
        public string TrangloEntity { get; set; }
        public string InternalRiskRanking { get; set; }
        public int? FinalApprovalStatus { get; set; }
        public string FinalApprovalStatusDescription { get; set; }
    }

    public class ResultDesc
    {
        public string TemplateDescription { get; set; }
        public int? Score { get; set; }
        public string ActualValue { get; set; }
        public string CriticalRanking { get; set; }
    }
}
