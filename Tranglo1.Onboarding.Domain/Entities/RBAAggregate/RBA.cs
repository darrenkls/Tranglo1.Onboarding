using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.RBAAggregate
{
    public class RBA : Entity
    {
        public long? BusinessProfileCode { get; set; }
        public Guid? ResultId { get; set; }
        public int? RiskScore { get; set; }
        public string RiskRanking { get; set; }
        public Solution Solution { get; set; }
        public string RBAPlatformDescription { get; set; }  //follows Solution Code Description
        public ScreeningEntityType ScreeningEntityType { get; set; }
        public string RBAEntityType { get; set; } // follows ScreeningEntityType Description 
        public PartnerType PartnerType { get; set; }
        public string RBAPartnerType { get; set; } // follows Partner Type Description
        public List<EvaluationRules> EvaluationRules { get; set; }
        public List<OverridingRules> OverridingRules { get; set; }
        public DateTime? RBAScreeningDate { get; set; }
        public string TrangloEntity { get; set; }
    }
}
