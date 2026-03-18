using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.RBAAggregate
{
    public class OverridingRules : Entity
    {
        public RBA RBA { get; set; }
        public string Template { get; set; }
        public string ActualValue { get; set; }
        public bool? IsMatched { get; set; }
        public OverridingRulesParameter Parameter { get; set; }
    }
}
