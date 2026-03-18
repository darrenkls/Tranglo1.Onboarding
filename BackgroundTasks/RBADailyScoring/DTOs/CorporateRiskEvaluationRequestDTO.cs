using System;

namespace Tranglo1.RBADailyScoring.DTOs
{
    public class CorporateRiskEvaluationRequestDTO
    {
        public DateTime? IncorporationDate { get; set; }
        public string IncorporationCountry { get; set; }
        public long? IncorporationType { get; set; }
        public long? CustomerCategory { get; set; }
        public long? IndustrySector { get; set; }
        public string CollectionTier { get; set; }
        public bool IsPEP { get; set; }
        public bool EnforcementActionTakenByRegulator { get; set; }
    }
}
