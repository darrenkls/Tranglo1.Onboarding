using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation
{
    public class TransactionEvaluationInfo: Entity
    {
        public string TransactionEvaluationInfoDescription { get; set; }
        public int CustomerTypeGroupCode { get; set; }
    }
}
