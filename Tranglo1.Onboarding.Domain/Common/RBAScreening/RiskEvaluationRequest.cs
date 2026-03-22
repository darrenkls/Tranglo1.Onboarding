using System;

namespace Tranglo1.Onboarding.Domain.Common.RBAScreening
{
    public class RiskEvaluationRequest
    {
        public long? IndustrySector { get; set; }
        public string CollectionTier { get; set; }
        public bool IsPEP { get; set; }
        public bool EnforcementActionTakenByRegulator { get; set; }
    }

    public class IndividualRiskEvaluationRequest : RiskEvaluationRequest
    {
        public string Nationality { get; set; }
    }

    public class CorporateRiskEvaluationRequest : RiskEvaluationRequest
    {
        public DateTime? IncorporationDate { get; set; }
        public string IncorporationCountry { get; set; }
        public long? IncorporationType { get; set; }
        public long? CustomerCategory { get; set; }
    }
}
