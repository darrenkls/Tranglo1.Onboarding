using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation
{
    public class TransactionEvaluationAnswerChoice: Entity
    {
        public string AnswerChoiceDescription { get; set; }
        public TransactionEvaluationQuestion TransactionEvaluationQuestion { get; set; }
        public int CustomerTypeGroupCode { get; set; }
    }
}
