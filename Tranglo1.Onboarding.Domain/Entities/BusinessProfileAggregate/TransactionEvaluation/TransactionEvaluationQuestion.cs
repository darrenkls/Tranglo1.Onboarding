using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation
{
    public class TransactionEvaluationQuestion: Entity
    {
        public string TransactionEvaluationQuestionDescription { get; set; }
        public int CustomerTypeGroupCode { get; set; }
        public TransactionEvaluationQuestionInputType TransactionEvaluationQuestionInputType { get; set; }
        public string MetaName { get; set; } //eg: country
        public int SequenceNo { get; set; }
        public bool IsOptional { get; set; }

    }
}
