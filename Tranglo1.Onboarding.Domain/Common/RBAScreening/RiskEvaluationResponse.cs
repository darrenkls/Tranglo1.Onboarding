using System;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Domain.Common.RBAScreening
{
    public class RiskEvaluationResponse
    {
        public Guid? ResultId { get; set; }
        public int RiskScore { get; set; }
        public string RiskRanking { get; set; }
        public string Platform { get; set; }
        public string EntityType { get; set; }
        public string PartnerType { get; set; }
        public List<EvaluationRulesResponse> EvaluationRules { get; set; }
        public List<OverridingRulesResponse> OverridingRules { get; set; }
    }

    public class EvaluationRulesResponse
    {
        public string Template { get; set; }
        public int? Score { get; set; }
        public string ActualValue { get; set; }
        public string CriticalRanking { get; set; }
        public EvaluationRulesParameterResponse Parameter { get; set; }
        public bool IsMatched { get; set; }
    }

    public class EvaluationRulesParameterResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class OverridingRulesResponse
    {
        public string Template { get; set; }
        public OverridingRulesParameterResponse Parameter { get; set; }
        public string ActualValue { get; set; }
        public bool IsMatched { get; set; }
    }

    public class OverridingRulesParameterResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
